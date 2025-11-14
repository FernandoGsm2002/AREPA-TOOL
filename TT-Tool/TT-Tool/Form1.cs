using TT_Tool.Managers;
using TT_Tool.Brands;
using TT_Tool.Controls;

namespace TT_Tool
{
    public partial class Form1 : Form
    {
        // Managers
        private LogManager? _logManager;
        private DeviceManager? _deviceManager;

        // Brand Operations
        private AndroidOperations? _androidOps;
        private SamsungOperations? _samsungOps;
        private HonorOperations? _honorOps;
        private MotorolaOperations? _motorolaOps;
        // private QualcommOperations? _qualcommOps; // TODO: Implementar QualcommOperations

        // Dispositivo seleccionado
        private string? _dispositivoActual;
        private ModernButton? _botonActivo;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            // Inicializar managers primero
            _logManager = new LogManager(txtLogs);
            _logManager.ConfigurarControlesProgreso(progressBar, lblEstadoProgreso, btnCancelar);
            _deviceManager = new DeviceManager();
            
            // Cargar imágenes de los botones (después de inicializar logManager)
            CargarImagenesBotones();

            // ========================================
            // VERIFICACIÓN DE LICENCIA REMOTA
            // ========================================
            _logManager.AgregarLog("Verificando estado de la aplicación...", TipoLog.Info);
            
            var (isEnabled, message) = await LicenseManager.VerificarLicencia();
            
            if (!isEnabled)
            {
                // La aplicación está desactivada remotamente
                _logManager.AgregarLog("", TipoLog.Error);
                _logManager.AgregarLog("===============================================", TipoLog.Error);
                _logManager.AgregarLog("  APLICACIÓN DESACTIVADA", TipoLog.Error);
                _logManager.AgregarLog("===============================================", TipoLog.Error);
                _logManager.AgregarLog("", TipoLog.Error);
                _logManager.AgregarLog(message, TipoLog.Advertencia);
                
                MessageBox.Show(
                    message,
                    "AREPA-TOOL - Aplicación Desactivada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                
                // Cerrar la aplicación
                Application.Exit();
                return;
            }
            
            // Mostrar mensaje de bienvenida si existe
            if (!string.IsNullOrEmpty(message))
            {
                _logManager.AgregarLog(message, TipoLog.Exito);
                _logManager.AgregarLog("");
            }

            // Inicializar operaciones de marcas
            _androidOps = new AndroidOperations();
            _androidOps.SetLogManager(_logManager); // Pasar LogManager a Android
            
            _samsungOps = new SamsungOperations();
            _samsungOps.SetLogManager(_logManager); // Pasar LogManager a Samsung
            
            _honorOps = new HonorOperations();
            _honorOps.SetLogManager(_logManager); // Pasar LogManager a Honor
            
            _motorolaOps = new MotorolaOperations();
            _motorolaOps.SetLogManager(_logManager); // Pasar LogManager a Motorola
            
            // _qualcommOps = new QualcommOperations();
            // _qualcommOps.SetLogManager(_logManager); // Pasar LogManager a Xiaomi

            // Suscribirse a eventos
            _deviceManager.OnLogMessage += (s, mensaje) => _logManager?.AgregarLog(mensaje);

            // Logs iniciales
            _logManager.AgregarLog("AREPA-TOOL v1.0 By LeoPE-GSM.COM", TipoLog.Exito);
            _logManager.AgregarLog("Sistema iniciado correctamente", TipoLog.Exito);
            _logManager.AgregarLog("");

            // Cargar contenido inicial
            CambiarBotonActivo(btnAndroid);
            CargarPanel(_androidOps!);
        }

        private void CargarImagenesBotones()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Cargar imagen para Android
            CargarImagenBoton(btnAndroid, Path.Combine(baseDir, "Resources", "androidd.png"));

            // Cargar imagen para Samsung
            CargarImagenBoton(btnSamsung, Path.Combine(baseDir, "Resources", "samsung.png"));

            // Cargar imagen para Honor
            CargarImagenBoton(btnHonor, Path.Combine(baseDir, "Resources", "hhonor.png"));

            // Cargar imagen para Motorola
            CargarImagenBoton(btnMotorola, Path.Combine(baseDir, "Resources", "motorola.png"));

