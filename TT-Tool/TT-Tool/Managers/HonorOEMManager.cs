using System.Text;
using System.Text.RegularExpressions;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Manager para operaciones OEM en dispositivos Honor/Huawei
    /// </summary>
    public class HonorOEMManager
    {
        private LogManager? _logManager;

        public void SetLogManager(LogManager logManager)
        {
            _logManager = logManager;
        }

        private void Log(string mensaje, TipoLog tipo = TipoLog.Info)
        {
            _logManager?.AgregarLog(mensaje, tipo);
        }

        /// <summary>
        /// Analiza y modifica un archivo OEM Info
        /// </summary>
        public async Task<bool> AnalizarYModificarOEMInfo(string rutaArchivo)
        {
            try
            {
                Log("=== ANALYZING OEM INFO FILE ===");
                Log($"File: {Path.GetFileName(rutaArchivo)}");
                Log("");

                // Leer el archivo
                byte[] contenido = await File.ReadAllBytesAsync(rutaArchivo);
                Log($"File size: {contenido.Length} bytes ({contenido.Length:N0} bytes)");

                // Convertir a string para análisis
                string contenidoTexto = Encoding.ASCII.GetString(contenido);

                // Buscar modelo en el OEM Info
                string? modelo = ExtraerModelo(contenidoTexto);
                if (!string.IsNullOrEmpty(modelo))
                {
                    Log($"✓ Model detected: {modelo}", TipoLog.Exito);
                }

                Log("");
                Log("Analyzing patterns (HEX mode)...");

                // Detectar tipo de bloqueo
                string tipoBloqueo = DetectarTipoBloqueo(contenido, contenidoTexto);
                
                if (tipoBloqueo == "NONE")
                {
                    Log("✓ No target patterns detected", TipoLog.Exito);
                    Log("File does not need modification");
                    return false;
                }

                Log($"✓ Pattern detected: {tipoBloqueo}", TipoLog.Advertencia);
                Log("");

                // Modificar el contenido (HEX)
                byte[] contenidoModificado = ModificarContenidoHex(contenido, tipoBloqueo);

                // Generar nombre de archivo modificado
                string directorio = Path.GetDirectoryName(rutaArchivo) ?? "";
                string nombreArchivo = Path.GetFileNameWithoutExtension(rutaArchivo);
                string extension = Path.GetExtension(rutaArchivo);
                string rutaModificada = Path.Combine(directorio, $"{nombreArchivo}_modificado{extension}");

                // Guardar archivo modificado
                await File.WriteAllBytesAsync(rutaModificada, contenidoModificado);

                Log("=== PATCHING COMPLETED ===", TipoLog.Exito);
                Log($"Original pattern: {tipoBloqueo}");
                Log($"Patched with: HEX replacement");
                Log("");
                Log($"✓ Modified file saved:", TipoLog.Exito);
                Log($"  {Path.GetFileName(rutaModificada)}");

                return true;
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}", TipoLog.Error);
                return false;
            }
        }

        /// <summary>
        /// Extrae el modelo del dispositivo del contenido OEM
        /// </summary>
        private string? ExtraerModelo(string contenido)
        {
            try
            {
                // Buscar patrones comunes de modelo
                var patrones = new[]
                {
                    @"MODEL[:\s]+([A-Z0-9\-]+)",
                    @"PRODUCT[:\s]+([A-Z0-9\-]+)",
                    @"DEVICE[:\s]+([A-Z0-9\-]+)",
                    @"([A-Z]{3,5}-[A-Z0-9]{2,4})", // Formato típico Honor/Huawei
                };

                foreach (var patron in patrones)
                {
                    var match = Regex.Match(contenido, patron, RegexOptions.IgnoreCase);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        return match.Groups[1].Value.Trim();
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Detecta el tipo de bloqueo en el archivo
        /// </summary>
        private string DetectarTipoBloqueo(byte[] contenido, string contenidoTexto)
        {
            // Buscar patrones de bloqueo
            if (BuscarPatron(contenido, "def"))
            {
                return "DEF";
            }
            else if (BuscarPatron(contenido, "claro"))
            {
                return "CLARO";
            }
            else if (BuscarPatron(contenido, "telcel"))
            {
                return "TELCEL";
            }

            return "NONE";
        }

        /// <summary>
        /// Busca un patrón en el contenido del archivo
        /// </summary>
        private bool BuscarPatron(byte[] contenido, string patron)
        {
            byte[] patronBytes = Encoding.ASCII.GetBytes(patron);
            
            for (int i = 0; i <= contenido.Length - patronBytes.Length; i++)
            {
                bool encontrado = true;
                for (int j = 0; j < patronBytes.Length; j++)
                {
                    if (contenido[i + j] != patronBytes[j])
                    {
                        encontrado = false;
                        break;
                    }
                }
                if (encontrado)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Modifica el contenido según el tipo de bloqueo (HEX mode)
        /// </summary>
        private byte[] ModificarContenidoHex(byte[] contenido, string tipoBloqueo)
        {
            byte[] contenidoModificado = new byte[contenido.Length];
            Array.Copy(contenido, contenidoModificado, contenido.Length);

            // Definir reemplazos en HEX
            // ÿ = 0xFF en hexadecimal
            byte[][] reemplazoHex = tipoBloqueo switch
            {
                "DEF" => new byte[][] { new byte[] { 0xFF, 0x68, 0x77 } },  // ÿhw
                "CLARO" => new byte[][] { new byte[] { 0xFF, 0xFF, 0xFF, 0x68, 0x77 } },  // ÿÿÿhw
                "TELCEL" => new byte[][] { new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x68, 0x77 } },  // ÿÿÿÿhw
                _ => new byte[][] { }
            };

            if (reemplazoHex.Length == 0)
                return contenidoModificado;

            // Buscar y reemplazar el patrón
            byte[] patronOriginal = Encoding.ASCII.GetBytes(tipoBloqueo.ToLower());
            byte[] patronReemplazo = reemplazoHex[0];

            for (int i = 0; i <= contenidoModificado.Length - patronOriginal.Length; i++)
            {
                bool encontrado = true;
                for (int j = 0; j < patronOriginal.Length; j++)
                {
                    if (contenidoModificado[i + j] != patronOriginal[j])
                    {
                        encontrado = false;
                        break;
                    }
                }

                if (encontrado)
                {
                    // Reemplazar con valores HEX
                    for (int j = 0; j < patronReemplazo.Length && (i + j) < contenidoModificado.Length; j++)
                    {
                        contenidoModificado[i + j] = patronReemplazo[j];
                    }
                    break;
                }
            }

            return contenidoModificado;
        }
    }
}
