using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Manager para operaciones MTK en dispositivos Motorola
    /// </summary>
    public class MotorolaMTKManager
    {
        private LogManager? _logManager;
        public event EventHandler<string>? OnLogMessage;

        public void SetLogManager(LogManager logManager)
        {
            _logManager = logManager;
        }

        private void Log(string mensaje, TipoLog tipo = TipoLog.Info)
        {
            _logManager?.AgregarLog(mensaje, tipo);
            OnLogMessage?.Invoke(this, mensaje);
        }

        /// <summary>
        /// Verifica si MTKClient está instalado
        /// </summary>
        public async Task<bool> VerificarMTKClient()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c mtk --help",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Detecta dispositivos MTK conectados
        /// </summary>
        public async Task<List<string>> DetectarDispositivosMTK()
        {
            var dispositivos = new List<string>();

            try
            {
                var ports = System.IO.Ports.SerialPort.GetPortNames();
                
                foreach (var port in ports)
                {
                    // Buscar puertos MTK (VID:PID=0E8D:*)
                    var portInfo = await ObtenerInfoPuerto(port);
                    if (portInfo.Contains("0E8D", StringComparison.OrdinalIgnoreCase))
                    {
                        dispositivos.Add($"{port} - MTK Device");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error detecting MTK devices: {ex.Message}", TipoLog.Error);
            }

            return dispositivos;
        }

        private async Task<string> ObtenerInfoPuerto(string puerto)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"Get-PnpDevice | Where-Object {{$_.Name -like '*{puerto}*'}} | Select-Object -ExpandProperty InstanceId\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                return output;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Ejecuta comando MTKClient
        /// </summary>
        private async Task<(bool success, string output)> EjecutarComandoMTK(string comando, string preloader, string loader)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c mtk {comando} --preloader \"{preloader}\" --loader \"{loader}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                var output = new StringBuilder();
                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.AppendLine(e.Data);
                        Log(e.Data);
                    }
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.AppendLine(e.Data);
                        Log(e.Data, TipoLog.Advertencia);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                return (process.ExitCode == 0, output.ToString());
            }
            catch (Exception ex)
            {
                Log($"Error executing MTK command: {ex.Message}", TipoLog.Error);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Lee una partición del dispositivo
        /// </summary>
        public async Task<bool> LeerParticion(string nombreParticion, string rutaSalida, string preloader, string loader)
        {
            Log($"Reading partition: {nombreParticion}");
            Log($"Output: {rutaSalida}");

            var (success, output) = await EjecutarComandoMTK($"r {nombreParticion} \"{rutaSalida}\"", preloader, loader);

            if (success)
            {
                Log($"✓ Partition {nombreParticion} read successfully", TipoLog.Exito);
            }
            else
            {
                Log($"✗ Failed to read partition {nombreParticion}", TipoLog.Error);
            }

            return success;
        }

        /// <summary>
        /// Escribe una partición al dispositivo
        /// </summary>
        public async Task<bool> EscribirParticion(string nombreParticion, string rutaArchivo, string preloader, string loader)
        {
            Log($"Writing partition: {nombreParticion}");
            Log($"File: {Path.GetFileName(rutaArchivo)}");

            var (success, output) = await EjecutarComandoMTK($"w {nombreParticion} \"{rutaArchivo}\"", preloader, loader);

            if (success)
            {
                Log($"✓ Partition {nombreParticion} written successfully", TipoLog.Exito);
            }
            else
            {
                Log($"✗ Failed to write partition {nombreParticion}", TipoLog.Error);
            }

            return success;
        }

        /// <summary>
        /// Borra una partición
        /// </summary>
        public async Task<bool> BorrarParticion(string nombreParticion, string preloader, string loader)
        {
            Log($"Erasing partition: {nombreParticion}");

            var (success, output) = await EjecutarComandoMTK($"e {nombreParticion}", preloader, loader);

            if (success)
            {
                Log($"✓ Partition {nombreParticion} erased successfully", TipoLog.Exito);
            }
            else
            {
                Log($"✗ Failed to erase partition {nombreParticion}", TipoLog.Error);
            }

            return success;
        }

        /// <summary>
        /// Realiza backup de particiones críticas
        /// </summary>
        public async Task<bool> BackupParticionesCriticas(string carpetaDestino, string preloader, string loader)
        {
            Log("=== STARTING CRITICAL PARTITIONS BACKUP ===");
            Log($"Destination: {carpetaDestino}");

            Directory.CreateDirectory(carpetaDestino);

            var particionesCriticas = new[]
            {
                "boot_a", "boot_b", "vbmeta_a", "vbmeta_b",
                "vbmeta_system_a", "vbmeta_system_b",
                "nvram", "persist", "metadata", "misc"
            };

            int exitosas = 0;
            int fallidas = 0;

            foreach (var particion in particionesCriticas)
            {
                string rutaSalida = Path.Combine(carpetaDestino, $"{particion}.img");
                bool resultado = await LeerParticion(particion, rutaSalida, preloader, loader);

                if (resultado)
                    exitosas++;
                else
                    fallidas++;
            }

            Log("");
            Log($"=== BACKUP COMPLETED ===");
            Log($"Successful: {exitosas}/{particionesCriticas.Length}");
            Log($"Failed: {fallidas}/{particionesCriticas.Length}");

            return fallidas == 0;
        }
    }
}
