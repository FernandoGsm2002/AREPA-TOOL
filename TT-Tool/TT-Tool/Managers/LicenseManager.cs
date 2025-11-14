using System.Text.Json;

namespace TT_Tool.Managers
{
    /// <summary>
    /// Gestor de licencia y control remoto de la aplicación
    /// </summary>
    public class LicenseManager
    {
        // URL donde alojarás el archivo de configuración JSON
        // Puedes usar: GitHub Raw, tu servidor web, Pastebin, etc.
        private const string LICENSE_URL = "https://raw.githubusercontent.com/FernandoGsm2002/arepa-tool-config/main/license.json";
        
        // Timeout de verificación (5 segundos)
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Verifica si la aplicación está habilitada para su uso
        /// </summary>
        public static async Task<(bool isEnabled, string message)> VerificarLicencia()
        {
            try
            {
                using var client = new HttpClient { Timeout = Timeout };
                client.DefaultRequestHeaders.Add("User-Agent", "AREPA-TOOL");
                
                // Descargar configuración remota
                var response = await client.GetStringAsync(LICENSE_URL);
                var config = JsonSerializer.Deserialize<LicenseConfig>(response);

                if (config == null)
                {
                    // Si no puede deserializar, permitir acceso (fail-open)
                    return (true, string.Empty);
                }

                // Verificar si está habilitada
                if (!config.Enabled)
                {
                    return (false, config.Message ?? "Esta versión de AREPA-TOOL ha sido desactivada.\n\nPor favor descarga la última versión desde:\nLeoPE-GSM.COM");
                }

                // Verificar fecha de expiración (opcional)
                if (config.ExpirationDate.HasValue && DateTime.Now > config.ExpirationDate.Value)
                {
                    return (false, "El periodo de prueba gratuito ha finalizado.\n\nPara continuar usando AREPA-TOOL, visita:\nLeoPE-GSM.COM");
                }

                // Todo OK
                return (true, config.WelcomeMessage ?? string.Empty);
            }
            catch (TaskCanceledException)
            {
                // Timeout - permitir acceso sin verificación
                return (true, string.Empty);
            }
            catch (HttpRequestException)
            {
                // Error de red - permitir acceso sin verificación
                return (true, string.Empty);
            }
            catch
            {
                // Cualquier otro error - permitir acceso (fail-open)
                return (true, string.Empty);
            }
        }

        /// <summary>
        /// Configuración de licencia descargada del servidor
        /// </summary>
        private class LicenseConfig
        {
            /// <summary>
            /// Si es true, la app funciona. Si es false, se bloquea.
            /// </summary>
            public bool Enabled { get; set; } = true;

            /// <summary>
            /// Mensaje personalizado a mostrar al usuario
            /// </summary>
            public string? Message { get; set; }

            /// <summary>
            /// Mensaje de bienvenida (opcional)
            /// </summary>
            public string? WelcomeMessage { get; set; }

            /// <summary>
            /// Fecha de expiración (opcional)
            /// </summary>
            public DateTime? ExpirationDate { get; set; }

            /// <summary>
            /// Versión mínima requerida (opcional, para futuras actualizaciones)
            /// </summary>
            public string? MinimumVersion { get; set; }

            /// <summary>
            /// URL de descarga de la nueva versión (opcional)
            /// </summary>
            public string? UpdateUrl { get; set; }
        }
    }
}

