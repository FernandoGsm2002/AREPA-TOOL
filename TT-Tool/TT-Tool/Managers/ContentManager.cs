namespace TT_Tool.Managers
{
    /// <summary>
    /// Gestor de contenido dinámico para los paneles
    /// </summary>
    public class ContentManager
    {
        private readonly Label _lblTitulo;
        private readonly RichTextBox _txtContenido;
        private Button? _botonActual;

        private readonly Color _colorActivo = Color.FromArgb(0, 150, 136);
        private readonly Color _colorInactivo = Color.FromArgb(55, 55, 60);

        public ContentManager(Label lblTitulo, RichTextBox txtContenido)
        {
            _lblTitulo = lblTitulo ?? throw new ArgumentNullException(nameof(lblTitulo));
            _txtContenido = txtContenido ?? throw new ArgumentNullException(nameof(txtContenido));
        }

        /// <summary>
        /// Cambia el botón activo visualmente
        /// </summary>
        public void CambiarBotonActivo(Button botonNuevo)
        {
            if (_botonActual != null)
            {
                _botonActual.BackColor = _colorInactivo;
            }

            _botonActual = botonNuevo;
            _botonActual.BackColor = _colorActivo;
        }

        /// <summary>
        /// Actualiza el contenido del panel
        /// </summary>
        public void ActualizarContenido(string titulo, string contenido)
        {
            _lblTitulo.Text = titulo;
            _txtContenido.Text = contenido;
        }

        /// <summary>
        /// Carga contenido desde una sección específica
        /// </summary>
        public void CargarSeccion(SeccionContenido seccion, Button boton)
        {
            CambiarBotonActivo(boton);
            ActualizarContenido(seccion.Titulo, seccion.Contenido);
        }
    }

    /// <summary>
    /// Clase que representa una sección de contenido
    /// </summary>
    public class SeccionContenido
    {
        public string Titulo { get; set; } = "";
        public string Contenido { get; set; } = "";

        public SeccionContenido(string titulo, string contenido)
        {
            Titulo = titulo;
            Contenido = contenido;
        }
    }
}

