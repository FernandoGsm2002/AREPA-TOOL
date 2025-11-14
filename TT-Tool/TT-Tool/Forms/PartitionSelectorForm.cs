using TT_Tool.Controls;

namespace TT_Tool.Forms
{
    /// <summary>
    /// Formulario para seleccionar qué particiones flashear de un archivo TAR
    /// </summary>
    public class PartitionSelectorForm : Form
    {
        private CheckedListBox _listPartitions = null!;
        private ModernButton _btnAceptar = null!;
        private ModernButton _btnCancelar = null!;
        private ModernButton _btnSelectAll = null!;
        private ModernButton _btnDeselectAll = null!;
        private Label _lblInfo = null!;
        
        public List<PartitionInfo> Partitions { get; private set; }
        
        public PartitionSelectorForm(List<PartitionInfo> partitions, string fileName)
        {
            Partitions = partitions;
            InitializeComponent(fileName);
        }
        
        private void InitializeComponent(string fileName)
        {
            // Configuración del formulario
            this.Text = "Seleccionar Particiones";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            
            // Título
            var lblTitulo = new Label
            {
                Text = "Selecciona las particiones a flashear",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(66, 66, 66),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            this.Controls.Add(lblTitulo);
            
            // Info del archivo
            _lblInfo = new Label
            {
                Text = $"Archivo: {Path.GetFileName(fileName)}",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(120, 120, 120),
                AutoSize = true,
                Location = new Point(20, 50)
            };
            this.Controls.Add(_lblInfo);
            
            // Lista de particiones
            _listPartitions = new CheckedListBox
            {
                Location = new Point(20, 80),
                Size = new Size(440, 220),
                Font = new Font("Segoe UI", 9F),
                CheckOnClick = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Agregar particiones a la lista
            foreach (var partition in Partitions)
            {
                string displayText = $"{partition.FileName} ({partition.Size / 1024 / 1024} MB)";
                _listPartitions.Items.Add(displayText, partition.Enabled);
            }
            
            this.Controls.Add(_listPartitions);
            
            // Botones de selección rápida
            _btnSelectAll = new ModernButton
            {
                Text = "✓ Seleccionar Todo",
                Location = new Point(20, 310),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                BorderRadius = 4,
                HoverColor = Color.FromArgb(41, 128, 185)
            };
            _btnSelectAll.Click += (s, e) =>
            {
                for (int i = 0; i < _listPartitions.Items.Count; i++)
                {
                    _listPartitions.SetItemChecked(i, true);
                }
            };
            this.Controls.Add(_btnSelectAll);
            
            _btnDeselectAll = new ModernButton
            {
                Text = "✗ Deseleccionar Todo",
                Location = new Point(170, 310),
                Size = new Size(140, 30),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                BorderRadius = 4,
                HoverColor = Color.FromArgb(127, 140, 141)
            };
            _btnDeselectAll.Click += (s, e) =>
            {
                for (int i = 0; i < _listPartitions.Items.Count; i++)
                {
                    _listPartitions.SetItemChecked(i, false);
                }
            };
            this.Controls.Add(_btnDeselectAll);
            
            // Botones de acción
            _btnAceptar = new ModernButton
            {
                Text = "Aceptar",
                Location = new Point(260, 360),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                BorderRadius = 6,
                HoverColor = Color.FromArgb(39, 174, 96),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _btnAceptar.Click += (s, e) =>
            {
                // Actualizar el estado de las particiones
                for (int i = 0; i < _listPartitions.Items.Count; i++)
                {
                    Partitions[i].Enabled = _listPartitions.GetItemChecked(i);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(_btnAceptar);
            
            _btnCancelar = new ModernButton
            {
                Text = "Cancelar",
                Location = new Point(370, 360),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                BorderRadius = 6,
                HoverColor = Color.FromArgb(127, 140, 141)
            };
            _btnCancelar.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(_btnCancelar);
        }
    }
    
    /// <summary>
    /// Información de una partición
    /// </summary>
    public class PartitionInfo
    {
        public bool Enabled { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public long Size { get; set; }
    }
}
