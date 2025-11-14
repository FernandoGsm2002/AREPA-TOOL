namespace TT_Tool.Managers
{
    /// <summary>
    /// Gestor centralizado de logs para la aplicación
    /// </summary>
    public class LogManager
    {
        private readonly RichTextBox _txtLogs;
        private readonly object _lockObj = new object();
        
        // Controles de progreso
        private ProgressBar? _progressBar;
        private Label? _lblEstado;
        private Button? _btnCancelar;
        
        // Sistema de cancelación
        private CancellationTokenSource? _cancellationTokenSource;

        public LogManager(RichTextBox txtLogs)
        {
            _txtLogs = txtLogs ?? throw new ArgumentNullException(nameof(txtLogs));
        }

        /// <summary>
        /// Configura los controles de progreso del footer
        /// </summary>
        public void ConfigurarControlesProgreso(ProgressBar progressBar, Label lblEstado, Button btnCancelar)
        {
            _progressBar = progressBar;
            _lblEstado = lblEstado;
            _btnCancelar = btnCancelar;
        }

        /// <summary>
        /// Agrega un mensaje al log con timestamp
        /// </summary>
        public void AgregarLog(string mensaje, TipoLog tipo = TipoLog.Info)
        {
            if (_txtLogs.InvokeRequired)
            {
                _txtLogs.Invoke(() => AgregarLogInterno(mensaje, tipo));
            }
            else
            {
                AgregarLogInterno(mensaje, tipo);
            }
        }

        private void AgregarLogInterno(string mensaje, TipoLog tipo)
        {
            lock (_lockObj)
            {
                string prefijo = ObtenerPrefijo(tipo);
                
                int startIndex = _txtLogs.TextLength;
                _txtLogs.AppendText($"{prefijo}{mensaje}\n");
                
                // Colorear según el tipo
                int endIndex = _txtLogs.TextLength;
                _txtLogs.Select(startIndex, endIndex - startIndex);
                _txtLogs.SelectionColor = ObtenerColor(tipo);
                
                _txtLogs.ScrollToCaret();
                _txtLogs.Select(endIndex, 0);
            }
        }

        /// <summary>
        /// Agrega una instrucción estática con fuente más pequeña (7F)
        /// </summary>
        public void AgregarInstruccion(string mensaje, TipoLog tipo = TipoLog.Info)
        {
            if (_txtLogs.InvokeRequired)
            {
                _txtLogs.Invoke(() => AgregarInstruccionInterno(mensaje, tipo));
            }
            else
            {
                AgregarInstruccionInterno(mensaje, tipo);
            }
        }

        private void AgregarInstruccionInterno(string mensaje, TipoLog tipo)
        {
            lock (_lockObj)
            {
                string prefijo = ObtenerPrefijo(tipo);
                
                int startIndex = _txtLogs.TextLength;
                _txtLogs.AppendText($"{prefijo}{mensaje}\n");
                
                // Aplicar formato con fuente más pequeña
                int endIndex = _txtLogs.TextLength;
                _txtLogs.Select(startIndex, endIndex - startIndex);
                _txtLogs.SelectionColor = ObtenerColor(tipo);
                _txtLogs.SelectionFont = new Font("Consolas", 7F, FontStyle.Bold);
                
                _txtLogs.ScrollToCaret();
                _txtLogs.Select(endIndex, 0);
            }
        }

        /// <summary>
        /// Limpia todos los logs
        /// </summary>
        public void LimpiarLogs()
        {
            if (_txtLogs.InvokeRequired)
            {
                _txtLogs.Invoke(() => _txtLogs.Clear());
            }
            else
            {
                _txtLogs.Clear();
            }
        }

        private string ObtenerPrefijo(TipoLog tipo)
        {
            return tipo switch
            {
                TipoLog.Error => "[ERROR] ",
                TipoLog.Advertencia => "[WARN] ",
                TipoLog.Exito => "[OK] ",
                TipoLog.Info => "",
                _ => ""
            };
        }

        private Color ObtenerColor(TipoLog tipo)
        {
            return tipo switch
            {
                TipoLog.Error => Color.FromArgb(220, 20, 60),       // Rojo más intenso (Crimson)
                TipoLog.Advertencia => Color.FromArgb(255, 140, 0), // Naranja brillante (DarkOrange)
                TipoLog.Exito => Color.FromArgb(34, 139, 34),       // Verde más intenso (ForestGreen)
                TipoLog.Info => Color.FromArgb(25, 118, 210),       // Azul vibrante
                TipoLog.Titulo => Color.FromArgb(138, 43, 226),     // Púrpura vibrante (BlueViolet)
                TipoLog.Proceso => Color.FromArgb(0, 150, 136),     // Teal/Cyan intenso
                TipoLog.Comando => Color.FromArgb(255, 87, 34),     // Naranja rojizo (Deep Orange)
                _ => Color.FromArgb(25, 118, 210)
            };
        }

        // ============= MÉTODOS DE BARRA DE PROGRESO =============

        /// <summary>
        /// Inicia una operación con barra de progreso
        /// </summary>
        public CancellationToken IniciarProgreso(string mensaje, int valorMaximo = 100)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if (_progressBar != null && _progressBar.InvokeRequired)
            {
                _progressBar.Invoke(() => IniciarProgresoInterno(mensaje, valorMaximo));
            }
            else
            {
                IniciarProgresoInterno(mensaje, valorMaximo);
            }

            return _cancellationTokenSource.Token;
        }

        private void IniciarProgresoInterno(string mensaje, int valorMaximo)
        {
            if (_progressBar != null)
            {
                _progressBar.Value = 0;
                _progressBar.Maximum = valorMaximo;
                _progressBar.Visible = true;
            }

            if (_lblEstado != null)
            {
                _lblEstado.Text = mensaje;
                _lblEstado.ForeColor = Color.FromArgb(66, 133, 244);
            }

            if (_btnCancelar != null)
            {
                _btnCancelar.Visible = true;
                _btnCancelar.Enabled = true;
            }
        }

        /// <summary>
        /// Actualiza el progreso de la operación
        /// </summary>
        public void ActualizarProgreso(int valor, string? mensaje = null)
        {
            if (_progressBar != null && _progressBar.InvokeRequired)
            {
                _progressBar.Invoke(() => ActualizarProgresoInterno(valor, mensaje));
            }
            else
            {
                ActualizarProgresoInterno(valor, mensaje);
            }
        }

        private void ActualizarProgresoInterno(int valor, string? mensaje)
        {
            if (_progressBar != null && valor <= _progressBar.Maximum)
            {
                _progressBar.Value = valor;
            }

            if (_lblEstado != null && !string.IsNullOrEmpty(mensaje))
            {
                _lblEstado.Text = mensaje;
            }
        }

        /// <summary>
        /// Finaliza la operación con progreso
        /// </summary>
        public void FinalizarProgreso(string mensaje, bool exito = true)
        {
            if (_progressBar != null && _progressBar.InvokeRequired)
            {
                _progressBar.Invoke(() => FinalizarProgresoInterno(mensaje, exito));
            }
            else
            {
                FinalizarProgresoInterno(mensaje, exito);
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private void FinalizarProgresoInterno(string mensaje, bool exito)
        {
            if (_progressBar != null)
            {
                _progressBar.Value = exito ? _progressBar.Maximum : 0;
                
                // Ocultar después de un breve delay
                Task.Delay(1000).ContinueWith(_ =>
                {
                    if (_progressBar.InvokeRequired)
                    {
                        _progressBar.Invoke(() => _progressBar.Visible = false);
                    }
                    else
                    {
                        _progressBar.Visible = false;
                    }
                });
            }

            if (_lblEstado != null)
            {
                _lblEstado.Text = mensaje;
                _lblEstado.ForeColor = exito 
                    ? Color.FromArgb(76, 175, 80) 
                    : Color.FromArgb(244, 67, 54);
                
                // Restaurar después de un delay
                Task.Delay(3000).ContinueWith(_ =>
                {
                    if (_lblEstado.InvokeRequired)
                    {
                        _lblEstado.Invoke(() =>
                        {
                            _lblEstado.Text = "Listo";
                            _lblEstado.ForeColor = Color.FromArgb(100, 105, 115);
                        });
                    }
                    else
                    {
                        _lblEstado.Text = "Listo";
                        _lblEstado.ForeColor = Color.FromArgb(100, 105, 115);
                    }
                });
            }

            if (_btnCancelar != null)
            {
                _btnCancelar.Visible = false;
            }
        }

        /// <summary>
        /// Cancela la operación actual
        /// </summary>
        public void CancelarOperacion()
        {
            _cancellationTokenSource?.Cancel();
            AgregarLog("⚠ Operación cancelada por el usuario", TipoLog.Advertencia);
            FinalizarProgreso("Cancelado", false);
        }

        /// <summary>
        /// Verifica si hay una cancelación pendiente
        /// </summary>
        public bool EsCancelacionSolicitada()
        {
            return _cancellationTokenSource?.IsCancellationRequested ?? false;
        }
    }

    /// <summary>
    /// Tipos de log disponibles
    /// </summary>
    public enum TipoLog
    {
        Info,
        Exito,
        Advertencia,
        Error,
        Titulo,      // Para encabezados como "=== XXX ==="
        Proceso,     // Para pasos numerados como "[1/5]"
        Comando      // Para comandos y acciones técnicas
    }
}

