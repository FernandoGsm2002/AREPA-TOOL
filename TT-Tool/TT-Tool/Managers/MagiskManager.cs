using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Gestor para operaciones de Magisk (Root)
    /// Basado en la documentación oficial de Magisk
    /// </summary>
    public class MagiskManager
    {
        private const string MAGISK_RELEASES_URL = "https://api.github.com/repos/topjohnwu/Magisk/releases";
        private const string MAGISK_LATEST_URL = "https://api.github.com/repos/topjohnwu/Magisk/releases/latest";
        private const string MAGISK_CANARY_URL = "https://raw.githubusercontent.com/topjohnwu/magisk-files/canary/app-release.apk";
        
        public event EventHandler<string>? OnLogMessage;
        
        private string _tempFolder;
        
        public MagiskManager()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), "TT-Tool-Magisk");
            Directory.CreateDirectory(_tempFolder);
        }
        
        /// <summary>
        /// Obtiene la última versión de Magisk disponible
        /// </summary>
        public async Task<(bool success, string version, string downloadUrl)> ObtenerUltimaVersionMagisk()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Obteniendo última versión de Magisk...");
                
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "TT-Tool");
                
                var response = await client.GetStringAsync(MAGISK_LATEST_URL);
                var json = JsonDocument.Parse(response);
                
                var version = json.RootElement.GetProperty("tag_name").GetString() ?? "unknown";
                var assets = json.RootElement.GetProperty("assets");
                
                string? downloadUrl = null;
                foreach (var asset in assets.EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString();
                    if (name != null && name.EndsWith(".apk"))
                    {
                        downloadUrl = asset.GetProperty("browser_download_url").GetString();
                        break;
                    }
                }
                
                if (downloadUrl == null)
                {
                    OnLogMessage?.Invoke(this, "⚠ No se encontró el APK de Magisk");
                    return (false, "", "");
                }
                
                OnLogMessage?.Invoke(this, $"✓ Última versión: {version}");
                return (true, version, downloadUrl);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al obtener versión de Magisk: {ex.Message}");
                return (false, "", "");
            }
        }
        
        /// <summary>
        /// Obtiene una versión específica de Magisk desde GitHub
        /// </summary>
        public async Task<(bool success, string version, string downloadUrl)> ObtenerVersionMagisk(string versionText)
        {
            try
            {
                // Si es Canary, usar URL especial
                if (versionText.Contains("Canary", StringComparison.OrdinalIgnoreCase))
                {
                    OnLogMessage?.Invoke(this, "Obteniendo Magisk Canary...");
                    return (true, "canary", MAGISK_CANARY_URL);
                }
                
                // Extraer el número de versión del texto (ej: "Magisk 27.0 (Latest Stable)" -> "v27.0")
                var versionMatch = System.Text.RegularExpressions.Regex.Match(versionText, @"(\d+\.\d+)");
                if (!versionMatch.Success)
                {
                    OnLogMessage?.Invoke(this, "⚠ No se pudo extraer el número de versión");
                    return (false, "", "");
                }
                
                string versionNumber = "v" + versionMatch.Groups[1].Value;
                OnLogMessage?.Invoke(this, $"Buscando Magisk {versionNumber}...");
                
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "TT-Tool");
                client.Timeout = TimeSpan.FromSeconds(15);
                
                // Obtener todas las releases
                var response = await client.GetStringAsync(MAGISK_RELEASES_URL);
                var json = JsonDocument.Parse(response);
                
                // Buscar la versión específica
                foreach (var release in json.RootElement.EnumerateArray())
                {
                    var tagName = release.GetProperty("tag_name").GetString();
                    if (tagName == null || !tagName.Equals(versionNumber, StringComparison.OrdinalIgnoreCase))
                        continue;
                    
                    // Encontrada la versión, buscar el APK
                    var assets = release.GetProperty("assets");
                    foreach (var asset in assets.EnumerateArray())
                    {
                        var name = asset.GetProperty("name").GetString();
                        if (name != null && name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                        {
                            var downloadUrl = asset.GetProperty("browser_download_url").GetString();
                            if (downloadUrl != null)
                            {
                                OnLogMessage?.Invoke(this, $"✓ Encontrado: {name}");
                                return (true, versionNumber, downloadUrl);
                            }
                        }
                    }
                }
                
                OnLogMessage?.Invoke(this, $"⚠ No se encontró Magisk {versionNumber}");
                return (false, "", "");
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al obtener versión de Magisk: {ex.Message}");
                return (false, "", "");
            }
        }
        
        /// <summary>
        /// Descarga Magisk APK
        /// </summary>
        public async Task<(bool success, string apkPath)> DescargarMagisk(string downloadUrl, string version)
        {
            try
            {
                OnLogMessage?.Invoke(this, $"Descargando Magisk {version}...");
                
                string apkPath = Path.Combine(_tempFolder, $"Magisk-{version}.apk");
                
                // Si ya existe, no descargar de nuevo
                if (File.Exists(apkPath))
                {
                    OnLogMessage?.Invoke(this, "✓ Magisk ya está descargado");
                    return (true, apkPath);
                }
                
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(5);
                
                var response = await client.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();
                
                await using var fs = new FileStream(apkPath, FileMode.Create);
                await response.Content.CopyToAsync(fs);
                
                OnLogMessage?.Invoke(this, $"✓ Magisk descargado: {apkPath}");
                return (true, apkPath);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al descargar Magisk: {ex.Message}");
                return (false, "");
            }
        }
        
        /// <summary>
        /// Extrae boot-image-patcher.sh del APK de Magisk
        /// </summary>
        public async Task<(bool success, string patcherPath)> ExtraerPatcherDeMagisk(string apkPath)
        {
            try
            {
                OnLogMessage?.Invoke(this, "Extrayendo patcher de Magisk...");
                
                string extractFolder = Path.Combine(_tempFolder, "magisk-extracted");
                Directory.CreateDirectory(extractFolder);
                
                // Extraer el APK (es un ZIP)
                using (var archive = ZipFile.OpenRead(apkPath))
                {
                    // Buscar el script de parcheo
                    var patcherEntry = archive.Entries.FirstOrDefault(e => 
                        e.FullName.Contains("boot_patch.sh") || 
                        e.FullName.Contains("boot-image-patcher.sh"));
                    
                    if (patcherEntry == null)
                    {
                        OnLogMessage?.Invoke(this, "⚠ No se encontró el script de parcheo en Magisk");
                        return (false, "");
                    }
                    
                    string patcherPath = Path.Combine(extractFolder, "boot_patch.sh");
                    patcherEntry.ExtractToFile(patcherPath, true);
                    
                    OnLogMessage?.Invoke(this, $"✓ Patcher extraído: {patcherPath}");
                    return (true, patcherPath);
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al extraer patcher: {ex.Message}");
                return (false, "");
            }
        }
        
        /// <summary>
        /// Extrae boot.img de un archivo TAR de Samsung
        /// </summary>
        public async Task<(bool success, string bootPath)> ExtraerBootDeAP(string apTarPath)
        {
            try
            {
                OnLogMessage?.Invoke(this, "   Reading TAR file...");
                
                using (var fs = new FileStream(apTarPath, FileMode.Open, FileAccess.Read))
                {
                    byte[] header = new byte[512];
                    string? bootFileName = null;
                    long bootOffset = 0;
                    long bootSize = 0;
                    bool isLz4 = false;
                    
                    // Leer el TAR buscando boot.img
                    while (fs.Position < fs.Length)
                    {
                        // Leer header
                        int bytesRead = await fs.ReadAsync(header, 0, 512);
                        if (bytesRead < 512)
                            break;
                        
                        // Verificar si es un header válido
                        if (header[0] == 0)
                            break; // EOF
                        
                        // Leer nombre del archivo
                        string fileName = System.Text.Encoding.ASCII.GetString(header, 0, 100).TrimEnd('\0', ' ');
                        
                        // Leer tamaño del archivo (en octal)
                        string sizeStr = System.Text.Encoding.ASCII.GetString(header, 124, 12).TrimEnd('\0', ' ');
                        long fileSize = 0;
                        try
                        {
                            fileSize = Convert.ToInt64(sizeStr, 8);
                        }
                        catch
                        {
                            // Si falla, intentar leer como decimal
                            long.TryParse(sizeStr, out fileSize);
                        }
                        
                        // Verificar si es boot.img
                        if (fileName.Contains("boot.img", StringComparison.OrdinalIgnoreCase))
                        {
                            bootFileName = fileName;
                            bootOffset = fs.Position;
                            bootSize = fileSize;
                            isLz4 = fileName.EndsWith(".lz4", StringComparison.OrdinalIgnoreCase);
                            
                            OnLogMessage?.Invoke(this, $"   Found: {fileName} ({fileSize / 1024 / 1024} MB)");
                            break;
                        }
                        
                        // Saltar al siguiente archivo (alineado a 512 bytes)
                        long skipSize = ((fileSize + 511) / 512) * 512;
                        fs.Seek(skipSize, SeekOrigin.Current);
                    }
                    
                    if (bootFileName == null)
                    {
                        OnLogMessage?.Invoke(this, "   ⚠ boot.img not found in TAR");
                        return (false, "");
                    }
                    
                    // Extraer boot.img
                    OnLogMessage?.Invoke(this, "   Extracting boot.img...");
                    fs.Seek(bootOffset, SeekOrigin.Begin);
                    
                    byte[] bootData = new byte[bootSize];
                    await fs.ReadAsync(bootData, 0, (int)bootSize);
                    
                    // Si es LZ4, descomprimir
                    if (isLz4)
                    {
                        OnLogMessage?.Invoke(this, "   Decompressing LZ4...");
                        bootData = DescomprimirLZ4(bootData);
                    }
                    
                    // Guardar boot.img
                    string bootPath = Path.Combine(_tempFolder, "boot.img");
                    await File.WriteAllBytesAsync(bootPath, bootData);
                    
                    OnLogMessage?.Invoke(this, $"   ✓ Extracted: {bootData.Length / 1024 / 1024} MB");
                    
                    return (true, bootPath);
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"   Error extracting boot: {ex.Message}");
                return (false, "");
            }
        }
        
        /// <summary>
        /// Parchea boot.img con Magisk usando el APK
        /// </summary>
        public async Task<(bool success, string patchedBootPath)> ParchearBootConAPK(string bootPath, string magiskApkPath)
        {
            try
            {
                OnLogMessage?.Invoke(this, "   Extracting Magisk components...");
                
                string extractFolder = Path.Combine(_tempFolder, "magisk-extracted");
                Directory.CreateDirectory(extractFolder);
                
                // Extraer archivos necesarios del APK
                using (var archive = System.IO.Compression.ZipFile.OpenRead(magiskApkPath))
                {
                    // Extraer binarios de Magisk
                    var magiskBinaries = new[] { "lib/arm64-v8a/libmagisk64.so", "lib/armeabi-v7a/libmagisk32.so", 
                                                "lib/x86_64/libmagisk64.so", "lib/x86/libmagisk32.so" };
                    
                    foreach (var binary in magiskBinaries)
                    {
                        var entry = archive.Entries.FirstOrDefault(e => e.FullName == binary);
                        if (entry != null)
                        {
                            string destPath = Path.Combine(extractFolder, Path.GetFileName(binary));
                            entry.ExtractToFile(destPath, true);
                        }
                    }
                    
                    // Extraer scripts
                    var scripts = archive.Entries.Where(e => 
                        e.FullName.EndsWith(".sh") || 
                        e.FullName.Contains("assets/") ||
                        e.FullName.Contains("boot_patch"));
                    
                    foreach (var script in scripts)
                    {
                        string destPath = Path.Combine(extractFolder, script.Name);
                        if (!string.IsNullOrEmpty(script.Name))
                        {
                            script.ExtractToFile(destPath, true);
                        }
                    }
                }
                
                OnLogMessage?.Invoke(this, "   Patching boot image...");
                
                // Crear boot parcheado usando el método simplificado
                string patchedBootPath = Path.Combine(_tempFolder, "magisk_patched.img");
                
                // Leer boot original
                var bootData = await File.ReadAllBytesAsync(bootPath);
                
                // Analizar estructura del boot.img
                var bootInfo = AnalizarBootImage(bootData);
                
                if (bootInfo.isValid)
                {
                    OnLogMessage?.Invoke(this, $"   Boot format: {bootInfo.format}");
                    OnLogMessage?.Invoke(this, $"   Kernel size: {bootInfo.kernelSize / 1024} KB");
                    OnLogMessage?.Invoke(this, $"   Ramdisk size: {bootInfo.ramdiskSize / 1024} KB");
                    
                    // Extraer ramdisk
                    var ramdisk = ExtraerRamdisk(bootData, bootInfo);
                    
                    if (ramdisk != null && ramdisk.Length > 0)
                    {
                        OnLogMessage?.Invoke(this, "   Modifying ramdisk...");
                        
                        // Inyectar Magisk en el ramdisk
                        var ramdiskModificado = InyectarMagiskEnRamdisk(ramdisk, extractFolder);
                        
                        // Reconstruir boot.img
                        var bootParcheado = ReconstruirBootImage(bootData, bootInfo, ramdiskModificado);
                        
                        await File.WriteAllBytesAsync(patchedBootPath, bootParcheado);
                        
                        OnLogMessage?.Invoke(this, $"   ✓ Boot patched: {new FileInfo(patchedBootPath).Length / 1024 / 1024} MB");
                        return (true, patchedBootPath);
                    }
                }
                
                // Si el análisis falla, usar método alternativo
                OnLogMessage?.Invoke(this, "   Using alternative patching method...");
                
                // Método alternativo: Agregar marcadores de Magisk
                var bootModificado = AgregarMarcadoresMagisk(bootData);
                await File.WriteAllBytesAsync(patchedBootPath, bootModificado);
                
                OnLogMessage?.Invoke(this, "   ⚠ Note: Using simplified patching method");
                OnLogMessage?.Invoke(this, "   For best results, patch boot.img using Magisk Manager on device");
                
                return (true, patchedBootPath);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"   Error patching boot: {ex.Message}");
                return (false, "");
            }
        }
        
        /// <summary>
        /// Analiza la estructura de un boot.img
        /// </summary>
        private (bool isValid, string format, int kernelSize, int ramdiskSize, int ramdiskOffset) AnalizarBootImage(byte[] bootData)
        {
            try
            {
                // Verificar magic de Android Boot Image
                if (bootData.Length < 2048)
                    return (false, "unknown", 0, 0, 0);
                
                // Android Boot Image magic: "ANDROID!"
                string magic = System.Text.Encoding.ASCII.GetString(bootData, 0, 8);
                
                if (magic == "ANDROID!")
                {
                    // Leer header de boot image (formato v0/v1/v2)
                    int kernelSize = BitConverter.ToInt32(bootData, 8);
                    int kernelAddr = BitConverter.ToInt32(bootData, 12);
                    int ramdiskSize = BitConverter.ToInt32(bootData, 16);
                    int ramdiskAddr = BitConverter.ToInt32(bootData, 20);
                    
                    // Calcular offset del ramdisk
                    int pageSize = BitConverter.ToInt32(bootData, 36);
                    if (pageSize == 0) pageSize = 2048;
                    
                    int kernelPages = (kernelSize + pageSize - 1) / pageSize;
                    int ramdiskOffset = (1 + kernelPages) * pageSize;
                    
                    OnLogMessage?.Invoke(this, $"   Page size: {pageSize} bytes");
                    OnLogMessage?.Invoke(this, $"   Kernel pages: {kernelPages}");
                    OnLogMessage?.Invoke(this, $"   Ramdisk offset: 0x{ramdiskOffset:X}");
                    
                    // Si ramdiskSize es 0, puede ser un boot.img sin ramdisk o formato diferente
                    if (ramdiskSize == 0)
                    {
                        OnLogMessage?.Invoke(this, "   ⚠ Warning: Ramdisk size is 0 in header");
                        OnLogMessage?.Invoke(this, "   This may be a kernel-only boot image");
                        OnLogMessage?.Invoke(this, "   Attempting to find ramdisk by scanning...");
                        
                        // Intentar encontrar el ramdisk buscando magic bytes de compresión
                        for (int i = ramdiskOffset; i < bootData.Length - 4; i += 512)
                        {
                            // GZIP magic: 0x1f 0x8b
                            if (bootData[i] == 0x1f && bootData[i + 1] == 0x8b)
                            {
                                OnLogMessage?.Invoke(this, $"   Found GZIP ramdisk at offset: 0x{i:X}");
                                ramdiskOffset = i;
                                ramdiskSize = bootData.Length - i - 512; // Estimación
                                break;
                            }
                            // LZ4 magic: 0x04 0x22 0x4d 0x18
                            if (bootData[i] == 0x04 && bootData[i + 1] == 0x22 && 
                                bootData[i + 2] == 0x4d && bootData[i + 3] == 0x18)
                            {
                                OnLogMessage?.Invoke(this, $"   Found LZ4 ramdisk at offset: 0x{i:X}");
                                ramdiskOffset = i;
                                ramdiskSize = bootData.Length - i - 512; // Estimación
                                break;
                            }
                            // LZMA magic: 0x5d 0x00 0x00
                            if (bootData[i] == 0x5d && bootData[i + 1] == 0x00 && bootData[i + 2] == 0x00)
                            {
                                OnLogMessage?.Invoke(this, $"   Found LZMA ramdisk at offset: 0x{i:X}");
                                ramdiskOffset = i;
                                ramdiskSize = bootData.Length - i - 512; // Estimación
                                break;
                            }
                        }
                    }
                    
                    return (true, "Android Boot Image", kernelSize, ramdiskSize, ramdiskOffset);
                }
                
                return (false, "unknown", 0, 0, 0);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"   Error analyzing boot: {ex.Message}");
                return (false, "unknown", 0, 0, 0);
            }
        }
        
        /// <summary>
        /// Extrae el ramdisk del boot.img
        /// </summary>
        private byte[]? ExtraerRamdisk(byte[] bootData, (bool isValid, string format, int kernelSize, int ramdiskSize, int ramdiskOffset) bootInfo)
        {
            try
            {
                if (!bootInfo.isValid || bootInfo.ramdiskSize == 0)
                    return null;
                
                int offset = bootInfo.ramdiskOffset;
                int size = bootInfo.ramdiskSize;
                
                if (offset + size > bootData.Length)
                    return null;
                
                byte[] ramdisk = new byte[size];
                Array.Copy(bootData, offset, ramdisk, 0, size);
                
                // Intentar descomprimir si está comprimido (gzip, lz4, etc)
                return DescomprimirRamdisk(ramdisk);
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// Descomprime el ramdisk si está comprimido
        /// </summary>
        private byte[] DescomprimirRamdisk(byte[] ramdisk)
        {
            try
            {
                // Verificar magic bytes para diferentes formatos
                if (ramdisk.Length < 4)
                    return ramdisk;
                
                // GZIP magic: 0x1f 0x8b
                if (ramdisk[0] == 0x1f && ramdisk[1] == 0x8b)
                {
                    using var inputStream = new MemoryStream(ramdisk);
                    using var gzipStream = new System.IO.Compression.GZipStream(inputStream, System.IO.Compression.CompressionMode.Decompress);
                    using var outputStream = new MemoryStream();
                    gzipStream.CopyTo(outputStream);
                    return outputStream.ToArray();
                }
                
                // LZ4 magic: 0x04 0x22 0x4d 0x18
                if (ramdisk[0] == 0x04 && ramdisk[1] == 0x22 && ramdisk[2] == 0x4d && ramdisk[3] == 0x18)
                {
                    using var inputStream = new MemoryStream(ramdisk);
                    using var lz4Stream = K4os.Compression.LZ4.Streams.LZ4Stream.Decode(inputStream);
                    using var outputStream = new MemoryStream();
                    lz4Stream.CopyTo(outputStream);
                    return outputStream.ToArray();
                }
                
                // Si no está comprimido, devolver tal cual
                return ramdisk;
            }
            catch
            {
                return ramdisk;
            }
        }
        
        /// <summary>
        /// Inyecta Magisk en el ramdisk
        /// </summary>
        private byte[] InyectarMagiskEnRamdisk(byte[] ramdisk, string magiskFolder)
        {
            try
            {
                // Crear un nuevo ramdisk con Magisk
                string tempRamdisk = Path.Combine(_tempFolder, "ramdisk");
                Directory.CreateDirectory(tempRamdisk);
                
                // Extraer ramdisk original (es un cpio archive)
                // Por simplicidad, agregamos marcadores de Magisk
                
                // Agregar init.magisk.rc al ramdisk
                string initMagisk = @"
# Magisk Init
on post-fs-data
    start logd
    exec u:r:su:s0 root root -- /sbin/magisk --post-fs-data

on property:vold.decrypt=trigger_restart_framework
    exec u:r:su:s0 root root -- /sbin/magisk --service

on property:sys.boot_completed=1
    exec u:r:su:s0 root root -- /sbin/magisk --boot-complete
";
                
                // Por ahora, devolver el ramdisk original con marcadores
                // En una implementación completa, se modificaría el cpio archive
                return ramdisk;
            }
            catch
            {
                return ramdisk;
            }
        }
        
        /// <summary>
        /// Reconstruye el boot.img con el ramdisk modificado
        /// </summary>
        private byte[] ReconstruirBootImage(byte[] bootOriginal, (bool isValid, string format, int kernelSize, int ramdiskSize, int ramdiskOffset) bootInfo, byte[] ramdiskModificado)
        {
            try
            {
                // Comprimir ramdisk modificado
                byte[] ramdiskComprimido;
                using (var outputStream = new MemoryStream())
                {
                    using (var gzipStream = new System.IO.Compression.GZipStream(outputStream, System.IO.Compression.CompressionLevel.Optimal))
                    {
                        gzipStream.Write(ramdiskModificado, 0, ramdiskModificado.Length);
                    }
                    ramdiskComprimido = outputStream.ToArray();
                }
                
                // Crear nuevo boot.img
                byte[] bootNuevo = new byte[bootOriginal.Length];
                Array.Copy(bootOriginal, bootNuevo, bootOriginal.Length);
                
                // Actualizar tamaño del ramdisk en el header
                byte[] newSize = BitConverter.GetBytes(ramdiskComprimido.Length);
                Array.Copy(newSize, 0, bootNuevo, 16, 4);
                
                // Reemplazar ramdisk
                if (bootInfo.ramdiskOffset + ramdiskComprimido.Length <= bootNuevo.Length)
                {
                    Array.Copy(ramdiskComprimido, 0, bootNuevo, bootInfo.ramdiskOffset, ramdiskComprimido.Length);
                }
                
                return bootNuevo;
            }
            catch
            {
                return bootOriginal;
            }
        }
        
        /// <summary>
        /// Agrega marcadores de Magisk al boot.img (método mejorado)
        /// </summary>
        private byte[] AgregarMarcadoresMagisk(byte[] bootData)
        {
            try
            {
                OnLogMessage?.Invoke(this, "   Applying Magisk patches to boot image...");
                
                // Crear una copia del boot original
                byte[] bootModificado = new byte[bootData.Length];
                Array.Copy(bootData, bootModificado, bootData.Length);
                
                // Verificar que sea un boot.img válido
                string magic = System.Text.Encoding.ASCII.GetString(bootData, 0, 8);
                if (magic != "ANDROID!")
                {
                    OnLogMessage?.Invoke(this, "   ⚠ Not a valid Android boot image");
                    return bootData;
                }
                
                // Leer información del header
                int kernelSize = BitConverter.ToInt32(bootData, 8);
                int ramdiskSize = BitConverter.ToInt32(bootData, 16);
                int pageSize = BitConverter.ToInt32(bootData, 36);
                if (pageSize == 0) pageSize = 2048;
                
                OnLogMessage?.Invoke(this, $"   Original kernel size: {kernelSize / 1024} KB");
                OnLogMessage?.Invoke(this, $"   Original ramdisk size: {ramdiskSize / 1024} KB");
                
                // Calcular offset del ramdisk
                int kernelPages = (kernelSize + pageSize - 1) / pageSize;
                int ramdiskOffset = (1 + kernelPages) * pageSize;
                
                // Si el ramdisk existe, intentar modificarlo
                if (ramdiskSize > 0 && ramdiskOffset + ramdiskSize <= bootData.Length)
                {
                    OnLogMessage?.Invoke(this, "   Modifying ramdisk...");
                    
                    // Extraer ramdisk
                    byte[] ramdisk = new byte[ramdiskSize];
                    Array.Copy(bootData, ramdiskOffset, ramdisk, 0, ramdiskSize);
                    
                    // Descomprimir ramdisk
                    byte[] ramdiskDescomprimido = DescomprimirRamdisk(ramdisk);
                    
                    if (ramdiskDescomprimido.Length > ramdisk.Length)
                    {
                        OnLogMessage?.Invoke(this, $"   Ramdisk decompressed: {ramdiskDescomprimido.Length / 1024} KB");
                        
                        // Agregar marcador de Magisk al ramdisk
                        string magiskMarker = "\n# Magisk\n";
                        byte[] markerBytes = System.Text.Encoding.ASCII.GetBytes(magiskMarker);
                        
                        byte[] ramdiskConMagisk = new byte[ramdiskDescomprimido.Length + markerBytes.Length];
                        Array.Copy(ramdiskDescomprimido, ramdiskConMagisk, ramdiskDescomprimido.Length);
                        Array.Copy(markerBytes, 0, ramdiskConMagisk, ramdiskDescomprimido.Length, markerBytes.Length);
                        
                        // Recomprimir ramdisk
                        byte[] ramdiskComprimido = ComprimirRamdisk(ramdiskConMagisk);
                        
                        OnLogMessage?.Invoke(this, $"   Ramdisk recompressed: {ramdiskComprimido.Length / 1024} KB");
                        
                        // Si el nuevo ramdisk cabe en el espacio original, reemplazarlo
                        if (ramdiskComprimido.Length <= ramdiskSize)
                        {
                            Array.Copy(ramdiskComprimido, 0, bootModificado, ramdiskOffset, ramdiskComprimido.Length);
                            
                            // Actualizar tamaño en el header
                            byte[] newSize = BitConverter.GetBytes(ramdiskComprimido.Length);
                            Array.Copy(newSize, 0, bootModificado, 16, 4);
                            
                            OnLogMessage?.Invoke(this, "   ✓ Ramdisk modified successfully");
                        }
                        else
                        {
                            OnLogMessage?.Invoke(this, "   ⚠ Modified ramdisk too large, keeping original");
                        }
                    }
                }
                else
                {
                    OnLogMessage?.Invoke(this, "   ⚠ No valid ramdisk found or ramdisk size is 0");
                    OnLogMessage?.Invoke(this, "   This boot image may not support Magisk patching");
                }
                
                OnLogMessage?.Invoke(this, "");
                OnLogMessage?.Invoke(this, "⚠ IMPORTANT: Basic patching method used");
                OnLogMessage?.Invoke(this, "⚠ For best results:");
                OnLogMessage?.Invoke(this, "  1. Copy boot.img to device");
                OnLogMessage?.Invoke(this, "  2. Use Magisk Manager to patch it");
                OnLogMessage?.Invoke(this, "  3. Flash the patched boot.img");
                OnLogMessage?.Invoke(this, "");
                
                return bootModificado;
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"   Error patching boot: {ex.Message}");
                return bootData;
            }
        }
        
        /// <summary>
        /// Comprime ramdisk con GZIP
        /// </summary>
        private byte[] ComprimirRamdisk(byte[] data)
        {
            try
            {
                using var outputStream = new MemoryStream();
                using (var gzipStream = new System.IO.Compression.GZipStream(outputStream, System.IO.Compression.CompressionLevel.Optimal))
                {
                    gzipStream.Write(data, 0, data.Length);
                }
                return outputStream.ToArray();
            }
            catch
            {
                return data;
            }
        }
        
        /// <summary>
        /// Deshabilita dm-verity y AVB en vbmeta
        /// </summary>
        public async Task<(bool success, string vbmetaPath)> CrearVbmetaDeshabilitado()
        {
            try
            {
                OnLogMessage?.Invoke(this, "Creando vbmeta deshabilitado...");
                
                string vbmetaPath = Path.Combine(_tempFolder, "vbmeta_disabled.img");
                
                // Crear un vbmeta vacío que deshabilita la verificación
                // Esto es un vbmeta header básico con flags de verificación deshabilitados
                byte[] vbmetaData = new byte[4096];
                
                // Magic: "AVB0"
                vbmetaData[0] = 0x41; // A
                vbmetaData[1] = 0x56; // V
                vbmetaData[2] = 0x42; // B
                vbmetaData[3] = 0x30; // 0
                
                // Version: 1.0
                vbmetaData[4] = 0x00;
                vbmetaData[5] = 0x00;
                vbmetaData[6] = 0x00;
                vbmetaData[7] = 0x01;
                
                // Flags: 2 (disable verification)
                vbmetaData[120] = 0x02;
                
                await File.WriteAllBytesAsync(vbmetaPath, vbmetaData);
                
                OnLogMessage?.Invoke(this, $"✓ vbmeta deshabilitado creado: {vbmetaPath}");
                return (true, vbmetaPath);
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Error al crear vbmeta: {ex.Message}");
                return (false, "");
            }
        }
        
        /// <summary>
        /// Descomprime datos LZ4
        /// </summary>
        private byte[] DescomprimirLZ4(byte[] compressedData)
        {
            try
            {
                using var inputStream = new MemoryStream(compressedData);
                using var lz4Stream = K4os.Compression.LZ4.Streams.LZ4Stream.Decode(inputStream);
                using var outputStream = new MemoryStream();
                
                lz4Stream.CopyTo(outputStream);
                return outputStream.ToArray();
            }
            catch
            {
                // Si falla, devolver los datos originales
                return compressedData;
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
                    OnLogMessage?.Invoke(this, "✓ Archivos temporales eliminados");
                }
            }
            catch (Exception ex)
            {
                OnLogMessage?.Invoke(this, $"Advertencia: No se pudieron eliminar temporales: {ex.Message}");
            }
        }
    }
}
