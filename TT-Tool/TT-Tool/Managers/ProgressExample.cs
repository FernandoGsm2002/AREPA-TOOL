namespace TT_Tool.Managers
{
    /// <summary>
    /// Ejemplo de uso del sistema de progreso y cancelación
    /// </summary>
    public class ProgressExample
    {
        private readonly LogManager _logManager;

        public ProgressExample(LogManager logManager)
        {
            _logManager = logManager;
        }

        /// <summary>
        /// Ejemplo de operación larga con progreso y cancelación
        /// </summary>
        public async Task EjecutarOperacionConProgreso()
        {
            // Iniciar progreso (retorna un CancellationToken)
            var token = _logManager.IniciarProgreso("Iniciando operación...", 100);

            try
            {
                _logManager.AgregarLog("Comenzando operación de ejemplo", TipoLog.Info);

                // Simular operación en pasos
                for (int i = 0; i <= 100; i += 10)
                {
                    // Verificar si se solicitó cancelación
                    if (token.IsCancellationRequested)
                    {
                        _logManager.AgregarLog("Operación cancelada", TipoLog.Advertencia);
                        _logManager.FinalizarProgreso("Cancelado", false);
                        return;
                    }

                    // Actualizar progreso
                    _logManager.ActualizarProgreso(i, $"Procesando... {i}%");
                    _logManager.AgregarLog($"Paso {i}/100 completado");

                    // Simular trabajo
                    await Task.Delay(500, token);
                }

                // Operación completada con éxito
                _logManager.AgregarLog("Operación completada exitosamente", TipoLog.Exito);
                _logManager.FinalizarProgreso("✓ Completado", true);
            }
            catch (OperationCanceledException)
            {
                _logManager.AgregarLog("Operación cancelada por el usuario", TipoLog.Advertencia);
                _logManager.FinalizarProgreso("Cancelado", false);
            }
            catch (Exception ex)
            {
                _logManager.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                _logManager.FinalizarProgreso("✗ Error", false);
            }
        }

        /// <summary>
        /// Ejemplo de operación con progreso indeterminado
        /// </summary>
        public async Task EjecutarOperacionIndeterminada()
        {
            var token = _logManager.IniciarProgreso("Procesando...", 0);

            try
            {
                _logManager.AgregarLog("Iniciando operación indeterminada", TipoLog.Info);

                // Simular operación sin progreso específico
                await Task.Delay(3000, token);

                if (!token.IsCancellationRequested)
                {
                    _logManager.AgregarLog("Operación completada", TipoLog.Exito);
                    _logManager.FinalizarProgreso("✓ Listo", true);
                }
            }
            catch (OperationCanceledException)
            {
                _logManager.AgregarLog("Operación cancelada", TipoLog.Advertencia);
                _logManager.FinalizarProgreso("Cancelado", false);
            }
        }

        /// <summary>
        /// Ejemplo de múltiples pasos con actualización de mensaje
        /// </summary>
        public async Task EjecutarOperacionMultiPaso()
        {
            var token = _logManager.IniciarProgreso("Preparando...", 5);

            try
            {
                // Paso 1
                _logManager.ActualizarProgreso(1, "Paso 1: Verificando dispositivo...");
                _logManager.AgregarLog("Verificando dispositivo conectado");
                await Task.Delay(1000, token);
                token.ThrowIfCancellationRequested();

                // Paso 2
                _logManager.ActualizarProgreso(2, "Paso 2: Descargando archivos...");
                _logManager.AgregarLog("Descargando archivos necesarios");
                await Task.Delay(1000, token);
                token.ThrowIfCancellationRequested();

                // Paso 3
                _logManager.ActualizarProgreso(3, "Paso 3: Instalando...");
                _logManager.AgregarLog("Instalando componentes");
                await Task.Delay(1000, token);
                token.ThrowIfCancellationRequested();

                // Paso 4
                _logManager.ActualizarProgreso(4, "Paso 4: Configurando...");
                _logManager.AgregarLog("Configurando sistema");
                await Task.Delay(1000, token);
                token.ThrowIfCancellationRequested();

                // Paso 5
                _logManager.ActualizarProgreso(5, "Paso 5: Finalizando...");
                _logManager.AgregarLog("Finalizando instalación");
                await Task.Delay(1000, token);

                _logManager.AgregarLog("Todos los pasos completados", TipoLog.Exito);
                _logManager.FinalizarProgreso("✓ Instalación completa", true);
            }
            catch (OperationCanceledException)
            {
                _logManager.AgregarLog("Instalación cancelada", TipoLog.Advertencia);
                _logManager.FinalizarProgreso("Cancelado", false);
            }
            catch (Exception ex)
            {
                _logManager.AgregarLog($"Error en instalación: {ex.Message}", TipoLog.Error);
                _logManager.FinalizarProgreso("✗ Error", false);
            }
        }
    }
}
