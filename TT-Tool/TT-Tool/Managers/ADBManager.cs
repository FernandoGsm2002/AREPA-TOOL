using System.Diagnostics;
using System.Text;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Gestor para comandos ADB (Android Debug Bridge)
    /// </summary>
    public class ADBManager
    {
        public event EventHandler<string>? OnLogMessage;
        
        private string _adbPath;
        
        public ADBManager()
        {
            // Buscar ADB en Resources/Tools (con T mayúscula)
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string resourcesPath = Path.Combine(baseDir, "Resources", "Tools", "adb.exe");
            
            if (File.Exists(resourcesPath))
            {
                _adbPath = resourcesPath;
            }
            else
            {
                // Intentar buscar en el directorio del proyecto (para desarrollo)
                string projectPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Resources", "Tools", "adb.exe"));
                if (File.Exists(projectPath))
                {
                    _adbPath = projectPath;
                }
                else
                {
                    // Intentar en el directorio actual
                    string currentPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Tools", "adb.exe");
                    if (File.Exists(currentPath))
                    {
                        _adbPath = currentPath;
                    }
                    else
                    {
                        // Usar PATH del sistema como último recurso
                        _adbPath = "adb";
                    }
                }
            }
        }
        
        /// <summary>
        /// Ejecuta un comando ADB
        /// </summary>
        public async Task<string> ExecuteAdbCommandAsync(string arguments)
        {
            try
            {
                // Verificar que el archivo existe
                if (!File.Exists(_adbPath) && _adbPath != "adb")
                {
                    return $"Error: ADB no encontrado en {_adbPath}";
                }
                
                var processInfo = new ProcessStartInfo
                {
                    FileName = _adbPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                using var process = new Process { StartInfo = processInfo };
                
                var output = new StringBuilder();
                var error = new StringBuilder();
                
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.AppendLine(e.Data);
                    }
                };
                
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        error.AppendLine(e.Data);
                    }
                };
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                await process.WaitForExitAsync();
                
                if (error.Length > 0)
                {
                    return error.ToString();
                }
                
                return output.ToString();
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error ejecutando ADB: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Obtiene la ruta de ADB
        /// </summary>
        public string ObtenerRutaADB()
        {
            return _adbPath;
        }
        
        /// <summary>
        /// Verifica si ADB está disponible
        /// </summary>
        public async Task<bool> VerificarADB()
        {
            try
            {
                OnLogMessage?.Invoke(this, $"Verificando ADB en: {_adbPath}");
                
                if (!File.Exists(_adbPath) && _adbPath != "adb")
                {
                    OnLogMessage?.Invoke(this, $"⚠ ADB no encontrado en: {_adbPath}");
                    return false;
                }
                
                var result = await ExecuteAdbCommandAsync("version");
                
                if (result.Contains("Android Debug Bridge"))
                {
                    OnLogMessage?.Invoke(this, "✓ ADB disponible");
                    return true;
                }
                else
                {
                    OnLogMessage?.Invoke(this, $"⚠ Respuesta inesperada de ADB: {result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"⚠ Error al verificar ADB: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Obtiene dispositivos conectados
        /// </summary>
        public async Task<List<string>> ObtenerDispositivos()
        {
            try
            {
                var result = await ExecuteAdbCommandAsync("devices");
                var devices = new List<string>();
                
                var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines.Skip(1)) // Skip "List of devices attached"
                {
                    if (line.Contains("device") && !line.Contains("offline"))
                    {
                        var parts = line.Split('\t');
                        if (parts.Length > 0)
                        {
                            devices.Add(parts[0].Trim());
                        }
                    }
                }
                
                return devices;
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
