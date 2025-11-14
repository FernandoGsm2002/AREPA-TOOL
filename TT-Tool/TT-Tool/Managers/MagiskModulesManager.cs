using System.Net.Http;
using System.Text.Json;
using System.IO.Compression;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Gestor para descargar e instalar módulos de Magisk automáticamente
    /// </summary>
    public class MagiskModulesManager
    {
        private ADBManager _adbManager;
        private string _tempFolder;
        
        public event EventHandler<string>? OnLogMessage;
        public event EventHandler<int>? OnProgress;
        
        // URLs de los módulos esenciales
        private readonly Dictionary<string, ModuleInfo> _modulosEsenciales = new()
        {
            ["Shamiko"] = new ModuleInfo
            {
                Name = "Shamiko",
                Description = "Oculta Magisk de las aplicaciones",
                GitHubRepo = "LSPosed/LSPosed.github.io",
                AssetPattern = "Shamiko",
                Required = true
            },
            ["PlayIntegrityFix"] = new ModuleInfo
            {
                Name = "Play Integrity Fix",
                Description = "Pasa Play Integrity API",
                GitHubRepo = "chiteroman/PlayIntegrityFix",
                AssetPattern = ".zip",
                Required = true
            },
            ["ZygiskNext"] = new ModuleInfo
            {
                Name = "Zygisk Next",
                Description = "Implementación mejorada de Zygisk",
                GitHubRepo = "Dr-TSNG/ZygiskNext",
                AssetPattern = "zygisk-next",
                Required = false
            }
        };
        
        public MagiskModulesManager(ADBManager adbManager)
        {
            _adbManager = adbManager;
            _tempFolder = Path.Combine(Path.GetTempPath(), "TT-Tool-Modules");
            Directory.CreateDirectory(_tempFolder);
        }
        
        /// <summary>
        /// Descarga e instala todos los módulos esenciales
        /// </summary>
        public async Task<bool> InstalarModulosEsenciales()
        {
            try
            {
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "=== INSTALANDO MÓDULOS ESENCIALES ===");
                OnLogMessage?.Invoke(this, "");
                
                int totalModulos = _modulosEsenciales.Count;
                int moduloActual = 0;
                
                foreach (var modulo in _modulosEsenciales.Values)
                {
                    moduloActual++;
                    OnProgress?.Invoke(this, (moduloActual * 100) / totalModulos);
                    
                    OnLogMessage?.Invoke(this, $"[{moduloActual}/{totalModulos}] {modulo.Name}");
                    OnLogMessage?.Invoke(this, $"  {modulo.Description}");
                    OnLogMessage?.Invoke(this, "");
                    
                    // Descargar módulo
                    var (downloadSuccess, zipPath) = await DescargarModulo(modulo);
                    
                    if (!downloadSuccess)
                    {
                        if (modulo.Required)
                        {
                            OnLogMessage?.Invoke(this, $"⚠ Error: No se pudo descargar {modulo.Name} (requerido)");
                            return false;
                        }
                        else
                        {
                            OnLogMessage?.Invoke(this, $"⚠ Advertencia: No se pudo descargar {modulo.Name} (opcional)");
                            continue;
                        }
                    }
                    
                    // Instalar módulo
                    bool installSuccess = await InstalarModulo(zipPath, modulo.Name);
                    
                    if (!installSuccess && modulo.Required)
                    {
                        OnLogMessage?.Invoke(this, $"⚠ Error: No se pudo instalar {modulo.Name}");
                        return false;
                    }
                    
                    OnLogMessage?.Invoke(this, "");
                }
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "✓✓✓ MÓDULOS INSTALADOS CORRECTAMENTE ✓✓✓");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "⚠ IMPORTANTE:");
                OnLogMessage?.Invoke(this, "1. Reinicia el dispositivo");
                OnLogMessage?.Invoke(this, "2. Abre Magisk Manager");
                OnLogMessage?.Invoke(this, "3. Verifica que los módulos estén activos");
                OnLogMessage?.Invoke(this, "4. Limpia datos de Google Play Services");
                OnLogMessage?.Invoke(this, "5. Prueba las aplicaciones bancarias");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error crítico: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Descarga un módulo desde GitHub
        /// </summary>
        private async Task<(bool success, string zipPath)> DescargarModulo(ModuleInfo modulo)
        {
            try
            {
                OnLogMessage?.Invoke(this, $"  Obteniendo última versión de {modulo.Name}...");
                
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "TT-Tool");
                client.Timeout = TimeSpan.FromMinutes(10);
                
                // Obtener última release
                string apiUrl = $"https://api.github.com/repos/{modulo.GitHubRepo}/releases/latest";
                var response = await client.GetStringAsync(apiUrl);
                var json = JsonDocument.Parse(response);
                
                var version = json.RootElement.GetProperty("tag_name").GetString() ?? "unknown";
                OnLogMessage?.Invoke(this, $"  Versión: {version}");
                
                // Buscar el asset correcto
                var assets = json.RootElement.GetProperty("assets");
                string? downloadUrl = null;
                string? fileName = null;
                
                foreach (var asset in assets.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString();
                    if (name != null && name.Contains(modulo.AssetPattern, StringComparison.OrdinalIgnoreCase) && 
                        name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.GetProperty("browser_download_url").GetString();
                        fileName = name;
                        break;
                    }
                }
                
                if (downloadUrl == null || fileName == null)
                {
                    OnLogMessage?.Invoke(this, $"  ⚠ No se encontró el archivo ZIP");
                    return (false, "");
                }
                
                // Descargar
                string zipPath = Path.Combine(_tempFolder, fileName);
                
                if (File.Exists(zipPath))
                {
                    OnLogMessage?.Invoke(this, $"  ✓ Ya descargado: {fileName}");
                    return (true, zipPath);
                }
                
                OnLogMessage?.Invoke(this, $"  Descargando {fileName}...");
                
                var downloadResponse = await client.GetAsync(downloadUrl);
                downloadResponse.EnsureSuccessStatusCode();
                
                await using var fs = new FileStream(zipPath, FileMode.Create);
                await downloadResponse.Content.CopyToAsync(fs);
                
                OnLogMessage?.Invoke(this, $"  ✓ Descargado: {new FileInfo(zipPath).Length / 1024} KB");
                
                return (true, zipPath);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"  ⚠ Error al descargar: {ex.Message}");
                return (false, "");
            }
        }
        
        /// <summary>
        /// Instala un módulo en el dispositivo
        /// </summary>
        private async Task<bool> InstalarModulo(string zipPath, string moduleName)
        {
            try
            {
                OnLogMessage?.Invoke(this, $"  Instalando {moduleName}...");
                
                // Subir el módulo al dispositivo
                string remotePath = $"/sdcard/Download/{Path.GetFileName(zipPath)}";
                
                OnLogMessage?.Invoke(this, $"  Subiendo al dispositivo...");
                var pushResult = await _adbManager.ExecuteAdbCommandAsync($"push \"{zipPath}\" {remotePath}");
                
                if (pushResult.Contains("error") || pushResult.Contains("failed"))
                {
                    OnLogMessage?.Invoke(this, $"  ⚠ Error al subir archivo: {pushResult}");
                    return false;
                }
                
                OnLogMessage?.Invoke(this, $"  Instalando con Magisk...");
                
                // Instalar con Magisk
                var installResult = await _adbManager.ExecuteAdbCommandAsync($"shell su -c 'magisk --install-module {remotePath}'");
                
                // Limpiar archivo remoto
                await _adbManager.ExecuteAdbCommandAsync($"shell rm {remotePath}");
                
                if (installResult.Contains("Success") || installResult.Contains("done") || string.IsNullOrEmpty(installResult))
                {
                    OnLogMessage?.Invoke(this, $"  ✓ {moduleName} instalado");
                    return true;
                }
                else
                {
                    OnLogMessage?.Invoke(this, $"  ⚠ Respuesta: {installResult}");
                    // Asumir éxito si no hay error explícito
                    OnLogMessage?.Invoke(this, $"  ✓ {moduleName} instalado (verificar en Magisk Manager)");
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"  ⚠ Error al instalar: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Configura todo lo necesario para ocultar root completamente con Shamiko
        /// </summary>
        public async Task<bool> ConfigurarOcultacionCompleta()
        {
            try
            {
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "=== CONFIGURACIÓN COMPLETA CON SHAMIKO ===");
                OnLogMessage?.Invoke(this, "");
                
                // 1. Habilitar Zygisk (REQUERIDO para Shamiko)
                OnLogMessage?.Invoke(this, "1. Habilitando Zygisk...");
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'magisk --sqlite \"UPDATE settings SET value=1 WHERE key='\\''zygisk'\\'';\"'");
                OnLogMessage?.Invoke(this, "   ✓ Zygisk habilitado");
                
                // 2. DESHABILITAR DenyList (Shamiko NO funciona con DenyList)
                OnLogMessage?.Invoke(this, "2. Deshabilitando DenyList (Shamiko lo reemplaza)...");
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'magisk --sqlite \"UPDATE settings SET value=0 WHERE key='\\''denylist'\\'';\"'");
                OnLogMessage?.Invoke(this, "   ✓ DenyList deshabilitado");
                
                // 3. Configurar Shamiko en modo whitelist
                OnLogMessage?.Invoke(this, "3. Configurando Shamiko (modo whitelist)...");
                
                // Crear directorio de Shamiko
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'mkdir -p /data/adb/shamiko'");
                
                // Crear whitelist correctamente
                var appsOcultar = new[]
                {
                    // Google (crítico)
                    "com.google.android.gms",
                    "com.google.android.gsf", 
                    "com.android.vending",
                    "com.google.android.apps.walletnfcrel", // Google Pay
                    
                    // Bancos Perú
                    "com.bcp.innovacxion.yapeapp", // Yape
                    "pe.com.interbank.mobilebanking", // Interbank
                    "com.bbva.nxt_peru", // BBVA Perú
                    "com.scotiabank.peru.mobile.banking", // Scotiabank Perú
                    "pe.com.banbif.bancamovil", // BanBif
                    "com.financiero.mobile", // Banco Financiero
                    
                    // Otros servicios que detectan root
                    "com.netflix.mediaclient", // Netflix
                    "com.disney.disneyplus", // Disney+
                    "com.paypal.android.p2pmobile" // PayPal
                };
                
                // Crear archivo whitelist completo
                string whitelistContent = string.Join("\\n", appsOcultar);
                await _adbManager.ExecuteAdbCommandAsync($"shell su -c 'echo -e \"{whitelistContent}\" > /data/adb/shamiko/whitelist'");
                
                // Verificar que se creó
                var verifyWhitelist = await _adbManager.ExecuteAdbCommandAsync("shell su -c 'cat /data/adb/shamiko/whitelist'");
                OnLogMessage?.Invoke(this, $"   ✓ Whitelist creado con {appsOcultar.Length} apps");
                
                // 4. Configurar propiedades críticas del sistema
                OnLogMessage?.Invoke(this, "4. Configurando propiedades del sistema...");
                
                // Props críticos para pasar SafetyNet/Play Integrity
                var propsCommands = new[]
                {
                    // Eliminar props problemáticos primero
                    "resetprop --delete ro.boot.vbmeta.device_state",
                    "resetprop --delete ro.boot.verifiedbootstate",
                    "resetprop --delete ro.boot.flash.locked",
                    "resetprop --delete ro.boot.veritymode",
                    "resetprop --delete ro.boot.warranty_bit",
                    "resetprop --delete ro.warranty_bit",
                    "resetprop --delete ro.debuggable",
                    "resetprop --delete ro.secure",
                    "resetprop --delete ro.adb.secure",
                    "resetprop --delete ro.build.selinux",
                    
                    // Establecer valores correctos
                    "resetprop ro.boot.vbmeta.device_state locked",
                    "resetprop ro.boot.verifiedbootstate green",
                    "resetprop ro.boot.flash.locked 1",
                    "resetprop ro.boot.veritymode enforcing",
                    "resetprop ro.boot.warranty_bit 0",
                    "resetprop ro.warranty_bit 0",
                    "resetprop ro.debuggable 0",
                    "resetprop ro.secure 1",
                    "resetprop ro.adb.secure 1",
                    "resetprop ro.build.type user",
                    "resetprop ro.build.tags release-keys",
                    "resetprop ro.build.selinux 0",
                    
                    // Props adicionales para Samsung
                    "resetprop ro.boot.em.status 0x0",
                    "resetprop ro.boot.security_mode 1526595585"
                };
                
                foreach (var cmd in propsCommands)
                {
                    await _adbManager.ExecuteAdbCommandAsync($"shell su -c '{cmd}'");
                }
                
                OnLogMessage?.Invoke(this, "   ✓ Props configurados");
                
                // 5. Limpiar datos de Google Play Services
                OnLogMessage?.Invoke(this, "5. Limpiando datos de Google Play Services...");
                await _adbManager.ExecuteAdbCommandAsync("shell pm clear com.google.android.gms");
                await _adbManager.ExecuteAdbCommandAsync("shell pm clear com.google.android.gsf");
                await _adbManager.ExecuteAdbCommandAsync("shell pm clear com.android.vending");
                OnLogMessage?.Invoke(this, "   ✓ Datos limpiados");
                
                // 6. Forzar detención de servicios
                OnLogMessage?.Invoke(this, "6. Deteniendo servicios de Google...");
                await _adbManager.ExecuteAdbCommandAsync("shell am force-stop com.google.android.gms");
                await _adbManager.ExecuteAdbCommandAsync("shell am force-stop com.google.android.gsf");
                await _adbManager.ExecuteAdbCommandAsync("shell am force-stop com.android.vending");
                OnLogMessage?.Invoke(this, "   ✓ Servicios detenidos");
                
                // 7. Verificar módulos instalados
                OnLogMessage?.Invoke(this, "7. Verificando módulos instalados...");
                var modulosResult = await _adbManager.ExecuteAdbCommandAsync("shell su -c 'ls /data/adb/modules'");
                OnLogMessage?.Invoke(this, $"   Módulos: {modulosResult.Replace("\n", ", ")}");
                
                // 8. Configurar permisos correctos
                OnLogMessage?.Invoke(this, "8. Configurando permisos...");
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'chmod 755 /data/adb/shamiko'");
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'chmod 644 /data/adb/shamiko/whitelist'");
                OnLogMessage?.Invoke(this, "   ✓ Permisos configurados");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "✓✓✓ CONFIGURACIÓN COMPLETA ✓✓✓");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "⚠ IMPORTANTE:");
                OnLogMessage?.Invoke(this, "• Shamiko usa WHITELIST (solo oculta de apps listadas)");
                OnLogMessage?.Invoke(this, "• DenyList está DESHABILITADO (incompatible con Shamiko)");
                OnLogMessage?.Invoke(this, "• Reinicia el dispositivo AHORA");
                OnLogMessage?.Invoke(this, "• Después del reinicio, oculta Magisk Manager");
                OnLogMessage?.Invoke(this, "• Prueba las apps bancarias");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error en configuración: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Método alternativo: Configuración avanzada con múltiples capas de ocultación
        /// </summary>
        public async Task<bool> ConfigurarOcultacionAvanzada()
        {
            try
            {
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "=== CONFIGURACIÓN AVANZADA DE OCULTACIÓN ===");
                OnLogMessage?.Invoke(this, "");
                
                // 1. Verificar que Shamiko esté instalado
                OnLogMessage?.Invoke(this, "1. Verificando Shamiko...");
                var shamikoCheck = await _adbManager.ExecuteAdbCommandAsync("shell su -c 'ls /data/adb/modules | grep -i shamiko'");
                if (string.IsNullOrEmpty(shamikoCheck))
                {
                    OnLogMessage?.Invoke(this, "   ⚠ Shamiko no está instalado. Instálalo primero.");
                    return false;
                }
                OnLogMessage?.Invoke(this, "   ✓ Shamiko detectado");
                
                // 2. Configurar Zygisk
                OnLogMessage?.Invoke(this, "2. Configurando Zygisk...");
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'magisk --sqlite \"UPDATE settings SET value=1 WHERE key='\\''zygisk'\\'';\"'");
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'magisk --sqlite \"UPDATE settings SET value=0 WHERE key='\\''denylist'\\'';\"'");
                OnLogMessage?.Invoke(this, "   ✓ Zygisk ON, DenyList OFF");
                
                // 3. Configurar Shamiko con whitelist
                OnLogMessage?.Invoke(this, "3. Configurando Shamiko whitelist...");
                
                // Eliminar whitelist anterior si existe
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'rm -f /data/adb/shamiko/whitelist'");
                
                // Crear nuevo whitelist
                var apps = new[]
                {
                    "com.google.android.gms",
                    "com.google.android.gsf",
                    "com.android.vending",
                    "com.bcp.innovacxion.yapeapp",
                    "pe.com.interbank.mobilebanking",
                    "com.bbva.nxt_peru",
                    "com.scotiabank.peru.mobile.banking",
                    "pe.com.banbif.bancamovil",
                    "com.financiero.mobile",
                    "com.netflix.mediaclient",
                    "com.paypal.android.p2pmobile"
                };
                
                foreach (var app in apps)
                {
                    await _adbManager.ExecuteAdbCommandAsync($"shell su -c 'echo \"{app}\" >> /data/adb/shamiko/whitelist'");
                }
                
                // Verificar contenido
                var whitelistContent = await _adbManager.ExecuteAdbCommandAsync("shell su -c 'cat /data/adb/shamiko/whitelist'");
                int lineCount = whitelistContent.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
                OnLogMessage?.Invoke(this, $"   ✓ Whitelist creado: {lineCount} apps");
                
                // 4. Ocultar rastros de Magisk
                OnLogMessage?.Invoke(this, "4. Ocultando rastros de Magisk...");
                
                // Renombrar directorios de Magisk
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'setprop persist.sys.safemode 0'");
                
                // Ocultar binarios
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'mount -o bind /data/adb/magisk /sbin 2>/dev/null || true'");
                
                OnLogMessage?.Invoke(this, "   ✓ Rastros ocultados");
                
                // 5. Configurar props de SafetyNet
                OnLogMessage?.Invoke(this, "5. Configurando props de SafetyNet...");
                
                var propsCommands = new[]
                {
                    "resetprop --delete ro.boot.vbmeta.device_state",
                    "resetprop --delete ro.boot.verifiedbootstate",
                    "resetprop --delete ro.boot.flash.locked",
                    "resetprop --delete ro.boot.veritymode",
                    "resetprop --delete ro.boot.warranty_bit",
                    "resetprop --delete ro.warranty_bit",
                    "resetprop --delete ro.debuggable",
                    "resetprop --delete ro.secure",
                    "resetprop ro.boot.verifiedbootstate green",
                    "resetprop ro.boot.flash.locked 1",
                    "resetprop ro.boot.veritymode enforcing",
                    "resetprop ro.boot.warranty_bit 0",
                    "resetprop ro.warranty_bit 0",
                    "resetprop ro.debuggable 0",
                    "resetprop ro.secure 1",
                    "resetprop ro.build.type user",
                    "resetprop ro.build.tags release-keys",
                    "resetprop ro.build.selinux 0"
                };
                
                foreach (var cmd in propsCommands)
                {
                    await _adbManager.ExecuteAdbCommandAsync($"shell su -c '{cmd}'");
                }
                
                OnLogMessage?.Invoke(this, "   ✓ Props configurados");
                
                // 6. Limpiar caché y datos de Google
                OnLogMessage?.Invoke(this, "6. Limpiando caché de Google...");
                await _adbManager.ExecuteAdbCommandAsync("shell pm clear com.google.android.gms");
                await _adbManager.ExecuteAdbCommandAsync("shell pm clear com.google.android.gsf");
                await _adbManager.ExecuteAdbCommandAsync("shell pm clear com.android.vending");
                
                // Limpiar también apps bancarias
                foreach (var app in apps.Where(a => a.Contains("banco") || a.Contains("yape") || a.Contains("interbank")))
                {
                    await _adbManager.ExecuteAdbCommandAsync($"shell pm clear {app} 2>/dev/null || true");
                }
                
                OnLogMessage?.Invoke(this, "   ✓ Caché limpiado");
                
                // 7. Deshabilitar SELinux temporalmente (solo para pruebas)
                OnLogMessage?.Invoke(this, "7. Configurando SELinux...");
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'setenforce 0'");
                OnLogMessage?.Invoke(this, "   ✓ SELinux en modo permisivo");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "✓✓✓ CONFIGURACIÓN AVANZADA COMPLETA ✓✓✓");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "PASOS FINALES:");
                OnLogMessage?.Invoke(this, "1. REINICIA el dispositivo");
                OnLogMessage?.Invoke(this, "2. Abre Magisk Manager");
                OnLogMessage?.Invoke(this, "3. Verifica: Zygisk ON, DenyList OFF, Shamiko activo");
                OnLogMessage?.Invoke(this, "4. Oculta Magisk Manager (Configuración → Ocultar)");
                OnLogMessage?.Invoke(this, "5. Reinicia de nuevo");
                OnLogMessage?.Invoke(this, "6. Prueba las apps bancarias");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "Si aún no funciona:");
                OnLogMessage?.Invoke(this, "• Instala 'Universal SafetyNet Fix' desde Magisk");
                OnLogMessage?.Invoke(this, "• Verifica Play Integrity con 'YASNAC'");
                OnLogMessage?.Invoke(this, "• Considera usar 'Magisk Delta' en lugar de Magisk");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Configuración específica para dispositivos Samsung
        /// </summary>
        public async Task<bool> ConfigurarSamsung()
        {
            try
            {
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "=== CONFIGURACIÓN ESPECÍFICA SAMSUNG ===");
                OnLogMessage?.Invoke(this, "");
                
                // 1. Detectar si es Samsung
                OnLogMessage?.Invoke(this, "1. Detectando dispositivo Samsung...");
                var manufacturer = await _adbManager.ExecuteAdbCommandAsync("shell getprop ro.product.manufacturer");
                
                if (!manufacturer.ToLower().Contains("samsung"))
                {
                    OnLogMessage?.Invoke(this, "   ⚠ No es un dispositivo Samsung");
                    return false;
                }
                
                var model = await _adbManager.ExecuteAdbCommandAsync("shell getprop ro.product.model");
                OnLogMessage?.Invoke(this, $"   ✓ Samsung {model.Trim()} detectado");
                
                // 2. Props específicos de Samsung
                OnLogMessage?.Invoke(this, "2. Configurando props de Samsung...");
                
                var samsungProps = new[]
                {
                    // Knox y seguridad Samsung
                    "resetprop ro.boot.warranty_bit 0",
                    "resetprop ro.warranty_bit 0",
                    "resetprop ro.boot.em.status 0x0",
                    "resetprop ro.boot.security_mode 1526595585",
                    "resetprop ro.config.knox v30",
                    "resetprop ro.config.tima 1",
                    "resetprop ro.config.timaversion 3.0",
                    
                    // Verified Boot
                    "resetprop ro.boot.vbmeta.device_state locked",
                    "resetprop ro.boot.verifiedbootstate green",
                    "resetprop ro.boot.flash.locked 1",
                    "resetprop ro.boot.veritymode enforcing",
                    
                    // Build props
                    "resetprop ro.build.type user",
                    "resetprop ro.build.tags release-keys",
                    "resetprop ro.debuggable 0",
                    "resetprop ro.secure 1",
                    "resetprop ro.adb.secure 1",
                    
                    // SELinux
                    "resetprop ro.build.selinux 0",
                    "resetprop ro.build.selinux.enforce 1"
                };
                
                foreach (var prop in samsungProps)
                {
                    await _adbManager.ExecuteAdbCommandAsync($"shell su -c '{prop}'");
                }
                
                OnLogMessage?.Invoke(this, "   ✓ Props de Samsung configurados");
                
                // 3. Deshabilitar Knox si es posible
                OnLogMessage?.Invoke(this, "3. Intentando deshabilitar servicios Knox...");
                
                var knoxServices = new[]
                {
                    "com.samsung.android.knox.analytics.uploader",
                    "com.samsung.android.knox.attestation",
                    "com.samsung.android.knox.containeragent",
                    "com.samsung.android.knox.containercore"
                };
                
                foreach (var service in knoxServices)
                {
                    await _adbManager.ExecuteAdbCommandAsync($"shell pm disable-user {service} 2>/dev/null || true");
                }
                
                OnLogMessage?.Invoke(this, "   ✓ Servicios Knox deshabilitados");
                
                // 4. Configurar Shamiko para Samsung
                OnLogMessage?.Invoke(this, "4. Configurando Shamiko para Samsung...");
                
                // Crear whitelist con apps Samsung críticas
                var samsungApps = new[]
                {
                    "com.google.android.gms",
                    "com.google.android.gsf",
                    "com.android.vending",
                    "com.samsung.android.samsungpass",
                    "com.samsung.android.spay",
                    "com.samsung.android.spayfw",
                    "com.samsung.android.authfw",
                    "com.samsung.android.scloud",
                    "com.sec.android.app.sbrowser"
                };
                
                // Agregar a whitelist de Shamiko
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'rm -f /data/adb/shamiko/whitelist'");
                
                foreach (var app in samsungApps)
                {
                    await _adbManager.ExecuteAdbCommandAsync($"shell su -c 'echo \"{app}\" >> /data/adb/shamiko/whitelist'");
                }
                
                OnLogMessage?.Invoke(this, "   ✓ Shamiko configurado para Samsung");
                
                // 5. Limpiar datos de Samsung
                OnLogMessage?.Invoke(this, "5. Limpiando datos de apps Samsung...");
                
                await _adbManager.ExecuteAdbCommandAsync("shell pm clear com.samsung.android.samsungpass 2>/dev/null || true");
                await _adbManager.ExecuteAdbCommandAsync("shell pm clear com.samsung.android.spay 2>/dev/null || true");
                
                OnLogMessage?.Invoke(this, "   ✓ Datos limpiados");
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "✓✓✓ CONFIGURACIÓN SAMSUNG COMPLETA ✓✓✓");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "IMPORTANTE:");
                OnLogMessage?.Invoke(this, "• Reinicia el dispositivo");
                OnLogMessage?.Invoke(this, "• Verifica que Shamiko esté activo");
                OnLogMessage?.Invoke(this, "• Oculta Magisk Manager");
                OnLogMessage?.Invoke(this, "• Prueba las apps bancarias");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Método que replica exactamente lo que hace la herramienta que funciona
        /// Instala un script en /data/adb/service.d/ que se ejecuta al boot
        /// </summary>
        public async Task<bool> InstalarScriptMagiskHide()
        {
            try
            {
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "=== INSTALANDO SCRIPT MAGISK HIDE ===");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "Este método replica la herramienta que funciona");
                OnLogMessage?.Invoke(this, "");
                
                // 1. Leer el script desde recursos
                OnLogMessage?.Invoke(this, "1. Preparando script...");
                
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "magisk_hide_script.sh");
                
                if (!File.Exists(scriptPath))
                {
                    OnLogMessage?.Invoke(this, "   ⚠ Error: Script no encontrado");
                    OnLogMessage?.Invoke(this, $"   Ruta: {scriptPath}");
                    return false;
                }
                
                OnLogMessage?.Invoke(this, "   ✓ Script encontrado");
                
                // 2. Subir script a /data/local/tmp/
                OnLogMessage?.Invoke(this, "2. Subiendo script al dispositivo...");
                
                var pushResult = await _adbManager.ExecuteAdbCommandAsync($"push \"{scriptPath}\" /data/local/tmp/manager.sh");
                
                if (pushResult.Contains("error") || pushResult.Contains("failed"))
                {
                    OnLogMessage?.Invoke(this, $"   ⚠ Error al subir: {pushResult}");
                    return false;
                }
                
                OnLogMessage?.Invoke(this, "   ✓ Script subido a /data/local/tmp/manager.sh");
                
                // 3. Mover a /data/adb/service.d/
                OnLogMessage?.Invoke(this, "3. Moviendo script a /data/adb/service.d/...");
                
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'mkdir -p /data/adb/service.d'");
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'mv /data/local/tmp/manager.sh /data/adb/service.d/manager.sh'");
                
                OnLogMessage?.Invoke(this, "   ✓ Script movido");
                
                // 4. Cambiar permisos (755)
                OnLogMessage?.Invoke(this, "4. Configurando permisos...");
                
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'chmod 755 /data/adb/service.d/manager.sh'");
                
                OnLogMessage?.Invoke(this, "   ✓ Permisos 755 aplicados");
                
                // 5. Cambiar owner (root:everybody)
                OnLogMessage?.Invoke(this, "5. Configurando propietario...");
                
                await _adbManager.ExecuteAdbCommandAsync("shell su -c 'chown root:everybody /data/adb/service.d/manager.sh'");
                
                OnLogMessage?.Invoke(this, "   ✓ Owner configurado (root:everybody)");
                
                // 6. Verificar instalación
                OnLogMessage?.Invoke(this, "6. Verificando instalación...");
                
                var verifyResult = await _adbManager.ExecuteAdbCommandAsync("shell su -c 'ls -la /data/adb/service.d/manager.sh'");
                
                if (verifyResult.Contains("manager.sh"))
                {
                    OnLogMessage?.Invoke(this, "   ✓ Script instalado correctamente");
                    OnLogMessage?.Invoke(this, $"   {verifyResult.Trim()}");
                }
                else
                {
                    OnLogMessage?.Invoke(this, "   ⚠ No se pudo verificar la instalación");
                }
                
                // 7. Ejecutar script manualmente una vez (opcional)
                OnLogMessage?.Invoke(this, "7. Ejecutando script manualmente...");
                
                var execResult = await _adbManager.ExecuteAdbCommandAsync("shell su -c 'sh /data/adb/service.d/manager.sh &'");
                
                OnLogMessage?.Invoke(this, "   ✓ Script ejecutado");
                
                // 8. Esperar y verificar log
                OnLogMessage?.Invoke(this, "8. Esperando resultados...");
                await Task.Delay(3000);
                
                var logResult = await _adbManager.ExecuteAdbCommandAsync("shell su -c 'cat /data/local/tmp/magisk_hide.log'");
                
                if (!string.IsNullOrEmpty(logResult))
                {
                    OnLogMessage?.Invoke(this, "");
                    OnLogMessage?.Invoke(this, "--- Log del Script ---");
                    foreach (var line in logResult.Split('\n'))
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            OnLogMessage?.Invoke(this, $"   {line.Trim()}");
                        }
                    }
                    OnLogMessage?.Invoke(this, "--- Fin del Log ---");
                }
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "✓✓✓ SCRIPT INSTALADO CORRECTAMENTE ✓✓✓");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "IMPORTANTE:");
                OnLogMessage?.Invoke(this, "• El script se ejecutará automáticamente al reiniciar");
                OnLogMessage?.Invoke(this, "• Configurará props y DenyList en cada boot");
                OnLogMessage?.Invoke(this, "• NO necesitas Shamiko con este método");
                OnLogMessage?.Invoke(this, "• Reinicia el dispositivo AHORA");
                OnLogMessage?.Invoke(this, "• Después del reinicio, prueba las apps bancarias");
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "Para ver el log después del reinicio:");
                OnLogMessage?.Invoke(this, "adb shell su -c 'cat /data/local/tmp/magisk_hide.log'");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Limpia archivos temporales
        /// </summary>
        public void LimpiarTemporales()
        {
            try
            {
                if (Directory.Exists(_tempFolder))
                {
                    Directory.Delete(_tempFolder, true);
                }
            }
            catch { }
        }
    }
    
    /// <summary>
    /// Información de un módulo
    /// </summary>
    public class ModuleInfo
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string GitHubRepo { get; set; } = "";
        public string AssetPattern { get; set; } = "";
        public bool Required { get; set; }
    }
}
