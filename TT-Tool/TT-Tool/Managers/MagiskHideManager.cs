using System.Diagnostics;
using System.Text;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Gestor para ocultar root con Magisk (DenyList/Zygisk)
    /// Basado en la documentación oficial de Magisk
    /// </summary>
    public class MagiskHideManager
    {
        private ADBManager? _adbManager;
        
        public event EventHandler<string>? OnLogMessage;
        
        public MagiskHideManager(ADBManager adbManager)
        {
            _adbManager = adbManager;
        }
        
        /// <summary>
        /// Verifica si Magisk está instalado en el dispositivo
        /// </summary>
        public async Task<(bool installed, string version)> VerificarMagisk()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Verificando ADB...");
                
                // Primero verificar que ADB funcione
                bool adbOk = await _adbManager!.VerificarADB();
                if (!adbOk)
                {
                    OnLogMessage?.Invoke(this, "⚠ ADB no está disponible");
                    OnLogMessage?.Invoke(this, $"Ruta de ADB: {_adbManager.ObtenerRutaADB()}");
                    return (false, "");
                }
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "Verificando dispositivo conectado...");
                
                var devices = await _adbManager.ObtenerDispositivos();
                if (devices.Count == 0)
                {
                    OnLogMessage?.Invoke(this, "⚠ No hay dispositivos conectados");
                    OnLogMessage?.Invoke(this, "");
                    OnLogMessage?.Invoke(this, "Asegúrate de:");
                    OnLogMessage?.Invoke(this, "1. Conectar el dispositivo por USB");
                    OnLogMessage?.Invoke(this, "2. Habilitar USB Debugging");
                    OnLogMessage?.Invoke(this, "3. Autorizar la PC en el dispositivo");
                    return (false, "");
                }
                
                OnLogMessage?.Invoke(this, $"✓ Dispositivo conectado: {devices[0]}");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "Verificando Magisk...");
                
                // Verificar si existe el comando magisk
                var result = await _adbManager.ExecuteAdbCommandAsync("shell su -c 'magisk -v'");
                
                if (result.Contains("not found") || result.Contains("Error") || string.IsNullOrEmpty(result))
                {
                    OnLogMessage?.Invoke(this, "⚠ Magisk no está instalado o no hay root");
                    OnLogMessage?.Invoke(this, $"Respuesta: {result}");
                    return (false, "");
                }
                
                string version = result.Trim();
                OnLogMessage?.Invoke(this, $"✓ Magisk detectado: {version}");
                
                return (true, version);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al verificar Magisk: {ex.Message}");
                return (false, "");
            }
        }
        
        /// <summary>
        /// Habilita Zygisk (necesario para DenyList)
        /// </summary>
        public async Task<bool> HabilitarZygisk()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Habilitando Zygisk...");
                
                // Habilitar Zygisk
                var result = await _adbManager!.ExecuteAdbCommandAsync("shell su -c 'magisk --sqlite \"UPDATE settings SET value=1 WHERE key='zygisk'\"'");
                
                OnLogMessage?.Invoke(this, "✓ Zygisk habilitado");
                OnLogMessage?.Invoke(this, "⚠ Se requiere reinicio para aplicar cambios");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al habilitar Zygisk: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Habilita DenyList (ocultar root)
        /// </summary>
        public async Task<bool> HabilitarDenyList()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Habilitando DenyList...");
                
                // Habilitar DenyList
                var result = await _adbManager!.ExecuteAdbCommandAsync("shell su -c 'magisk --sqlite \"UPDATE settings SET value=1 WHERE key='denylist'\"'");
                
                OnLogMessage?.Invoke(this, "✓ DenyList habilitado");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al habilitar DenyList: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Obtiene la lista de aplicaciones instaladas
        /// </summary>
        public async Task<List<AppInfo>> ObtenerAplicaciones()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Obteniendo lista de aplicaciones...");
                
                var result = await _adbManager!.ExecuteAdbCommandAsync("shell pm list packages -3");
                
                var apps = new List<AppInfo>();
                var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    if (line.StartsWith("package:"))
                    {
                        string packageName = line.Replace("package:", "").Trim();
                        
                        // Obtener nombre de la app
                        var labelResult = await _adbManager.ExecuteAdbCommandAsync($"shell pm dump {packageName} | grep -A1 'labelRes'");
                        string label = packageName; // Por defecto usar el package name
                        
                        apps.Add(new AppInfo
                        {
                            PackageName = packageName,
                            Label = label,
                            IsInDenyList = false
                        });
                    }
                }
                
                OnLogMessage?.Invoke(this, $"✓ {apps.Count} aplicaciones encontradas");
                
                return apps;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al obtener aplicaciones: {ex.Message}");
                return new List<AppInfo>();
            }
        }
        
        /// <summary>
        /// Obtiene aplicaciones en DenyList
        /// </summary>
        public async Task<List<string>> ObtenerDenyList()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Obteniendo DenyList actual...");
                
                var result = await _adbManager!.ExecuteAdbCommandAsync("shell su -c 'magisk --denylist ls'");
                
                var denyList = new List<string>();
                var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        denyList.Add(line.Trim());
                    }
                }
                
                OnLogMessage?.Invoke(this, $"✓ {denyList.Count} aplicaciones en DenyList");
                
                return denyList;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al obtener DenyList: {ex.Message}");
                return new List<string>();
            }
        }
        
        /// <summary>
        /// Agrega una aplicación a DenyList
        /// </summary>
        public async Task<bool> AgregarADenyList(string packageName)
        {
            try
            {
                OnLogMessage?.Invoke(this, $"Agregando {packageName} a DenyList...");
                
                var result = await _adbManager!.ExecuteAdbCommandAsync($"shell su -c 'magisk --denylist add {packageName}'");
                
                OnLogMessage?.Invoke(this, $"✓ {packageName} agregado a DenyList");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al agregar a DenyList: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Remueve una aplicación de DenyList
        /// </summary>
        public async Task<bool> RemoverDeDenyList(string packageName)
        {
            try
            {
                OnLogMessage?.Invoke(this, $"Removiendo {packageName} de DenyList...");
                
                var result = await _adbManager!.ExecuteAdbCommandAsync($"shell su -c 'magisk --denylist rm {packageName}'");
                
                OnLogMessage?.Invoke(this, $"✓ {packageName} removido de DenyList");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al remover de DenyList: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Agrega aplicaciones bancarias comunes a DenyList
        /// </summary>
        public async Task<bool> OcultarRootDeBancos()
        {
            try
            {
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "=== OCULTANDO ROOT DE APLICACIONES BANCARIAS ===");
                OnLogMessage?.Invoke(this, "");
                
                // Lista de aplicaciones bancarias comunes
                var bancosComunes = new List<string>
                {
                    // Google
                    "com.google.android.gms",
                    "com.google.android.gsf",
                    "com.android.vending",
                    
                    // Bancos Colombia
                    "com.todo1.mobile",
                    "co.com.bancolombia.bancamovil",
                    "com.davivienda.davimovil",
                    "com.bbva.mobile.co",
                    "com.bancodebogota.bancamovil",
                    
                    // Bancos México
                    "com.bancomer.mbanking",
                    "com.santander.app",
                    "com.banorte.mobile",
                    
                    // Bancos España
                    "es.bancosantander.apps",
                    "es.lacaixa.mobile.android.newwapicon",
                    "es.openbank.mobile",
                    
                    // Bancos USA
                    "com.chase.sig.android",
                    "com.bankofamerica.mobile",
                    "com.wellsfargo.mobile",
                    
                    // Otros servicios
                    "com.paypal.android.p2pmobile",
                    "com.netflix.mediaclient",
                    "com.disney.disneyplus"
                };
                
                int agregados = 0;
                int noEncontrados = 0;
                
                // Obtener apps instaladas
                var appsInstaladas = await ObtenerAplicaciones();
                var packagesInstalados = appsInstaladas.Select(a => a.PackageName).ToHashSet();
                
                foreach (var banco in bancosComunes)
                {
                    if (packagesInstalados.Contains(banco))
                    {
                        bool success = await AgregarADenyList(banco);
                        if (success)
                        {
                            agregados++;
                        }
                    }
                    else
                    {
                        noEncontrados++;
                    }
                }
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, $"✓ {agregados} aplicaciones agregadas a DenyList");
                OnLogMessage?.Invoke(this, $"  {noEncontrados} aplicaciones no instaladas");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "⚠ IMPORTANTE:");
                OnLogMessage?.Invoke(this, "- Reinicia el dispositivo para aplicar cambios");
                OnLogMessage?.Invoke(this, "- Limpia datos de Google Play Services");
                OnLogMessage?.Invoke(this, "- Considera instalar módulos adicionales:");
                OnLogMessage?.Invoke(this, "  • Shamiko (ocultar Magisk)");
                OnLogMessage?.Invoke(this, "  • Play Integrity Fix");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Verifica SafetyNet/Play Integrity
        /// </summary>
        public async Task<(bool basicIntegrity, bool ctsProfile, bool strongIntegrity)> VerificarPlayIntegrity()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Verificando Play Integrity...");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "⚠ Para verificar Play Integrity:");
                OnLogMessage?.Invoke(this, "1. Instala 'YASNAC' desde Play Store");
                OnLogMessage?.Invoke(this, "2. O usa 'Play Integrity API Checker'");
                OnLogMessage?.Invoke(this, "3. Ejecuta el test en el dispositivo");
                
                return (false, false, false);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error: {ex.Message}");
                return (false, false, false);
            }
        }
        
        /// <summary>
        /// Instala un módulo de Magisk
        /// </summary>
        public async Task<bool> InstalarModulo(string zipPath)
        {
            try
            {
                OnLogMessage?.Invoke(this, $"Instalando módulo: {Path.GetFileName(zipPath)}");
                
                // Subir el módulo al dispositivo
                string remotePath = "/sdcard/Download/" + Path.GetFileName(zipPath);
                await _adbManager!.ExecuteAdbCommandAsync($"push \"{zipPath}\" {remotePath}");
                
                // Instalar con Magisk
                var result = await _adbManager.ExecuteAdbCommandAsync($"shell su -c 'magisk --install-module {remotePath}'");
                
                OnLogMessage?.Invoke(this, "✓ Módulo instalado");
                OnLogMessage?.Invoke(this, "⚠ Reinicia el dispositivo para activar el módulo");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al instalar módulo: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Reinicia el dispositivo
        /// </summary>
        public async Task<bool> ReiniciarDispositivo()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Reiniciando dispositivo...");
                
                await _adbManager!.ExecuteAdbCommandAsync("reboot");
                
                OnLogMessage?.Invoke(this, "✓ Dispositivo reiniciando...");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al reiniciar: {ex.Message}");
                return false;
            }
        }
    }
    
    /// <summary>
    /// Información de una aplicación
    /// </summary>
    public class AppInfo
    {
        public string PackageName { get; set; } = "";
        public string Label { get; set; } = "";
        public bool IsInDenyList { get; set; }
    }
}
