using System.Diagnostics;
using System.IO.Ports;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Gestor de dispositivos para escaneo de USB (ADB) y puertos COM
    /// </summary>
    public class DeviceManager
    {
        public event EventHandler<string>? OnLogMessage;
        private readonly string _adbPath;

        public DeviceManager()
        {
            // Usar adb.exe desde Resources/Tools
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _adbPath = Path.Combine(baseDir, "Resources", "Tools", "adb.exe");

            // Si no existe, intentar desde la ubicación del proyecto (modo desarrollo)
            if (!File.Exists(_adbPath))
            {
                var projectPath = Path.Combine(baseDir, "..", "..", "..", "Resources", "Tools", "adb.exe");
                if (File.Exists(projectPath))
                {
                    _adbPath = Path.GetFullPath(projectPath);
                }
            }

            // Última opción: usar del PATH del sistema
            if (!File.Exists(_adbPath))
            {
                _adbPath = "adb";
            }
        }

        /// <summary>
        /// Escanea dispositivos conectados mediante ADB
        /// </summary>
        public async Task<List<string>> EscanearDispositivosADB()
        {
            var dispositivos = new List<string>();
            
            try
            {
                // Asegurarse de que el directorio de trabajo sea donde están las DLLs
                var workingDir = Path.GetDirectoryName(_adbPath);
                if (string.IsNullOrEmpty(workingDir))
                {
                    workingDir = AppDomain.CurrentDomain.BaseDirectory;
                }

                var proceso = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _adbPath,
                        Arguments = "devices",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = workingDir
                    }
                };

                proceso.Start();
                string output = await proceso.StandardOutput.ReadToEndAsync();
                await proceso.WaitForExitAsync();

                // Parsear la salida de adb devices
                var lineas = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var linea in lineas)
                {
                    if (linea.Contains("\tdevice"))
                    {
                        var partes = linea.Split('\t');
                        if (partes.Length > 0)
                        {
                            dispositivos.Add($"ADB: {partes[0].Trim()}");
                        }
                    }
                }

                // Solo mostrar resultado final
                if (dispositivos.Count == 0)
                {
                    OnLogMessage?.Invoke(this, "⚠ No se encontraron dispositivos ADB");
                }
                else
                {
                    OnLogMessage?.Invoke(this, $"✓ {dispositivos.Count} dispositivo(s) ADB conectado(s)");
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"⚠ Error ADB: {ex.Message}");
                // Si ADB no está instalado o disponible, agregar dispositivos de ejemplo
                dispositivos.Add("ADB: No disponible (instalar Android Debug Bridge)");
            }

            return dispositivos;
        }

        /// <summary>
        /// Escanea puertos COM disponibles
        /// </summary>
        public List<string> EscanearPuertosCOM()
        {
            var puertos = new List<string>();

            try
            {
                string[] nombresPublicos = SerialPort.GetPortNames();
                
                foreach (string puerto in nombresPublicos)
                {
                    puertos.Add($"Puerto: {puerto}");
                }

                // Solo mostrar resultado final
                if (puertos.Count == 0)
                {
                    OnLogMessage?.Invoke(this, "⚠ No se encontraron puertos COM");
                    puertos.Add("No hay puertos COM disponibles");
                }
                else
                {
                    OnLogMessage?.Invoke(this, $"✓ {puertos.Count} puerto(s) COM detectado(s)");
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al escanear puertos COM: {ex.Message}");
            }

            return puertos;
        }

        /// <summary>
        /// Escanea todos los dispositivos
        /// </summary>
        public async Task<(List<string> adb, List<string> com)> EscanearTodosLosDispositivos()
        {
            OnLogMessage?.Invoke(this, "=== Iniciando escaneo completo de dispositivos ===");

            var dispositivosADB = await EscanearDispositivosADB();
            var puertosCOM = EscanearPuertosCOM();

            OnLogMessage?.Invoke(this, "=== Escaneo completo finalizado ===");

            return (dispositivosADB, puertosCOM);
        }
    }
}

