using System.Diagnostics;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Gestor de operaciones de desbloqueo para dispositivos Android
    /// </summary>
    public class UnlockManager
    {
        public event EventHandler<string>? OnLogMessage;
        public event EventHandler<string>? OnOperationComplete;

        private readonly string _adbPath;
        private readonly string _fastbootPath;

        public UnlockManager()
        {
            // Intentar usar ejecutables embebidos primero
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var toolsDir = Path.Combine(baseDir, "Resources", "Tools");
            
            _adbPath = Path.Combine(toolsDir, "adb.exe");
            _fastbootPath = Path.Combine(toolsDir, "fastboot.exe");

            // Si no existen, usar del PATH del sistema
            if (!File.Exists(_adbPath))
            {
                _adbPath = "adb";
            }
            if (!File.Exists(_fastbootPath))
            {
                _fastbootPath = "fastboot";
            }
        }

        /// <summary>
        /// Desbloqueo FRP (Factory Reset Protection) - Google Account
        /// </summary>
        public async Task<bool> UnlockFRP(string deviceId)
        {
            try
            {
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");
                OnLogMessage?.Invoke(this, "Iniciando proceso de UNLOCK FRP...");
                OnLogMessage?.Invoke(this, $"Dispositivo: {deviceId}");
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");

                // Verificar conexión ADB
                if (!await VerificarConexionADB(deviceId))
                {
                    OnLogMessage?.Invoke(this, "❌ Dispositivo no conectado o ADB no disponible");
                    return false;
                }

                // Comandos FRP bypass (métodos comunes)
                var comandos = new List<(string comando, string descripcion)>
                {
                    ("shell content insert --uri content://settings/secure --bind name:s:user_setup_complete --bind value:s:1", "Marcando setup como completo"),
                    ("shell am start -n com.google.android.gsf.login/", "Iniciando GSF Login"),
                    ("shell pm disable-user --user 0 com.android.vending", "Deshabilitando Play Store"),
                    ("shell pm clear com.google.android.gms", "Limpiando Google Play Services"),
                };

                foreach (var (comando, descripcion) in comandos)
                {
                    OnLogMessage?.Invoke(this, $"→ {descripcion}...");
                    var resultado = await EjecutarComandoADB(deviceId, comando);
                    OnLogMessage?.Invoke(this, resultado.exito ? "✓ Exitoso" : $"⚠ {resultado.error}");
                    await Task.Delay(500);
                }

                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");
                OnLogMessage?.Invoke(this, "✓ Proceso FRP completado");
                OnLogMessage?.Invoke(this, "⚠ Reinicia el dispositivo y verifica");
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");

                OnOperationComplete?.Invoke(this, "FRP Unlock completado. Reinicie el dispositivo.");
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"❌ Error en FRP: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Desbloqueo del Bootloader
        /// </summary>
        public async Task<bool> UnlockBootloader(string deviceId)
        {
            try
            {
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");
                OnLogMessage?.Invoke(this, "Iniciando UNLOCK BOOTLOADER...");
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");

                // Reiniciar en modo fastboot
                OnLogMessage?.Invoke(this, "→ Reiniciando en modo fastboot...");
                await EjecutarComandoADB(deviceId, "reboot bootloader");
                await Task.Delay(5000);

                // Verificar estado del bootloader
                OnLogMessage?.Invoke(this, "→ Verificando estado del bootloader...");
                var estado = await EjecutarComandoFastboot("getvar unlocked");
                OnLogMessage?.Invoke(this, $"Estado: {estado.salida}");

                // Intentar desbloquear
                OnLogMessage?.Invoke(this, "→ Ejecutando comando de desbloqueo...");
                OnLogMessage?.Invoke(this, "⚠ ATENCIÓN: Esto borrará todos los datos del dispositivo");
                
                var resultado = await EjecutarComandoFastboot("oem unlock");
                
                if (resultado.exito)
                {
                    OnLogMessage?.Invoke(this, "✓ Comando enviado exitosamente");
                    OnLogMessage?.Invoke(this, "→ Confirma en el dispositivo usando los botones de volumen");
                    OnLogMessage?.Invoke(this, "→ Presiona el botón de encendido para confirmar");
                }
                else
                {
                    OnLogMessage?.Invoke(this, $"⚠ {resultado.error}");
                    OnLogMessage?.Invoke(this, "Intentando comando alternativo...");
                    resultado = await EjecutarComandoFastboot("flashing unlock");
                }

                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");
                OnLogMessage?.Invoke(this, "⚠ Verifica el dispositivo y confirma manualmente");
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");

                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"❌ Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtener información completa del dispositivo
        /// </summary>
        public async Task<Dictionary<string, string>> GetDeviceInfo(string deviceId)
        {
            var info = new Dictionary<string, string>();

            try
            {
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");
                OnLogMessage?.Invoke(this, "Obteniendo información del dispositivo...");
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");

                // Propiedades a obtener
                var propiedades = new Dictionary<string, string>
                {
                    { "ro.product.model", "Modelo" },
                    { "ro.product.brand", "Marca" },
                    { "ro.product.manufacturer", "Fabricante" },
                    { "ro.build.version.release", "Versión Android" },
                    { "ro.build.version.sdk", "SDK Level" },
                    { "ro.serialno", "Número de Serie" },
                    { "ro.product.cpu.abi", "Arquitectura CPU" },
                    { "ro.build.id", "Build ID" },
                    { "ro.build.display.id", "Build Display" },
                    { "ro.bootloader", "Bootloader" },
                    { "ro.build.fingerprint", "Fingerprint" },
                    { "ro.build.host", "Build Host" },
                    { "ro.build.user", "Build User" },
                    { "ro.build.date", "Fecha de Build" }
                };

                foreach (var (prop, nombre) in propiedades)
                {
                    var resultado = await EjecutarComandoADB(deviceId, $"shell getprop {prop}");
                    if (resultado.exito && !string.IsNullOrWhiteSpace(resultado.salida))
                    {
                        var valor = resultado.salida.Trim();
                        info[nombre] = valor;
                        OnLogMessage?.Invoke(this, $"• {nombre}: {valor}");
                    }
                }

                // Información de almacenamiento
                var storage = await EjecutarComandoADB(deviceId, "shell df /data");
                if (storage.exito)
                {
                    OnLogMessage?.Invoke(this, $"• Almacenamiento: {storage.salida}");
                    info["Almacenamiento"] = storage.salida;
                }

                // Información de batería
                var battery = await EjecutarComandoADB(deviceId, "shell dumpsys battery");
                if (battery.exito)
                {
                    var lineas = battery.salida.Split('\n');
                    foreach (var linea in lineas)
                    {
                        if (linea.Contains("level:"))
                        {
                            var nivel = linea.Split(':')[1].Trim();
                            info["Batería"] = $"{nivel}%";
                            OnLogMessage?.Invoke(this, $"• Batería: {nivel}%");
                            break;
                        }
                    }
                }

                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");
                OnLogMessage?.Invoke(this, "✓ Información recopilada exitosamente");
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"❌ Error obteniendo info: {ex.Message}");
            }

            return info;
        }

        /// <summary>
        /// Flashear archivos al dispositivo
        /// </summary>
        public async Task<bool> FlashFiles(string deviceId, List<string> archivos)
        {
            try
            {
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");
                OnLogMessage?.Invoke(this, "Iniciando proceso de FLASH...");
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");

                // Reiniciar en fastboot
                OnLogMessage?.Invoke(this, "→ Reiniciando en modo fastboot...");
                await EjecutarComandoADB(deviceId, "reboot bootloader");
                await Task.Delay(5000);

                foreach (var archivo in archivos)
                {
                    if (!File.Exists(archivo))
                    {
                        OnLogMessage?.Invoke(this, $"⚠ Archivo no encontrado: {archivo}");
                        continue;
                    }

                    var nombreArchivo = Path.GetFileName(archivo);
                    var extension = Path.GetExtension(archivo).ToLower();

                    OnLogMessage?.Invoke(this, $"→ Flasheando {nombreArchivo}...");

                    // Determinar partición según tipo de archivo
                    string particion = extension switch
                    {
                        ".img" when nombreArchivo.Contains("boot") => "boot",
                        ".img" when nombreArchivo.Contains("recovery") => "recovery",
                        ".img" when nombreArchivo.Contains("system") => "system",
                        ".zip" => "flashall", // For full ROM packages
                        _ => "boot"
                    };

                    var resultado = await EjecutarComandoFastboot($"flash {particion} \"{archivo}\"");
                    OnLogMessage?.Invoke(this, resultado.exito ? "✓ Exitoso" : $"⚠ Error: {resultado.error}");
                }

                OnLogMessage?.Invoke(this, "→ Reiniciando dispositivo...");
                await EjecutarComandoFastboot("reboot");

                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");
                OnLogMessage?.Invoke(this, "✓ Proceso de flash completado");
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");

                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"❌ Error en flash: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ejecutar comando ADB personalizado
        /// </summary>
        private async Task<(bool exito, string salida, string error)> EjecutarComandoADB(string deviceId, string comando)
        {
            try
            {
                var proceso = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _adbPath,
                        Arguments = $"-s {deviceId} {comando}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(_adbPath) ?? ""
                    }
                };

                proceso.Start();
                var salida = await proceso.StandardOutput.ReadToEndAsync();
                var error = await proceso.StandardError.ReadToEndAsync();
                await proceso.WaitForExitAsync();

                return (proceso.ExitCode == 0, salida, error);
            }
            catch (Exception ex)
            {
                return (false, "", ex.Message);
            }
        }

        /// <summary>
        /// Ejecutar comando Fastboot
        /// </summary>
        private async Task<(bool exito, string salida, string error)> EjecutarComandoFastboot(string comando)
        {
            try
            {
                var proceso = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _fastbootPath,
                        Arguments = comando,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(_fastbootPath) ?? ""
                    }
                };

                proceso.Start();
                var salida = await proceso.StandardOutput.ReadToEndAsync();
                var error = await proceso.StandardError.ReadToEndAsync();
                await proceso.WaitForExitAsync();

                return (proceso.ExitCode == 0, salida, error);
            }
            catch (Exception ex)
            {
                return (false, "", ex.Message);
            }
        }

        /// <summary>
        /// Verificar si el dispositivo está conectado via ADB
        /// </summary>
        private async Task<bool> VerificarConexionADB(string deviceId)
        {
            var resultado = await EjecutarComandoADB(deviceId, "get-state");
            return resultado.exito && resultado.salida.Contains("device");
        }

        /// <summary>
        /// Root del dispositivo (requiere bootloader desbloqueado)
        /// </summary>
        public async Task<bool> RootDevice(string deviceId)
        {
            try
            {
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");
                OnLogMessage?.Invoke(this, "Iniciando proceso de ROOT...");
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");

                // Verificar si ya tiene root
                var tieneRoot = await EjecutarComandoADB(deviceId, "shell su -c 'echo test'");
                if (tieneRoot.exito && tieneRoot.salida.Contains("test"))
                {
                    OnLogMessage?.Invoke(this, "✓ El dispositivo ya tiene ROOT");
                    return true;
                }

                OnLogMessage?.Invoke(this, "→ Reiniciando en modo recovery...");
                await EjecutarComandoADB(deviceId, "reboot recovery");
                await Task.Delay(5000);

                OnLogMessage?.Invoke(this, "⚠ NOTA: Para rootear, necesitas:");
                OnLogMessage?.Invoke(this, "  1. Bootloader desbloqueado");
                OnLogMessage?.Invoke(this, "  2. Magisk o SuperSU flasheado");
                OnLogMessage?.Invoke(this, "  3. Custom recovery instalado");
                OnLogMessage?.Invoke(this, "═══════════════════════════════════════");

                return false;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"❌ Error: {ex.Message}");
                return false;
            }
        }
    }
}

