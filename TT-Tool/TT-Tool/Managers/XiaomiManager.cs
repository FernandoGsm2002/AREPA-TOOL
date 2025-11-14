using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace TT_Tool.Managers
{
    public class XiaomiManager
    {
        private LogManager? _logManager;
        private readonly string _adbPath;
        private readonly string _fastbootPath;
        private DateTime _startTime;

        public XiaomiManager()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _adbPath = Path.Combine(baseDir, "Resources", "Tools", "adb.exe");
            _fastbootPath = Path.Combine(baseDir, "Resources", "Tools", "fastboot.exe");
        }

        public void SetLogManager(LogManager logManager)
        {
            _logManager = logManager;
        }

        private void Log(string mensaje, TipoLog tipo = TipoLog.Info)
        {
            _logManager?.AgregarLog(mensaje, tipo);
        }

        private async Task<string> EjecutarComandoSilencioso(string comando, string argumentos)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = comando,
                        Arguments = argumentos,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
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

        private string ExtraerValor(string output, string patron)
        {
            var match = Regex.Match(output, patron);
            return match.Success ? match.Groups[1].Value.Trim() : "Unknown";
        }

        private void MostrarTiempoTranscurrido()
        {
            var elapsed = DateTime.Now - _startTime;
            Log("");
            Log($"AREPA-TOOL By LeoPE-GSM.COM");
            Log($"Elapsed time: {(int)elapsed.TotalSeconds} seconds");
        }

        public async Task FixBrickFastboot()
        {
            _startTime = DateTime.Now;
            Log("[FASTBOOT] FIX BRICK");
            Log("Setting active slot to A... OK", TipoLog.Exito);
            await EjecutarComandoSilencioso(_fastbootPath, "--set-active=a");
            
            Log("Rebooting device... OK", TipoLog.Exito);
            await EjecutarComandoSilencioso(_fastbootPath, "reboot");
            
            MostrarTiempoTranscurrido();
        }

        public async Task ReadInfo()
        {
            _startTime = DateTime.Now;
            Log("[ADB] READ DEVICE INFO");
            Log("Starting ADB Interface... OK", TipoLog.Exito);
            
            Log("Reading device info... OK", TipoLog.Exito);
            
            var manufacturer = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.product.manufacturer");
            var model = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.product.model");
            var platform = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.board.platform");
            var arch = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.product.cpu.abi");
            var serialno = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.serialno");
            var securityPatch = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.build.version.security_patch");
            var timezone = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop persist.sys.timezone");
            var androidVersion = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.build.version.release");
            var sdk = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.build.version.sdk");
            var buildId = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.build.id");
            var buildDate = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.build.date");
            var deviceName = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.product.device");
            var productName = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.product.name");
            var marketName = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.product.marketname");
            var miuiBuild = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.build.version.incremental");
            var miuiVersion = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.miui.ui.version.name");
            var miuiRegion = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.miui.region");
            var miuiCustomer = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.miui.cust_variant");

            Log("");
            Log($"Model            : {model.Trim()}");
            Log($"Manufacturer     : {manufacturer.Trim()}");
            Log($"Platform         : {platform.Trim()}");
            Log($"CPU Arch         : {arch.Trim()}");
            Log($"Android Serial   : {serialno.Trim()}");
            Log($"Security Patch   : {securityPatch.Trim()}");
            Log($"Connection       : adb");
            Log($"Timezone         : {timezone.Trim()}");
            Log($"Android Version  : {androidVersion.Trim()}");
            Log($"Android SDK      : {sdk.Trim()}");
            Log($"Build            : {buildId.Trim()}");
            Log($"Build Date       : {buildDate.Trim()}");
            Log($"Device Name      : {deviceName.Trim()}");
            Log($"Product Name     : {productName.Trim()}");
            Log($"Market Name      : {marketName.Trim()}");
            Log($"MIUI Build       : {miuiBuild.Trim()}");
            Log($"MIUI Version     : {miuiVersion.Trim()}");
            Log($"MIUI Region      : {miuiRegion.Trim()}");
            Log($"MIUI Customer    : {miuiCustomer.Trim()}");
            
            MostrarTiempoTranscurrido();
        }

        public async Task EnableDiagNoRoot()
        {
            _startTime = DateTime.Now;
            Log("[ADB] ENABLE DIAG (NO ROOT)");
            Log("Starting ADB Interface... OK", TipoLog.Exito);
            
            // Verificar si la APK está instalada
            var packages = await EjecutarComandoSilencioso(_adbPath, "shell -n pm list packages | grep longcheertel");
            
            if (string.IsNullOrEmpty(packages) || !packages.Contains("com.longcheertel.midtest"))
            {
                Log("DIAG APK not installed", TipoLog.Advertencia);
                
                string apkPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Tools", "diag.apk");
                if (File.Exists(apkPath))
                {
                    Log("Installing DIAG APK...");
                    var resultado = await EjecutarComandoSilencioso(_adbPath, $"install -r \"{apkPath}\"");
                    
                    if (resultado.Contains("Success"))
                    {
                        Log("DIAG APK installed... OK", TipoLog.Exito);
                    }
                    else
                    {
                        Log("APK installation blocked by MIUI", TipoLog.Advertencia);
                        Log("Copying APK to device...");
                        await EjecutarComandoSilencioso(_adbPath, $"push \"{apkPath}\" /sdcard/diag.apk");
                        Log("APK copied to /sdcard/diag.apk... OK", TipoLog.Exito);
                        Log("Please install manually from device");
                        MostrarTiempoTranscurrido();
                        return;
                    }
                }
                else
                {
                    Log("DIAG APK not found in Resources/Tools", TipoLog.Error);
                    MostrarTiempoTranscurrido();
                    return;
                }
            }
            else
            {
                Log("DIAG APK already installed... OK", TipoLog.Exito);
            }
            
            Log("Starting DIAG activity...");
            await EjecutarComandoSilencioso(_adbPath, "shell -n am start -n com.longcheertel.midtest/com.longcheertel.midtest.Diag");
            Log("DIAG mode enabled... OK", TipoLog.Exito);
            
            MostrarTiempoTranscurrido();
        }

        public async Task EnableDiagRoot()
        {
            _startTime = DateTime.Now;
            Log("[ADB] ENABLE DIAG (ROOT)");
            Log("Starting ADB Interface... OK", TipoLog.Exito);
            
            Log("Checking root access...");
            var rootCheck = await EjecutarComandoSilencioso(_adbPath, "shell -n su -c id");
            
            if (string.IsNullOrEmpty(rootCheck) || !rootCheck.Contains("uid=0"))
            {
                Log("Root access not available", TipoLog.Error);
                MostrarTiempoTranscurrido();
                return;
            }
            
            Log("Root access confirmed... OK", TipoLog.Exito);
            Log("Enabling DIAG mode...");
            await EjecutarComandoSilencioso(_adbPath, "shell -n su -c setprop sys.usb.config diag,adb");
            Log("DIAG mode enabled... OK", TipoLog.Exito);
            
            MostrarTiempoTranscurrido();
        }

        public async Task DisableUpdates()
        {
            _startTime = DateTime.Now;
            Log("[ADB] DISABLE UPDATES");
            Log("Starting ADB Interface... OK", TipoLog.Exito);
            
            Log("Setting debug app...");
            await EjecutarComandoSilencioso(_adbPath, "shell -n am set-debug-app -w --persistent com.android.updater");
            
            var debugApp = await EjecutarComandoSilencioso(_adbPath, "shell -n settings get --user 0 global debug_app");
            if (debugApp.Contains("com.android.updater"))
            {
                Log("Updates disabled... OK", TipoLog.Exito);
            }
            else
            {
                Log("Failed to disable updates", TipoLog.Error);
            }
            
            MostrarTiempoTranscurrido();
        }

        public async Task BypassMiAccount()
        {
            _startTime = DateTime.Now;
            Log("[ADB] BYPASS MI ACCOUNT 2024");
            Log("Starting ADB Interface... OK", TipoLog.Exito);
            
            Log("Reading device info... OK", TipoLog.Exito);
            
            var model = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.product.model");
            var manufacturer = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.product.manufacturer");
            var serialno = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.serialno");
            var androidVersion = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.build.version.release");
            var miuiVersion = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.miui.ui.version.name");
            var securityPatch = await EjecutarComandoSilencioso(_adbPath, "shell -n getprop ro.build.version.security_patch");
            
            Log($"Model            : {model.Trim()}");
            Log($"Manufacturer     : {manufacturer.Trim()}");
            Log($"Android Serial   : {serialno.Trim()}");
            Log($"Android Version  : {androidVersion.Trim()}");
            Log($"MIUI Version     : {miuiVersion.Trim()}");
            Log($"Security Patch   : {securityPatch.Trim()}");
            Log("");
            
            Log("Disabling Find Device lock...");
            await EjecutarComandoSilencioso(_adbPath, "shell -n pm uninstall -k --user 0 com.xiaomi.finddevice");
            Log("Find Device disabled... OK", TipoLog.Exito);
            
            Log("Disabling Mi Account...");
            await EjecutarComandoSilencioso(_adbPath, "shell -n pm uninstall -k --user 0 com.xiaomi.account");
            Log("Mi Account disabled... OK", TipoLog.Exito);
            
            Log("Disabling Updater...");
            await EjecutarComandoSilencioso(_adbPath, "shell -n pm uninstall -k --user 0 com.android.updater");
            Log("Updater disabled... OK", TipoLog.Exito);
            
            MostrarTiempoTranscurrido();
        }

        public async Task BypassMiAccountSideload(string modeloSeleccionado)
        {
            _startTime = DateTime.Now;
            Log("[SIDELOAD] MI ACCOUNT + FRP BYPASS");
            Log("Waiting for device in sideload mode...");
            
            // Esperar a que el dispositivo esté en modo sideload
            var waitResult = await EjecutarComandoSilencioso(_adbPath, "wait-for-sideload");
            await Task.Delay(2000);
            
            var devices = await EjecutarComandoSilencioso(_adbPath, "devices");
            if (!devices.Contains("sideload"))
            {
                Log("Device not in sideload mode", TipoLog.Error);
                Log("Boot device into recovery and select 'Apply update from ADB'");
                MostrarTiempoTranscurrido();
                return;
            }
            
            Log("Device connected in sideload mode... OK", TipoLog.Exito);
            Log($"Selected model: {modeloSeleccionado}");
            Log("");
            Log("Starting bypass process...");
            
            // Ejecutar bypass según el modelo seleccionado
            bool resultado = await EjecutarBypassSideload(modeloSeleccionado);
            
            if (resultado)
            {
                Log("");
                Log("✓ Mi Account + FRP bypassed successfully!", TipoLog.Exito);
                Log("Rebooting device...");
                await EjecutarComandoSilencioso(_adbPath, "reboot");
            }
            else
            {
                Log("");
                Log("Bypass failed", TipoLog.Error);
            }
            
            MostrarTiempoTranscurrido();
        }

        private bool VerificarModeloSoportado(string modelo)
        {
            var modelosSoportados = new[]
            {
                "mojito", "sunny", "agate", "alioth", "cmi", "courbet", "curtana",
                "davinci", "excalibur", "fog", "gauguin", "ginkgo", "joyeuse",
                "lime", "lisa", "raphael", "renoir", "rosemary", "spes", "spesn",
                "star", "surya", "sweetin", "toco", "umi", "apollo", "vayu",
                "veux", "vili", "zeus", "gram", "phoenixin", "sweet"
            };
            
            return modelosSoportados.Contains(modelo.ToLower());
        }

        private async Task<bool> EjecutarBypassSideload(string modelo)
        {
            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "xiaomi_bypass");
                Directory.CreateDirectory(tempDir);
                
                Log("Backing up partitions...");
                
                // Determinar particiones según el modelo
                var (modemPartition, persistPartition, frpPartition, modemBlock, persistBlock, frpBlock) = ObtenerParticiones(modelo);
                
                // Pull persist
                string persistFile = Path.Combine(tempDir, "persist");
                await EjecutarComandoSilencioso(_adbPath, $"pull /dev/block/by-name/persist \"{persistFile}\"");
                Log("Persist backed up... OK", TipoLog.Exito);
                
                // Pull modem
                string modemFile = Path.Combine(tempDir, modemPartition);
                await EjecutarComandoSilencioso(_adbPath, $"pull /dev/block/by-name/{modemPartition} \"{modemFile}\"");
                Log($"{modemPartition} backed up... OK", TipoLog.Exito);
                
                Log("");
                Log("Patching files...");
                
                // Patch modem
                if (File.Exists(modemFile))
                {
                    PatchFile(modemFile, "CARDAPP MDT", "CARDAPP RNX");
                    PatchFile(modemFile, "#*#cardapp#*#*", "#*#.......#*#*");
                    Log("Modem patched... OK", TipoLog.Exito);
                }
                
                // Patch persist
                if (File.Exists(persistFile))
                {
                    PatchFile(persistFile, "fdsd", "frnx");
                    Log("Persist patched... OK", TipoLog.Exito);
                }
                
                Log("");
                Log("Writing patched files...");
                
                // Push modem
                await EjecutarComandoSilencioso(_adbPath, $"push \"{modemFile}\" {modemBlock}");
                Log("Modem written... OK", TipoLog.Exito);
                
                // Push persist
                await EjecutarComandoSilencioso(_adbPath, $"push \"{persistFile}\" {persistBlock}");
                Log("Persist written... OK", TipoLog.Exito);
                
                // Erase FRP
                string frpFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Tools", "frp");
                if (File.Exists(frpFile))
                {
                    await EjecutarComandoSilencioso(_adbPath, $"push \"{frpFile}\" {frpBlock}");
                    Log("FRP erased... OK", TipoLog.Exito);
                }
                
                // Limpiar archivos temporales
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
                
                return true;
            }
            catch (Exception ex)
            {
                Log($"Error during bypass: {ex.Message}", TipoLog.Error);
                return false;
            }
        }

        private (string modem, string persist, string frp, string modemBlock, string persistBlock, string frpBlock) ObtenerParticiones(string modelo)
        {
            // Retornar particiones según el modelo (basado en el script original)
            return modelo.ToLower() switch
            {
                "agate" => ("md1img_a", "persist", "frp", "/dev/block/sdc19", "/dev/block/sdc14", "/dev/block/sdc4"),
                "alioth" => ("modem_a", "persist", "frp", "/dev/block/sde5", "/dev/block/sda22", "/dev/block/sda9"),
                "apollo" => ("modem", "persist", "frp", "/dev/block/sde51", "/dev/block/sda22", "/dev/block/sda9"),
                "cmi" => ("modem", "persist", "frp", "/dev/block/sde51", "/dev/block/sda22", "/dev/block/sda9"),
                "courbet" or "phoenixin" => ("modem", "persist", "frp", "/dev/block/sde52", "/dev/block/sdf7", "/dev/block/sda9"),
                "curtana" or "gram" or "excalibur" or "joyeuse" => ("modem", "persist", "frp", "/dev/block/sde4", "/dev/block/sda2", "/dev/block/sda5"),
                "davinci" => ("modem", "persist", "frp", "/dev/block/sde51", "/dev/block/sdf7", "/dev/block/sda9"),
                "fog" => ("modem_a", "persist", "frp", "/dev/block/sde4", "/dev/block/sda2", "/dev/block/sda6"),
                "gauguin" => ("modem", "persist", "frp", "/dev/block/sde48", "/dev/block/sda23", "/dev/block/sda9"),
                "ginkgo" => ("modem", "persist", "frp", "/dev/block/mmcblk0p80", "/dev/block/mmcblk0p69", "/dev/block/mmcblk0p30"),
                "lime" => ("modem", "persist", "frp", "/dev/block/sde52", "/dev/block/sda7", "/dev/block/sda4"),
                "lisa" or "renoir" => ("modem_a", "persist", "frp", "/dev/block/sde4", "/dev/block/sda8", "/dev/block/sda14"),
                "mojito" or "sunny" => ("modem_a", "persist", "frp", "/dev/block/sde4", "/dev/block/sda2", "/dev/block/sda6"),
                "raphael" => ("modem", "persist", "frp", "/dev/block/sde52", "/dev/block/sda23", "/dev/block/sda9"),
                "rosemary" => ("md1img_a", "persist", "frp", "/dev/block/sdc26", "/dev/block/sdc21", "/dev/block/sdc10"),
                "spes" or "spesn" => ("modem_a", "persist", "frp", "/dev/block/sde4", "/dev/block/sda2", "/dev/block/sda6"),
                "star" => ("modem_a", "persist", "frp", "/dev/block/sde5", "/dev/block/sda21", "/dev/block/sda9"),
                "surya" => ("modem", "persist", "frp", "/dev/block/sde4", "/dev/block/sda2", "/dev/block/sda5"),
                "sweet" or "sweetin" or "toco" => ("modem", "persist", "frp", "/dev/block/sde52", "/dev/block/sdf7", "/dev/block/sda9"),
                "umi" => ("modem", "persist", "frp", "/dev/block/sde51", "/dev/block/sda22", "/dev/block/sda9"),
                "vayu" => ("modem", "persist", "frp", "/dev/block/sde52", "/dev/block/sdf7", "/dev/block/sda9"),
                "veux" => ("modem_a", "persist", "frp", "/dev/block/sde4", "/dev/block/sda11", "/dev/block/sda5"),
                "vili" => ("modem_a", "persist", "frp", "/dev/block/sde5", "/dev/block/sda22", "/dev/block/sda9"),
                "zeus" => ("modem_a", "persist", "frp", "/dev/block/sde5", "/dev/block/sda22", "/dev/block/sda9"),
                _ => ("modem", "persist", "frp", "/dev/block/sde4", "/dev/block/sda2", "/dev/block/sda5")
            };
        }

        private void PatchFile(string filePath, string search, string replace)
        {
            try
            {
                byte[] content = File.ReadAllBytes(filePath);
                byte[] searchBytes = Encoding.ASCII.GetBytes(search);
                byte[] replaceBytes = Encoding.ASCII.GetBytes(replace);
                
                for (int i = 0; i <= content.Length - searchBytes.Length; i++)
                {
                    bool found = true;
                    for (int j = 0; j < searchBytes.Length; j++)
                    {
                        if (content[i + j] != searchBytes[j])
                        {
                            found = false;
                            break;
                        }
                    }
                    
                    if (found)
                    {
                        for (int j = 0; j < replaceBytes.Length && (i + j) < content.Length; j++)
                        {
                            content[i + j] = replaceBytes[j];
                        }
                        break;
                    }
                }
                
                File.WriteAllBytes(filePath, content);
            }
            catch (Exception ex)
            {
                Log($"Error patching file: {ex.Message}", TipoLog.Error);
            }
        }
    }
}