            // Cargar imagen para Qualcomm
            CargarImagenBoton(btnQualcomm, Path.Combine(baseDir, "Resources", "qualcomm.png"));
        }

        private void CargarImagenBoton(ModernButton boton, string rutaImagen)
        {
            try
            {
                if (File.Exists(rutaImagen))
                {
                    // Cargar imagen original
                    var imgOriginal = Image.FromFile(rutaImagen);
                    
                    // Calcular dimensiones manteniendo proporción
                    int targetWidth = boton.Width - 10;
                    int targetHeight = boton.Height - 10;
                    
                    // Aplicar zoom extra para Samsung y Motorola (aún más grande)
                    string nombreArchivo = Path.GetFileName(rutaImagen).ToLower();
                    float zoomExtra = 1.0f;
                    if (nombreArchivo.Contains("samsung") || nombreArchivo.Contains("motorola"))
                    {
                        zoomExtra = 2.8f; // 180% más grande para que se vean mucho mejor
                    }
                    
                    float ratioX = (float)targetWidth / imgOriginal.Width * zoomExtra;
                    float ratioY = (float)targetHeight / imgOriginal.Height * zoomExtra;
                    float ratio = Math.Min(ratioX, ratioY);
                    
                    int newWidth = (int)(imgOriginal.Width * ratio);
                    int newHeight = (int)(imgOriginal.Height * ratio);
                    
                    var imgRedimensionada = new Bitmap(imgOriginal, new Size(newWidth, newHeight));
                    
                    // Configurar el botón para mostrar solo la imagen (sin texto)
                    boton.Image = imgRedimensionada;
                    boton.Text = ""; // Eliminar texto
                    boton.ImageAlign = ContentAlignment.MiddleCenter;
                    
                    // IMPORTANTE: Forzar el repintado del botón
                    boton.Invalidate();
                    boton.Refresh();
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error al cargar imagen: {ex.Message}", TipoLog.Error);
            }
        }

        // ============= ESCANEO AUTOMÁTICO EN DROPBOX =============

        private async void CmbDispositivoADB_DropDown(object? sender, EventArgs e)
        {
            if (_deviceManager == null || _logManager == null) return;
            
            cmbDispositivoADB.Items.Clear();
            cmbDispositivoADB.Items.Add("⏳ Escaneando...");
            cmbDispositivoADB.SelectedIndex = 0;

            try
            {
                // DeviceManager ya envía sus propios logs
                var dispositivos = await _deviceManager.EscanearDispositivosADB();

                cmbDispositivoADB.Items.Clear();
                if (dispositivos.Count > 0)
                {
                    cmbDispositivoADB.Items.AddRange(dispositivos.ToArray());
                    cmbDispositivoADB.SelectedIndex = 0;
                }
                else
                {
                    cmbDispositivoADB.Items.Add("❌ No hay dispositivos");
                    cmbDispositivoADB.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                cmbDispositivoADB.Items.Clear();
                cmbDispositivoADB.Items.Add("❌ Error al escanear");
                cmbDispositivoADB.SelectedIndex = 0;
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private async void CmbDispositivoCOM_DropDown(object? sender, EventArgs e)
        {
            if (_deviceManager == null || _logManager == null) return;

            cmbDispositivoCOM.Items.Clear();
            cmbDispositivoCOM.Items.Add("⏳ Escaneando...");
            cmbDispositivoCOM.SelectedIndex = 0;

            try
            {
                _logManager.AgregarLog("Escaneando puertos COM...");
                
                await Task.Run(() =>
                {
                    var puertos = _deviceManager.EscanearPuertosCOM();
                    
                    Invoke(() =>
                    {
                        cmbDispositivoCOM.Items.Clear();
                        if (puertos.Count > 0)
                        {
                            cmbDispositivoCOM.Items.AddRange(puertos.ToArray());
                            cmbDispositivoCOM.SelectedIndex = 0;
                            _logManager.AgregarLog($"✓ {puertos.Count} puertos encontrados", TipoLog.Exito);
                        }
                        else
                        {
                            cmbDispositivoCOM.Items.Add("❌ No hay puertos COM");
                            cmbDispositivoCOM.SelectedIndex = 0;
                            _logManager.AgregarLog("⚠ No se encontraron puertos COM", TipoLog.Advertencia);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                cmbDispositivoCOM.Items.Clear();
                cmbDispositivoCOM.Items.Add("❌ Error al escanear");
                cmbDispositivoCOM.SelectedIndex = 0;
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private void CmbDispositivoADB_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbDispositivoADB.SelectedItem != null)
            {
                var seleccionado = cmbDispositivoADB.SelectedItem.ToString();
                if (seleccionado != null && 
                    !seleccionado.Contains("Escaneando") && 
                    !seleccionado.Contains("Error") && 
                    !seleccionado.Contains("No hay") &&
                    !seleccionado.Contains("No disponible"))
                {
                    // Extraer el device ID del formato "ADB: device_id"
                    if (seleccionado.Contains("ADB:"))
                    {
                        _dispositivoActual = seleccionado.Split(':')[1].Trim();
                        _logManager?.AgregarLog($"✓ Dispositivo seleccionado: {_dispositivoActual}", TipoLog.Exito);
                    }
                }
            }
        }

        private void CmbDispositivoCOM_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbDispositivoCOM.SelectedItem != null)
            {
                var seleccionado = cmbDispositivoCOM.SelectedItem.ToString();
                if (seleccionado != null && !seleccionado.Contains("Escaneando") && !seleccionado.Contains("Error") && !seleccionado.Contains("No hay"))
                {
                    _logManager?.AgregarLog($"✓ Puerto COM seleccionado: {seleccionado}", TipoLog.Exito);
                }
            }
        }

        // ============= CAMBIO DE CONTENIDO POR MARCA =============

        private void BtnAndroid_Click(object? sender, EventArgs e)
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("ANDROID OPERATIONS", TipoLog.Exito);
            CambiarBotonActivo(btnAndroid);
            CargarPanel(_androidOps!);
        }

        private void BtnSamsung_Click(object? sender, EventArgs e)
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("SAMSUNG OPERATIONS", TipoLog.Exito);
            CambiarBotonActivo(btnSamsung);
            CargarPanel(_samsungOps!);
        }

        private void BtnHonor_Click(object? sender, EventArgs e)
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("HONOR OPERATIONS", TipoLog.Exito);
            CambiarBotonActivo(btnHonor);
            CargarPanel(_honorOps!);
        }

        private void BtnMotorola_Click(object? sender, EventArgs e)
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("MOTOROLA OPERATIONS", TipoLog.Exito);
            CambiarBotonActivo(btnMotorola);
            CargarPanel(_motorolaOps!);
        }

        private void BtnQualcomm_Click(object? sender, EventArgs e)
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("XIAOMI OPERATIONS", TipoLog.Exito);
            _logManager?.AgregarLog("⚠ Funcionalidad en desarrollo", TipoLog.Advertencia);
            CambiarBotonActivo(btnQualcomm);
            // CargarPanel(_qualcommOps!); // TODO: Implementar QualcommOperations
        }

        // ============= MÉTODOS AUXILIARES =============

        private void CambiarBotonActivo(ModernButton botonNuevo)
        {
            // Restaurar color del botón anterior
            if (_botonActivo != null)
            {
                _botonActivo.BackColor = Color.FromArgb(224, 224, 224);
                _botonActivo.ForeColor = Color.FromArgb(66, 66, 66);
                _botonActivo.HoverColor = Color.FromArgb(200, 200, 200);
            }

            // Activar nuevo botón
            _botonActivo = botonNuevo;
            _botonActivo.BackColor = Color.FromArgb(52, 152, 219);
            _botonActivo.ForeColor = Color.White;
            _botonActivo.HoverColor = Color.FromArgb(41, 128, 185);
        }

        private void CargarPanel(IBrandOperations brandOperations)
        {
            // Limpiar panel
            panelContenido.Controls.Clear();

            // Cargar nuevo panel de la marca
            var panel = brandOperations.GetOperationsPanel();
            panelContenido.Controls.Add(panel);
        }

        // ============= EVENTO BOTÓN CANCELAR =============

        private void BtnCancelar_Click(object? sender, EventArgs e)
        {
            if (_logManager != null)
            {
                _logManager.CancelarOperacion();
            }
        }
    }
}
