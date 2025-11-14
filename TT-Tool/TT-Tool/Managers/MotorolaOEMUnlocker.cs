using System.Security.Cryptography;
using System.Text;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Generador de claves OEM para desbloqueo de bootloader Motorola
    /// Basado en el algoritmo de moto-utils
    /// </summary>
    public class MotorolaOEMUnlocker
    {
        /// <summary>
        /// Genera la clave de desbloqueo OEM a partir de la clave del dispositivo
        /// </summary>
        /// <param name="deviceKey">Clave obtenida con 'fastboot oem get_key'</param>
        /// <returns>Clave de desbloqueo de 32 caracteres</returns>
        public static string GenerarClaveDesbloqueo(string deviceKey)
        {
            if (string.IsNullOrWhiteSpace(deviceKey))
            {
                throw new ArgumentException("Device key cannot be empty", nameof(deviceKey));
            }

            // Limpiar la clave (remover espacios y caracteres especiales)
            deviceKey = deviceKey.Trim().Replace(" ", "").Replace("-", "").Replace("#", "");

            if (deviceKey.Length != 32)
            {
                throw new ArgumentException($"Device key must be 32 characters long (current: {deviceKey.Length})", nameof(deviceKey));
            }

            // Duplicar la clave
            string toHash = deviceKey + deviceKey;

            // Calcular SHA256
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(toHash));
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                // Retornar los primeros 32 caracteres
                return hash.Substring(0, 32);
            }
        }

        /// <summary>
        /// Valida si una clave de dispositivo tiene el formato correcto
        /// </summary>
        public static bool ValidarFormatoClave(string deviceKey)
        {
            if (string.IsNullOrWhiteSpace(deviceKey))
                return false;

            // Limpiar la clave
            deviceKey = deviceKey.Trim().Replace(" ", "").Replace("-", "").Replace("#", "");

            // Debe tener 32 caracteres hexadecimales
            if (deviceKey.Length != 32)
                return false;

            // Verificar que solo contenga caracteres hexadecimales
            return System.Text.RegularExpressions.Regex.IsMatch(deviceKey, "^[0-9A-Fa-f]{32}$");
        }

        /// <summary>
        /// Obtiene la clave del dispositivo usando fastboot
        /// </summary>
        public static async Task<string?> ObtenerClaveDispositivo()
        {
            try
            {
                var adbManager = new ADBManager();
                
                // Ejecutar comando fastboot oem get_key
                var resultado = await adbManager.ExecuteAdbCommandAsync("fastboot oem get_key");

                if (!string.IsNullOrEmpty(resultado))
                {
                    // Buscar la clave en la salida
                    // El formato típico es: "Device key: XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
                    var lineas = resultado.Split('\n');
                    foreach (var linea in lineas)
                    {
                        if (linea.Contains("key", StringComparison.OrdinalIgnoreCase))
                        {
                            // Extraer la clave (últimos 32 caracteres hex)
                            var match = System.Text.RegularExpressions.Regex.Match(linea, "[0-9A-Fa-f]{32}");
                            if (match.Success)
                            {
                                return match.Value;
                            }
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
