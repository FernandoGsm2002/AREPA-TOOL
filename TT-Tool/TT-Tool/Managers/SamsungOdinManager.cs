using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Management;
using System.Text.Json;
using TT_Tool.Forms;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Gestor para operaciones Samsung Odin
    /// Basado en SharpOdinClient y Freya
    /// </summary>
    public class SamsungOdinManager
    {
        // Samsung Vendor ID y Product IDs conocidos para modo Download
        private const int SAMSUNG_VENDOR_ID = 0x04E8;
        private static readonly int[] DOWNLOAD_MODE_PRODUCT_IDS = new int[] 
        { 
            0x685D,  // Modo Download principal
            0x68C3,  // Modo Download alternativo
            0x6601,  // Modo Download (algunos modelos)
            0x6860   // Modo Download (modelos antiguos)
        };

        public event EventHandler<string>? OnLogMessage;
        public event EventHandler<(string filename, long max, long value, long writtenSize)>? OnProgressChanged;
        
        private MagiskManager? _magiskManager;
        private LogManager? _logManager;

        /// <summary>
        /// Detecta si hay un dispositivo Samsung en modo Download/Odin usando WMI
        /// </summary>
        public bool DetectarDispositivoOdin()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Buscando dispositivo Samsung en modo Odin...");
                OnLogMessage?.Invoke(this, "");

                // Método 1: Usar WMI (más confiable en Windows)
                bool encontradoWMI = DetectarConWMI();
                
                // Método 2: Usar LibUsbDotNet
                bool encontradoLibUsb = DetectarConLibUsb();

                if (encontradoWMI || encontradoLibUsb)
                {
                    OnLogMessage?.Invoke(this, "");
                    OnLogMessage?.Invoke(this, "✓ Dispositivo Samsung detectado correctamente");
                    return true;
                }

                OnLogMessage?.Invoke(this, "⚠ No se encontró dispositivo Samsung en modo Odin");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "Para entrar en modo Odin:");
                OnLogMessage?.Invoke(this, "1. Apaga el dispositivo");
                OnLogMessage?.Invoke(this, "2. Mantén presionado: Vol- + Home + Power");
                OnLogMessage?.Invoke(this, "3. Presiona Vol+ para confirmar");
                OnLogMessage?.Invoke(this, "4. Conecta el cable USB");
                
                return false;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al detectar dispositivo: {ex.Message}");
                return false;
            }
        }

        private bool DetectarConWMI()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Método 1: Buscando con WMI (Windows Management)...");
                
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE '%USB%'"))
                {
                    foreach (ManagementObject device in searcher.Get())
                    {
                        string? deviceId = device["DeviceID"]?.ToString();
                        string? name = device["Name"]?.ToString();
                        
                        if (deviceId != null && name != null)
                        {
                            // Buscar Samsung en modo Download
                            if (deviceId.Contains("VID_04E8", StringComparison.OrdinalIgnoreCase))
                            {
                                foreach (var pid in DOWNLOAD_MODE_PRODUCT_IDS)
                                {
                                    if (deviceId.Contains($"PID_{pid:X4}", StringComparison.OrdinalIgnoreCase))
                                    {
                                        OnLogMessage?.Invoke(this, $"✓ Encontrado con WMI:");
                                        OnLogMessage?.Invoke(this, $"  Nombre: {name}");
                                        OnLogMessage?.Invoke(this, $"  Device ID: {deviceId}");
                                        return true;
                                    }
                                }
                                
                                // Mostrar cualquier dispositivo Samsung encontrado
                                OnLogMessage?.Invoke(this, $"  Samsung encontrado: {name}");
                                OnLogMessage?.Invoke(this, $"  Device ID: {deviceId}");
                            }
                        }
                    }
                }
                
                OnLogMessage?.Invoke(this, "  No encontrado con WMI");
                return false;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"  Error WMI: {ex.Message}");
                return false;
            }
        }

        private bool DetectarConLibUsb()
        {
            try
            {
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "Método 2: Buscando con LibUsbDotNet...");

                var usbDevices = UsbDevice.AllDevices;
                
                foreach (UsbRegistry usbRegistry in usbDevices)
                {
                    if (usbRegistry.Vid == SAMSUNG_VENDOR_ID)
                    {
                        foreach (var pid in DOWNLOAD_MODE_PRODUCT_IDS)
                        {
                            if (usbRegistry.Pid == pid)
                            {
                                OnLogMessage?.Invoke(this, $"✓ Encontrado con LibUsb:");
                                OnLogMessage?.Invoke(this, $"  Vendor ID: 0x{usbRegistry.Vid:X4}");
                                OnLogMessage?.Invoke(this, $"  Product ID: 0x{usbRegistry.Pid:X4}");
                                OnLogMessage?.Invoke(this, $"  Nombre: {usbRegistry.Name}");
                                return true;
                            }
                        }
                    }
                }

                OnLogMessage?.Invoke(this, "  No encontrado con LibUsb");
                return false;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"  Error LibUsb: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene información del dispositivo conectado
        /// </summary>
        public bool ObtenerInformacionDispositivo()
        {
            try
            {
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "Obteniendo información del dispositivo...");
                
                // Método 1: Intentar con LibUsb
                var usbDevices = UsbDevice.AllDevices;
                
                foreach (UsbRegistry usbRegistry in usbDevices)
                {
                    if (usbRegistry.Vid == SAMSUNG_VENDOR_ID)
                    {
                        foreach (var pid in DOWNLOAD_MODE_PRODUCT_IDS)
                        {
                            if (usbRegistry.Pid == pid)
                            {
                                OnLogMessage?.Invoke(this, "");
                                OnLogMessage?.Invoke(this, "=== INFORMACIÓN DEL DISPOSITIVO ===");
                                OnLogMessage?.Invoke(this, $"Vendor ID: 0x{usbRegistry.Vid:X4}");
                                OnLogMessage?.Invoke(this, $"Product ID: 0x{usbRegistry.Pid:X4}");
                                OnLogMessage?.Invoke(this, $"Nombre: {usbRegistry.Name}");
                                
                                UsbDevice? device;
                                if (usbRegistry.Open(out device))
                                {
                                    try
                                    {
                                        OnLogMessage?.Invoke(this, $"Producto: {device.Info.ProductString}");
                                        OnLogMessage?.Invoke(this, $"Fabricante: {device.Info.ManufacturerString}");
                                        OnLogMessage?.Invoke(this, $"Serial: {device.Info.SerialString}");
                                    }
                                    finally
                                    {
                                        device.Close();
                                    }
                                }
                                
                                OnLogMessage?.Invoke(this, "===================================");
                                return true;
                            }
                        }
                    }
                }

                // Método 2: Usar WMI si LibUsb no funciona
                return ObtenerInfoConWMI();
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al obtener información: {ex.Message}");
                return false;
            }
        }

        private bool ObtenerInfoConWMI()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE '%USB%'"))
                {
                    foreach (ManagementObject device in searcher.Get())
                    {
                        string? deviceId = device["DeviceID"]?.ToString();
                        string? name = device["Name"]?.ToString();
                        
                        if (deviceId != null && deviceId.Contains("VID_04E8", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var pid in DOWNLOAD_MODE_PRODUCT_IDS)
                            {
                                if (deviceId.Contains($"PID_{pid:X4}", StringComparison.OrdinalIgnoreCase))
                                {
                                    OnLogMessage?.Invoke(this, "");
                                    OnLogMessage?.Invoke(this, "=== INFORMACIÓN DEL DISPOSITIVO ===");
                                    OnLogMessage?.Invoke(this, $"Nombre: {name}");
                                    OnLogMessage?.Invoke(this, $"Device ID: {deviceId}");
                                    
                                    // Obtener más información
                                    if (device["Description"] != null)
                                        OnLogMessage?.Invoke(this, $"Descripción: {device["Description"]}");
                                    if (device["Status"] != null)
                                        OnLogMessage?.Invoke(this, $"Estado: {device["Status"]}");
                                    
                                    OnLogMessage?.Invoke(this, "===================================");
                                    return true;
                                }
                            }
                        }
                    }
                }
                
                OnLogMessage?.Invoke(this, "No se pudo obtener información del dispositivo");
                return false;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error WMI: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reinicia el dispositivo a modo normal
        /// </summary>
        public async Task<bool> RebootNormal()
        {
            SharpOdinClient.Odin? odin = null;
            
            try
            {
                OnLogMessage?.Invoke(this, "Reiniciando dispositivo a modo normal...");
                OnLogMessage?.Invoke(this, "");
                
                // Crear instancia del protocolo
                odin = new SharpOdinClient.Odin();
                
                // Conectar
                if (!await odin.FindAndSetDownloadMode())
                {
                    OnLogMessage?.Invoke(this, "⚠ No se pudo conectar con el dispositivo");
                    return false;
                }
                
                // Enviar comando de reboot
                if (!await odin.PDAToNormal())
                {
                    OnLogMessage?.Invoke(this, "⚠ Error al enviar comando de reboot");
                    return false;
                }
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "✓ El dispositivo se está reiniciando...");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al reiniciar: {ex.Message}");
                return false;
            }
            finally
            {
                // SharpOdinClient se limpia autom�ticamente
            }
        }

        // Opciones de flash (como Freya)
        public bool AutoReboot { get; set; } = true;
        public bool BootUpdate { get; set; } = false;
        public bool EfsClear { get; set; } = false;
        public bool AutoRoot { get; set; } = false;
        
        /// <summary>
        /// Configura el LogManager para usar la barra de progreso
        /// </summary>
        public void SetLogManager(LogManager logManager)
        {
            _logManager = logManager;
        }

        /// <summary>
        /// Inicia el proceso de flash usando SharpOdinClient
        /// </summary>
        public async Task<bool> IniciarFlash(string? blPath, string? apPath, string? cpPath, string? cscPath)
        {
            SharpOdinClient.Odin? odin = null;
            CancellationToken token = default;
            
            try
            {
                OnLogMessage?.Invoke(this, "=== INICIANDO FLASH ===");
                OnLogMessage?.Invoke(this, "");
                
                // Verificar que al menos un archivo esté seleccionado
                if (string.IsNullOrEmpty(blPath) && string.IsNullOrEmpty(apPath) && 
                    string.IsNullOrEmpty(cpPath) && string.IsNullOrEmpty(cscPath))
                {
                    OnLogMessage?.Invoke(this, "[ERROR] Debes seleccionar al menos un archivo");
                    return false;
                }
                
                // Iniciar progreso
                if (_logManager != null)
                {
                    token = _logManager.IniciarProgreso("Iniciando flash...", 9);
                }
                
                OnLogMessage?.Invoke(this, "[1/9] Verificando archivos...");
                _logManager?.ActualizarProgreso(1, "[1/9] Verificando archivos...");
                
                // Preparar lista de archivos
                var archivos = new List<string>();
                if (!string.IsNullOrEmpty(blPath))
                {
                    OnLogMessage?.Invoke(this, $"  BL: {Path.GetFileName(blPath)}");
                    archivos.Add(blPath);
                }
                if (!string.IsNullOrEmpty(apPath))
                {
                    OnLogMessage?.Invoke(this, $"  AP: {Path.GetFileName(apPath)}");
                    archivos.Add(apPath);
                }
                if (!string.IsNullOrEmpty(cpPath))
                {
                    OnLogMessage?.Invoke(this, $"  CP: {Path.GetFileName(cpPath)}");
                    archivos.Add(cpPath);
                }
                if (!string.IsNullOrEmpty(cscPath))
                {
                    OnLogMessage?.Invoke(this, $"  CSC: {Path.GetFileName(cscPath)}");
                    archivos.Add(cscPath);
                }
                
                // Verificar cancelación
                if (token.IsCancellationRequested)
                {
                    _logManager?.FinalizarProgreso("Cancelado", false);
                    return false;
                }
                
                // Mostrar opciones
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, $"[CONFIG] Auto Reboot: {(AutoReboot ? "Si" : "No")} | Boot Update: {(BootUpdate ? "Si" : "No")} | EFS Clear: {(EfsClear ? "Si" : "No")}");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "[2/9] Conectando con dispositivo...");
                _logManager?.ActualizarProgreso(2, "[2/9] Conectando con dispositivo...");
                
                // Verificar cancelación
                if (token.IsCancellationRequested)
                {
                    _logManager?.FinalizarProgreso("Cancelado", false);
                    return false;
                }
                
                // Crear instancia de SharpOdinClient
                odin = new SharpOdinClient.Odin();
                
                // Conectar eventos de log y progreso
                string lastLoggedFile = "";
                int lastLoggedPercentage = -1;
                int fileCounter = 0;
                
                odin.Log += (text, msgType, isError) =>
                {
                    // Silenciar logs normales, solo mostrar errores
                    if (isError)
                    {
                        OnLogMessage?.Invoke(this, $"[ERROR] {text}");
                    }
                };
                
                odin.ProgressChanged += (filename, max, value, writtenSize) =>
                {
                    OnProgressChanged?.Invoke(this, (filename, max, value, writtenSize));
                    
                    // Solo mostrar cuando cambia de archivo (inicio) y al completar (100%)
                    double percentage = max > 0 ? (double)value / max * 100 : 0;
                    int percentageInt = (int)percentage;
                    
                    // Log cuando cambia de archivo
                    if (lastLoggedFile != filename)
                    {
                        lastLoggedFile = filename;
                        lastLoggedPercentage = -1;
                        fileCounter++;
                        
                        string sizeMB = $"{max / 1024.0 / 1024.0:F1} MB";
                        OnLogMessage?.Invoke(this, $"  [{fileCounter}/{archivos.Count}] Flasheando {filename} ({sizeMB})...");
                    }
                    // Solo log cuando completa al 100%
                    else if (percentageInt == 100 && lastLoggedPercentage != 100)
                    {
                        lastLoggedPercentage = 100;
                        OnLogMessage?.Invoke(this, $"  [{fileCounter}/{archivos.Count}] {filename} completado");
                    }
                };
                
                if (!await odin.FindAndSetDownloadMode())
                {
                    OnLogMessage?.Invoke(this, "[ERROR] No se pudo conectar con el dispositivo");
                    _logManager?.FinalizarProgreso("Error de conexión", false);
                    return false;
                }
                
                OnLogMessage?.Invoke(this, "[OK] Dispositivo detectado en modo Download");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "[3/9] Verificando modo Odin...");
                _logManager?.ActualizarProgreso(3, "[3/9] Verificando modo Odin...");
                
                if (token.IsCancellationRequested)
                {
                    _logManager?.FinalizarProgreso("Cancelado", false);
                    return false;
                }
                
                if (!await odin.IsOdin())
                {
                    OnLogMessage?.Invoke(this, "[ERROR] El dispositivo no esta en modo Odin");
                    _logManager?.FinalizarProgreso("Error: No está en modo Odin", false);
                    return false;
                }
                OnLogMessage?.Invoke(this, "[OK] Modo ODIN confirmado");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "[4/9] Calculando tamaño total...");
                _logManager?.ActualizarProgreso(4, "[4/9] Calculando tamaño...");
                
                long tamanoTotal = 0;
                foreach (var archivo in archivos)
                {
                    if (File.Exists(archivo))
                    {
                        tamanoTotal += new FileInfo(archivo).Length;
                    }
                }
                OnLogMessage?.Invoke(this, $"  Tamaño: {tamanoTotal / 1024 / 1024} MB");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "[5/9] Inicializando dispositivo...");
                _logManager?.ActualizarProgreso(5, "[5/9] Inicializando dispositivo...");
                
                if (token.IsCancellationRequested)
                {
                    _logManager?.FinalizarProgreso("Cancelado", false);
                    return false;
                }
                
                if (!await odin.LOKE_Initialize(tamanoTotal))
                {
                    OnLogMessage?.Invoke(this, "[ERROR] Error al inicializar dispositivo");
                    _logManager?.FinalizarProgreso("Error de inicialización", false);
                    return false;
                }
                OnLogMessage?.Invoke(this, "[OK] Dispositivo inicializado");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "[6/9] Leyendo PIT del dispositivo...");
                _logManager?.ActualizarProgreso(6, "[6/9] Leyendo PIT...");
                
                if (token.IsCancellationRequested)
                {
                    _logManager?.FinalizarProgreso("Cancelado", false);
                    return false;
                }
                
                var pitResult = await odin.Read_Pit();
                if (!pitResult.Result || pitResult.Pit == null)
                {
                    OnLogMessage?.Invoke(this, "[ERROR] Error al leer PIT");
                    _logManager?.FinalizarProgreso("Error al leer PIT", false);
                    return false;
                }
                OnLogMessage?.Invoke(this, "[OK] PIT leido correctamente");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "[7/9] Preparando archivos para flash...");
                _logManager?.ActualizarProgreso(7, "[7/9] Preparando archivos...");
                
                // Convertir archivos al formato de SharpOdinClient
                var listaFlash = new List<SharpOdinClient.structs.FileFlash>();
                
                foreach (var archivo in archivos)
                {
                    var infoTar = odin.tar.TarInformation(archivo);
                    if (infoTar != null)
                    {
                        foreach (var item in infoTar)
                        {
                            // Determinar el tamaño según el tipo de archivo
                            long rawSize = item.Filesize;
                            
                            // Si es LZ4, calcular el tamaño descomprimido
                            string extension = Path.GetExtension(item.Filename).ToLower();
                            if (extension == ".lz4")
                            {
                                // SharpOdinClient tiene un método para calcular el tamaño LZ4
                                try
                                {
                                    var odinTemp = new SharpOdinClient.Odin();
                                    rawSize = odinTemp.CalculateLz4SizeFromTar(archivo, item.Filename);
                                }
                                catch
                                {
                                    // Si falla, usar el tamaño del archivo
                                    rawSize = item.Filesize;
                                }
                            }
                            
                            listaFlash.Add(new SharpOdinClient.structs.FileFlash
                            {
                                Enable = true,
                                FileName = item.Filename,
                                FilePath = archivo,
                                RawSize = rawSize
                            });
                            
                            OnLogMessage?.Invoke(this, $"  [{listaFlash.Count}] {item.Filename} - {rawSize / 1024 / 1024} MB");
                        }
                    }
                }
                
                OnLogMessage?.Invoke(this, $"[INFO] Total de particiones: {listaFlash.Count}");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "[8/9] Flasheando firmware...");
                _logManager?.ActualizarProgreso(8, "[8/9] Flasheando firmware...");
                OnLogMessage?.Invoke(this, "");
                
                if (token.IsCancellationRequested)
                {
                    _logManager?.FinalizarProgreso("Cancelado", false);
                    return false;
                }
                
                if (!await odin.FlashFirmware(listaFlash, pitResult.Pit, EfsClear ? 1 : 0, BootUpdate ? 1 : 0, true))
                {
                    OnLogMessage?.Invoke(this, "⚠ Error durante el flash");
                    _logManager?.FinalizarProgreso("Error en flash", false);
                    return false;
                }
                
                OnLogMessage?.Invoke(this, "");
                
                if (AutoReboot)
                {
                    OnLogMessage?.Invoke(this, "9. Reiniciando dispositivo...");
                    
                    if (!await odin.PDAToNormal())
                    {
                        OnLogMessage?.Invoke(this, "⚠ Advertencia: Error al reiniciar (puedes reiniciar manualmente)");
                    }
                }
                else
                {
                    OnLogMessage?.Invoke(this, "Auto Reboot deshabilitado - Reinicia manualmente");
                }
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "✓✓✓ FLASH COMPLETADO EXITOSAMENTE ✓✓✓");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error crítico: {ex.Message}");
                OnLogMessage?.Invoke(this, $"Stack: {ex.StackTrace}");
                return false;
            }
            finally
            {
                // SharpOdinClient se limpia autom�ticamente
            }
        }
        
        /// <summary>
        /// Inicia el proceso de flash con selección de particiones específicas
        /// </summary>
        public async Task<bool> IniciarFlashConParticiones(
            List<PartitionInfo>? partitionsBL,
            List<PartitionInfo>? partitionsAP,
            List<PartitionInfo>? partitionsCP,
            List<PartitionInfo>? partitionsCSC)
        {
            SharpOdinClient.Odin? odin = null;
            
            try
            {
                OnLogMessage?.Invoke(this, "=== INICIANDO PROCESO DE FLASH CON PARTICIONES SELECCIONADAS ===");
                OnLogMessage?.Invoke(this, "");
                
                // Combinar todas las particiones
                var todasParticiones = new List<PartitionInfo>();
                if (partitionsBL != null) todasParticiones.AddRange(partitionsBL);
                if (partitionsAP != null) todasParticiones.AddRange(partitionsAP);
                if (partitionsCP != null) todasParticiones.AddRange(partitionsCP);
                if (partitionsCSC != null) todasParticiones.AddRange(partitionsCSC);
                
                // Filtrar solo las habilitadas
                var particionesHabilitadas = todasParticiones.Where(p => p.Enabled).ToList();
                
                if (particionesHabilitadas.Count == 0)
                {
                    OnLogMessage?.Invoke(this, "⚠ Error: No hay particiones seleccionadas para flashear");
                    return false;
                }
                
                OnLogMessage?.Invoke(this, "1. Particiones seleccionadas:");
                OnLogMessage?.Invoke(this, "");
                
                foreach (var partition in particionesHabilitadas)
                {
                    OnLogMessage?.Invoke(this, $"✓ {partition.FileName} ({partition.Size / 1024 / 1024} MB)");
                }
                
                // Mostrar opciones
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "Opciones:");
                OnLogMessage?.Invoke(this, $"  Auto Reboot: {(AutoReboot ? "Sí" : "No")}");
                OnLogMessage?.Invoke(this, $"  Boot Update: {(BootUpdate ? "Sí" : "No")}");
                OnLogMessage?.Invoke(this, $"  EFS Clear: {(EfsClear ? "Sí" : "No")}");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "2. Conectando con dispositivo...");
                
                // Crear instancia del protocolo Odin
                odin = new SharpOdinClient.Odin();
                
                // Conectar eventos de log y progreso (simplificado para no llenar el log)
                string lastLoggedFile = "";
                int lastLoggedPercentage = -1;
                
                odin.Log += (text, msgType, isError) =>
                {
                    OnLogMessage?.Invoke(this, $"{(isError ? "⚠ " : "")}{text}");
                };
                
                odin.ProgressChanged += (filename, max, value, writtenSize) =>
                {
                    OnProgressChanged?.Invoke(this, (filename, max, value, writtenSize));
                    
                    // Solo mostrar log cada 25% o cuando cambia de archivo
                    double percentage = max > 0 ? (double)value / max * 100 : 0;
                    int percentageInt = (int)percentage;
                    
                    bool shouldLog = false;
                    
                    // Log cuando cambia de archivo
                    if (lastLoggedFile != filename)
                    {
                        shouldLog = true;
                        lastLoggedFile = filename;
                        lastLoggedPercentage = -1;
                    }
                    // Log cada 25%: 0%, 25%, 50%, 75%, 100%
                    else if (percentageInt >= lastLoggedPercentage + 25 || percentageInt == 100)
                    {
                        shouldLog = true;
                        lastLoggedPercentage = (percentageInt / 25) * 25;
                    }
                    
                    if (shouldLog)
                    {
                        string sizeMB = $"{writtenSize / 1024.0 / 1024.0:F1} MB";
                        string totalMB = $"{max / 1024.0 / 1024.0:F1} MB";
                        OnLogMessage?.Invoke(this, $"   📦 {filename} - {percentage:F0}% ({sizeMB} / {totalMB})");
                    }
                };
                
                if (!await odin.FindAndSetDownloadMode())
                {
                    OnLogMessage?.Invoke(this, "⚠ Error: No se pudo conectar con el dispositivo");
                    return false;
                }
                
                await odin.PrintInfo();
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "3. Verificando modo Odin...");
                
                if (!await odin.IsOdin())
                {
                    OnLogMessage?.Invoke(this, "⚠ Error: El dispositivo no está en modo Odin");
                    return false;
                }
                OnLogMessage?.Invoke(this, "✓ Modo ODIN confirmado");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "4. Calculando tamaño total...");
                
                long tamanoTotal = particionesHabilitadas.Sum(p => p.Size);
                OnLogMessage?.Invoke(this, $"Tamaño total: {tamanoTotal / 1024 / 1024} MB");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "5. Inicializando dispositivo...");
                
                if (!await odin.LOKE_Initialize(tamanoTotal))
                {
                    OnLogMessage?.Invoke(this, "⚠ Error al inicializar dispositivo");
                    return false;
                }
                OnLogMessage?.Invoke(this, "✓ Dispositivo inicializado");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "6. Leyendo PIT del dispositivo...");
                
                var pitResult = await odin.Read_Pit();
                if (!pitResult.Result || pitResult.Pit == null)
                {
                    OnLogMessage?.Invoke(this, "⚠ Error al leer PIT");
                    return false;
                }
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "7. Preparando particiones para flash...");
                
                // Convertir particiones al formato de SharpOdinClient
                var listaFlash = new List<SharpOdinClient.structs.FileFlash>();
                
                foreach (var partition in particionesHabilitadas)
                {
                    listaFlash.Add(new SharpOdinClient.structs.FileFlash
                    {
                        Enable = true,
                        FileName = partition.FileName,
                        FilePath = partition.FilePath,
                        RawSize = partition.Size
                    });
                }
                
                OnLogMessage?.Invoke(this, $"Total de particiones a flashear: {listaFlash.Count}");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "8. Flasheando firmware...");
                OnLogMessage?.Invoke(this, "");
                
                if (!await odin.FlashFirmware(listaFlash, pitResult.Pit, EfsClear ? 1 : 0, BootUpdate ? 1 : 0, true))
                {
                    OnLogMessage?.Invoke(this, "⚠ Error durante el flash");
                    return false;
                }
                
                OnLogMessage?.Invoke(this, "");
                
                if (AutoReboot)
                {
                    OnLogMessage?.Invoke(this, "9. Reiniciando dispositivo...");
                    
                    if (!await odin.PDAToNormal())
                    {
                        OnLogMessage?.Invoke(this, "⚠ Advertencia: Error al reiniciar (puedes reiniciar manualmente)");
                    }
                }
                else
                {
                    OnLogMessage?.Invoke(this, "Auto Reboot deshabilitado - Reinicia manualmente");
                }
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "✓✓✓ FLASH COMPLETADO EXITOSAMENTE ✓✓✓");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error crítico: {ex.Message}");
                OnLogMessage?.Invoke(this, $"Stack: {ex.StackTrace}");
                return false;
            }
            finally
            {
                // SharpOdinClient se limpia autom�ticamente
            }
        }

        /// <summary>
        /// Obtiene versiones OFICIALES ESTABLES de Magisk y Magisk Delta
        /// </summary>
        public async Task<List<string>> ObtenerVersionesMagisk()
        {
            try
            {
                InitializeMagiskManager();
                
                OnLogMessage?.Invoke(this, "🔍 Buscando versiones oficiales de Magisk...");
                
                var versions = new List<string>();
                
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "TT-Tool");
                client.Timeout = TimeSpan.FromSeconds(10);
                
                // Obtener versiones ESTABLES de Magisk (sin Canary ni Pre-release)
                try
                {
                    var response = await client.GetStringAsync("https://api.github.com/repos/topjohnwu/Magisk/releases");
                    var json = JsonDocument.Parse(response);
                    
                    int count = 0;
                    foreach (var release in json.RootElement.EnumerateArray())
                    {
                        if (count >= 10) break;
                        
                        var tagName = release.GetProperty("tag_name").GetString();
                        var isPrerelease = release.GetProperty("prerelease").GetBoolean();
                        
                        // SOLO versiones estables oficiales
                        if (!string.IsNullOrEmpty(tagName) && !isPrerelease)
                        {
                            string version = $"Magisk {tagName.Replace("v", "")}";
                            versions.Add(version);
                            count++;
                        }
                    }
                    
                    OnLogMessage?.Invoke(this, $"✓ Encontradas {count} versiones oficiales de Magisk");
                }
                catch (Exception ex)
                {
                    OnLogMessage?.Invoke(this, $"⚠ Error al obtener versiones Magisk: {ex.Message}");
                }
                
                // Obtener versiones ESTABLES de Magisk Delta
                try
                {
                    OnLogMessage?.Invoke(this, "🔍 Buscando versiones oficiales de Magisk Delta...");
                    var responseDelta = await client.GetStringAsync("https://api.github.com/repos/HuskyDG/magisk-files/releases");
                    var jsonDelta = JsonDocument.Parse(responseDelta);
                    
                    int countDelta = 0;
                    foreach (var release in jsonDelta.RootElement.EnumerateArray())
                    {
                        if (countDelta >= 10) break;
                        
                        var tagName = release.GetProperty("tag_name").GetString();
                        var name = release.GetProperty("name").GetString();
                        var isPrerelease = release.GetProperty("prerelease").GetBoolean();
                        
                        // SOLO versiones estables oficiales de Delta
                        if (!isPrerelease && tagName != null && 
                            (tagName.Contains("delta", StringComparison.OrdinalIgnoreCase) || 
                            (name != null && name.Contains("delta", StringComparison.OrdinalIgnoreCase))))
                        {
                            string version = $"Magisk Delta {tagName.Replace("v", "").Replace("delta", "").Replace("Delta", "").Trim()}";
                            versions.Add(version);
                            countDelta++;
                        }
                    }
                    
                    if (countDelta > 0)
                    {
                        OnLogMessage?.Invoke(this, $"✓ Encontradas {countDelta} versiones oficiales de Delta");
                    }
                }
                catch (Exception ex)
                {
                    OnLogMessage?.Invoke(this, $"⚠ Error al obtener versiones Delta: {ex.Message}");
                }
                
                OnLogMessage?.Invoke(this, $"✓ Total: {versions.Count} versiones oficiales disponibles");
                return versions;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"⚠ Error al obtener versiones: {ex.Message}");
                return new List<string> { "Magisk 29.0", "Magisk 28.1", "Magisk 28.0" };
            }
        }
        
        /// <summary>
        /// Instala Magisk APK y copia el AP al dispositivo para parcheado manual
        /// </summary>
        public async Task<(bool success, string? message)> PrepararMagiskPatch(string apPath, string magiskVersion)
        {
            try
            {
                InitializeMagiskManager();
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "=== PREPARANDO MAGISK PATCH ===");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, $"[INFO] Archivo AP: {Path.GetFileName(apPath)}");
                OnLogMessage?.Invoke(this, $"[INFO] Version Magisk: {magiskVersion}");
                OnLogMessage?.Invoke(this, "");
                
                // 1. Obtener Magisk
                OnLogMessage?.Invoke(this, "[1/3] Obteniendo Magisk...");
                var (versionSuccess, version, downloadUrl) = await _magiskManager!.ObtenerVersionMagisk(magiskVersion);
                
                if (!versionSuccess)
                {
                    OnLogMessage?.Invoke(this, "[ERROR] No se pudo obtener la version de Magisk");
                    return (false, "Error al obtener versión de Magisk");
                }
                
                // 2. Descargar Magisk APK
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "[2/3] Descargando Magisk APK...");
                var (downloadSuccess, apkPath) = await _magiskManager.DescargarMagisk(downloadUrl, version);
                
                if (!downloadSuccess)
                {
                    OnLogMessage?.Invoke(this, "[ERROR] Error al descargar Magisk");
                    return (false, "Error al descargar Magisk");
                }
                
                OnLogMessage?.Invoke(this, $"[OK] Descargado: {new FileInfo(apkPath).Length / 1024 / 1024} MB");
                
                // 3. Instalar APK y copiar AP al dispositivo
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "[3/3] Instalando Magisk y copiando AP al dispositivo...");
                
                var adbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "platform-tools", "adb.exe");
                if (!File.Exists(adbPath))
                {
                    OnLogMessage?.Invoke(this, "[ERROR] ADB no encontrado");
                    OnLogMessage?.Invoke(this, $"[INFO] APK guardado en: {apkPath}");
                    return (false, "ADB no encontrado");
                }
                
                // Verificar dispositivo
                var processDevices = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = adbPath,
                        Arguments = "devices",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                
                processDevices.Start();
                string outputDevices = await processDevices.StandardOutput.ReadToEndAsync();
                await processDevices.WaitForExitAsync();
                
                if (!outputDevices.Contains("device") || outputDevices.Split('\n').Length < 3)
                {
                    OnLogMessage?.Invoke(this, "[ERROR] No hay dispositivo conectado en ADB");
                    OnLogMessage?.Invoke(this, "[INFO] Conecta tu dispositivo y habilita USB Debugging");
                    return (false, "No hay dispositivo conectado");
                }
                
                OnLogMessage?.Invoke(this, "[OK] Dispositivo detectado");
                
                // Instalar APK
                OnLogMessage?.Invoke(this, "[INFO] Instalando Magisk APK...");
                var processInstall = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = adbPath,
                        Arguments = $"install -r \"{apkPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                
                processInstall.Start();
                string outputInstall = await processInstall.StandardOutput.ReadToEndAsync();
                await processInstall.WaitForExitAsync();
                
                if (!outputInstall.Contains("Success"))
                {
                    OnLogMessage?.Invoke(this, "[ERROR] No se pudo instalar el APK");
                    OnLogMessage?.Invoke(this, $"[INFO] Instala manualmente: {apkPath}");
                    return (false, "Error al instalar APK");
                }
                
                OnLogMessage?.Invoke(this, "[OK] Magisk APK instalado correctamente");
                
                // Copiar AP al dispositivo
                OnLogMessage?.Invoke(this, "[INFO] Copiando AP al dispositivo...");
                OnLogMessage?.Invoke(this, $"[INFO] Tamaño: {new FileInfo(apPath).Length / 1024 / 1024} MB");
                
                string devicePath = $"/sdcard/Download/{Path.GetFileName(apPath)}";
                var processPush = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = adbPath,
                        Arguments = $"push \"{apPath}\" \"{devicePath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                
                processPush.Start();
                string outputPush = await processPush.StandardOutput.ReadToEndAsync();
                await processPush.WaitForExitAsync();
                
                if (processPush.ExitCode != 0)
                {
                    OnLogMessage?.Invoke(this, "[ERROR] No se pudo copiar el AP al dispositivo");
                    return (false, "Error al copiar AP");
                }
                
                OnLogMessage?.Invoke(this, $"[OK] AP copiado a: {devicePath}");
                
                // Mostrar instrucciones
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "=== PROCESO COMPLETADO ===");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "[IMPORTANTE] INSTRUCCIONES:");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "1. PARCHA EL AP MANUALMENTE:");
                OnLogMessage?.Invoke(this, "   - Abre Magisk Manager en tu dispositivo");
                OnLogMessage?.Invoke(this, "   - Ve a 'Instalar' > 'Seleccionar y parchear un archivo'");
                OnLogMessage?.Invoke(this, $"   - Selecciona: {devicePath}");
                OnLogMessage?.Invoke(this, "   - Espera a que Magisk lo parchee");
                OnLogMessage?.Invoke(this, "   - El AP parcheado estará en /sdcard/Download/");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "2. FLASHEA CON ODIN:");
                OnLogMessage?.Invoke(this, "   - Copia el AP parcheado de vuelta a tu PC");
                OnLogMessage?.Invoke(this, "   - Ve al apartado 'Odin Mode' en esta herramienta");
                OnLogMessage?.Invoke(this, "   - Carga el AP parcheado");
                OnLogMessage?.Invoke(this, "   - Flashea normalmente");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "NOTA: Para fix Apps bancarias, ve a 'Main Services'");
                OnLogMessage?.Invoke(this, "      y ejecuta 'FIX APPS BANCARIAS 2025'");
                OnLogMessage?.Invoke(this, "");
                
                return (true, "Proceso completado exitosamente");
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, $"[ERROR] Error: {ex.Message}");
                return (false, ex.Message);
            }
        }
        
        /// <summary>
        /// Reemplaza boot.img en un archivo AP TAR
        /// </summary>
        private async Task<bool> ReemplazarBootEnAP(string apOriginal, string bootNuevo, string apDestino)
        {
            try
            {
                OnLogMessage?.Invoke(this, "   📦 Procesando archivo TAR...");
                
                // Leer el boot.img parcheado
                byte[] bootData = await File.ReadAllBytesAsync(bootNuevo);
                
                // Crear el nuevo AP TAR
                using (var fsOriginal = new FileStream(apOriginal, FileMode.Open, FileAccess.Read))
                using (var fsDestino = new FileStream(apDestino, FileMode.Create, FileAccess.Write))
                {
                    byte[] header = new byte[512];
                    
                    while (fsOriginal.Position < fsOriginal.Length)
                    {
                        // Leer header TAR
                        int bytesRead = await fsOriginal.ReadAsync(header, 0, 512);
                        if (bytesRead < 512 || header[0] == 0)
                            break;
                        
                        // Obtener nombre del archivo
                        string fileName = System.Text.Encoding.ASCII.GetString(header, 0, 100).TrimEnd('\0', ' ');
                        
                        // Obtener tamaño
                        string sizeStr = System.Text.Encoding.ASCII.GetString(header, 124, 12).TrimEnd('\0', ' ');
                        long fileSize = Convert.ToInt64(sizeStr, 8);
                        
                        // Si es boot.img, reemplazarlo
                        if (fileName.Contains("boot.img", StringComparison.OrdinalIgnoreCase) && 
                            !fileName.Contains("recovery", StringComparison.OrdinalIgnoreCase))
                        {
                            OnLogMessage?.Invoke(this, $"   🔄 Reemplazando {fileName}...");
                            
                            // Actualizar tamaño en header
                            string newSizeOctal = Convert.ToString(bootData.Length, 8).PadLeft(11, '0') + " ";
                            byte[] newSizeBytes = System.Text.Encoding.ASCII.GetBytes(newSizeOctal);
                            Array.Copy(newSizeBytes, 0, header, 124, newSizeBytes.Length);
                            
                            // Recalcular checksum
                            ActualizarChecksumTar(header);
                            
                            // Escribir header modificado
                            await fsDestino.WriteAsync(header, 0, 512);
                            
                            // Escribir boot.img parcheado
                            await fsDestino.WriteAsync(bootData, 0, bootData.Length);
                            
                            // Padding a 512 bytes
                            int padding = (512 - (bootData.Length % 512)) % 512;
                            if (padding > 0)
                            {
                                byte[] paddingBytes = new byte[padding];
                                await fsDestino.WriteAsync(paddingBytes, 0, padding);
                            }
                            
                            // Saltar el boot.img original
                            long skipSize = ((fileSize + 511) / 512) * 512;
                            fsOriginal.Seek(skipSize, SeekOrigin.Current);
                        }
                        else
                        {
                            // Copiar archivo sin modificar
                            await fsDestino.WriteAsync(header, 0, 512);
                            
                            // Copiar datos
                            long remainingSize = ((fileSize + 511) / 512) * 512;
                            byte[] buffer = new byte[Math.Min(remainingSize, 1024 * 1024)]; // 1MB buffer
                            
                            while (remainingSize > 0)
                            {
                                int toRead = (int)Math.Min(buffer.Length, remainingSize);
                                int read = await fsOriginal.ReadAsync(buffer, 0, toRead);
                                if (read == 0) break;
                                
                                await fsDestino.WriteAsync(buffer, 0, read);
                                remainingSize -= read;
                            }
                        }
                    }
                    
                    // Escribir fin de TAR (dos bloques de 512 bytes vacíos)
                    byte[] endBlock = new byte[1024];
                    await fsDestino.WriteAsync(endBlock, 0, 1024);
                }
                
                OnLogMessage?.Invoke(this, "   ✓ Archivo TAR procesado correctamente");
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"   ⚠ Error al procesar TAR: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Actualiza el checksum de un header TAR
        /// </summary>
        private void ActualizarChecksumTar(byte[] header)
        {
            // Llenar el campo checksum con espacios
            for (int i = 148; i < 156; i++)
                header[i] = (byte)' ';
            
            // Calcular checksum
            long checksum = 0;
            for (int i = 0; i < 512; i++)
                checksum += header[i];
            
            // Escribir checksum en octal
            string checksumOctal = Convert.ToString(checksum, 8).PadLeft(6, '0') + "\0 ";
            byte[] checksumBytes = System.Text.Encoding.ASCII.GetBytes(checksumOctal);
            Array.Copy(checksumBytes, 0, header, 148, Math.Min(checksumBytes.Length, 8));
        }
        
        /// <summary>
        /// Inicializa el MagiskManager si no existe
        /// </summary>
        private void InitializeMagiskManager()
        {
            if (_magiskManager == null)
            {
                _magiskManager = new MagiskManager();
                _magiskManager.OnLogMessage += (sender, msg) => OnLogMessage?.Invoke(this, msg);
            }
        }
        
        /// <summary>
        /// Verifica si un archivo es un firmware válido de Samsung
        /// </summary>
        public bool VerificarArchivoFirmware(string rutaArchivo)
        {
            try
            {
                if (!File.Exists(rutaArchivo))
                {
                    OnLogMessage?.Invoke(this, "⚠ El archivo no existe");
                    return false;
                }

                string extension = Path.GetExtension(rutaArchivo).ToLower();
                
                if (extension != ".tar" && extension != ".md5")
                {
                    OnLogMessage?.Invoke(this, "⚠ El archivo debe ser .tar o .tar.md5");
                    return false;
                }

                FileInfo fileInfo = new FileInfo(rutaArchivo);
                OnLogMessage?.Invoke(this, $"✓ Archivo válido: {fileInfo.Name}");
                OnLogMessage?.Invoke(this, $"  Tamaño: {fileInfo.Length / 1024 / 1024} MB");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al verificar archivo: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Método alternativo para parchear boot cuando el método estándar falla
        /// </summary>
        private async Task<string?> MetodoAlternativoParcheo(string bootPath, string apkPath)
        {
            try
            {
                OnLogMessage?.Invoke(this, "[INFO] Usando metodo de parcheado directo...");
                
                // Simplemente copiar el boot original y marcarlo como parcheado
                // Esto es útil cuando el boot no tiene ramdisk o tiene estructura no estándar
                string tempFolder = Path.Combine(Path.GetTempPath(), "TT-Tool-Magisk");
                Directory.CreateDirectory(tempFolder);
                
                string patchedPath = Path.Combine(tempFolder, "magisk_patched_alternative.img");
                File.Copy(bootPath, patchedPath, true);
                
                OnLogMessage?.Invoke(this, "[WARN] No se pudo aplicar Magisk al boot.img");
                OnLogMessage?.Invoke(this, "[INFO] Se recomienda usar Magisk Manager en el dispositivo:");
                OnLogMessage?.Invoke(this, "  1. Instala Magisk Manager en tu telefono");
                OnLogMessage?.Invoke(this, "  2. Copia boot.img al telefono");
                OnLogMessage?.Invoke(this, "  3. Usa Magisk Manager para parchear boot.img");
                OnLogMessage?.Invoke(this, "  4. Copia el boot parcheado de vuelta a PC");
                OnLogMessage?.Invoke(this, "  5. Flashea con Odin");
                
                return null; // Indicar que el parcheado falló
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"[ERROR] Metodo alternativo fallo: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Comprime un archivo con LZ4
        /// </summary>
        private async Task ComprimirLZ4(string inputPath, string outputPath)
        {
            await Task.Run(() =>
            {
                using (var inputStream = File.OpenRead(inputPath))
                using (var outputStream = File.Create(outputPath))
                {
                    // Usar K4os.Compression.LZ4 para comprimir
                    var settings = new K4os.Compression.LZ4.Streams.LZ4EncoderSettings
                    {
                        CompressionLevel = K4os.Compression.LZ4.LZ4Level.L09_HC,
                        ChainBlocks = true
                    };
                    
                    using (var lz4Stream = K4os.Compression.LZ4.Streams.LZ4Stream.Encode(outputStream, settings, false))
                    {
                        inputStream.CopyTo(lz4Stream);
                    }
                }
            });
        }
        
        /// <summary>
        /// Crea un archivo TAR con el boot parcheado
        /// </summary>
        private async Task CrearTARConBoot(string bootPath, string tarPath)
        {
            await Task.Run(() =>
            {
                using (var tarStream = File.Create(tarPath))
                {
                    // Obtener nombre del archivo boot
                    string bootFileName = Path.GetFileName(bootPath);
                    if (!bootFileName.Contains("boot"))
                    {
                        bootFileName = bootPath.Contains(".lz4") ? "boot.img.lz4" : "boot.img";
                    }
                    
                    // Leer datos del boot
                    byte[] bootData = File.ReadAllBytes(bootPath);
                    long fileSize = bootData.Length;
                    
                    // Crear header TAR (512 bytes)
                    byte[] header = new byte[512];
                    
                    // Nombre del archivo (offset 0, 100 bytes)
                    byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(bootFileName);
                    Array.Copy(nameBytes, 0, header, 0, Math.Min(nameBytes.Length, 100));
                    
                    // Modo (offset 100, 8 bytes) - "0000644\0"
                    byte[] modeBytes = System.Text.Encoding.ASCII.GetBytes("0000644");
                    Array.Copy(modeBytes, 0, header, 100, modeBytes.Length);
                    header[107] = 0;
                    
                    // UID (offset 108, 8 bytes) - "0000000\0"
                    byte[] uidBytes = System.Text.Encoding.ASCII.GetBytes("0000000");
                    Array.Copy(uidBytes, 0, header, 108, uidBytes.Length);
                    header[115] = 0;
                    
                    // GID (offset 116, 8 bytes) - "0000000\0"
                    byte[] gidBytes = System.Text.Encoding.ASCII.GetBytes("0000000");
                    Array.Copy(gidBytes, 0, header, 116, gidBytes.Length);
                    header[123] = 0;
                    
                    // Tamaño del archivo (offset 124, 12 bytes) - en octal
                    string sizeOctal = Convert.ToString(fileSize, 8).PadLeft(11, '0');
                    byte[] sizeBytes = System.Text.Encoding.ASCII.GetBytes(sizeOctal);
                    Array.Copy(sizeBytes, 0, header, 124, sizeBytes.Length);
                    header[135] = 0;
                    
                    // Timestamp (offset 136, 12 bytes) - en octal
                    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string timeOctal = Convert.ToString(timestamp, 8).PadLeft(11, '0');
                    byte[] timeBytes = System.Text.Encoding.ASCII.GetBytes(timeOctal);
                    Array.Copy(timeBytes, 0, header, 136, timeBytes.Length);
                    header[147] = 0;
                    
                    // Checksum placeholder (offset 148, 8 bytes) - primero espacios
                    for (int i = 148; i < 156; i++)
                        header[i] = 32; // espacio
                    
                    // Typeflag (offset 156, 1 byte) - '0' para archivo regular
                    header[156] = (byte)'0';
                    
                    // Magic (offset 257, 6 bytes) - "ustar\0"
                    byte[] magicBytes = System.Text.Encoding.ASCII.GetBytes("ustar");
                    Array.Copy(magicBytes, 0, header, 257, magicBytes.Length);
                    header[262] = 0;
                    
                    // Version (offset 263, 2 bytes) - "00"
                    header[263] = (byte)'0';
                    header[264] = (byte)'0';
                    
                    // Calcular checksum
                    int checksum = 0;
                    for (int i = 0; i < 512; i++)
                        checksum += header[i];
                    
                    // Escribir checksum (offset 148, 8 bytes)
                    string checksumOctal = Convert.ToString(checksum, 8).PadLeft(6, '0');
                    byte[] checksumBytes = System.Text.Encoding.ASCII.GetBytes(checksumOctal);
                    Array.Copy(checksumBytes, 0, header, 148, checksumBytes.Length);
                    header[154] = 0;
                    header[155] = 32; // espacio
                    
                    // Escribir header
                    tarStream.Write(header, 0, 512);
                    
                    // Escribir datos del archivo
                    tarStream.Write(bootData, 0, bootData.Length);
                    
                    // Padding para alinear a 512 bytes
                    int padding = 512 - (int)(fileSize % 512);
                    if (padding < 512)
                    {
                        byte[] paddingBytes = new byte[padding];
                        tarStream.Write(paddingBytes, 0, padding);
                    }
                    
                    // Escribir dos bloques vacíos al final (EOF)
                    byte[] eof = new byte[1024];
                    tarStream.Write(eof, 0, 1024);
                }
                
                OnLogMessage?.Invoke(this, $"[OK] Archivo TAR creado: {new FileInfo(tarPath).Length / 1024 / 1024} MB");
            });
        }

        /// <summary>
        /// Lista todos los dispositivos USB conectados (para debug)
        /// </summary>
        public void ListarDispositivosUSB()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Dispositivos USB conectados:");
                OnLogMessage?.Invoke(this, "");

                var usbDevices = UsbDevice.AllDevices;
                int count = 0;

                foreach (UsbRegistry usbRegistry in usbDevices)
                {
                    count++;
                    OnLogMessage?.Invoke(this, $"Dispositivo #{count}:");
                    OnLogMessage?.Invoke(this, $"  VID: 0x{usbRegistry.Vid:X4}");
                    OnLogMessage?.Invoke(this, $"  PID: 0x{usbRegistry.Pid:X4}");
                    OnLogMessage?.Invoke(this, $"  Nombre: {usbRegistry.Name}");
                    
                    if (usbRegistry.Vid == SAMSUNG_VENDOR_ID)
                    {
                        OnLogMessage?.Invoke(this, "  ⭐ Dispositivo Samsung detectado");
                    }
                    
                    OnLogMessage?.Invoke(this, "");
                }

                if (count == 0)
                {
                    OnLogMessage?.Invoke(this, "No se encontraron dispositivos USB");
                }
                else
                {
                    OnLogMessage?.Invoke(this, $"Total: {count} dispositivos");
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al listar dispositivos: {ex.Message}");
            }
        }
    }
}


