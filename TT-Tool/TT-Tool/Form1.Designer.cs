using TT_Tool.Controls;

namespace TT_Tool
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            // Panel superior (marcas + logo)
            panelSuperior = new Panel();
            btnAndroid = new ModernButton();
            btnSamsung = new ModernButton();
            btnHonor = new ModernButton();
            btnMotorola = new ModernButton();
            btnQualcomm = new ModernButton();
            picLogo = new PictureBox();
            
            // Línea divisoria
            var lineaDivisoria = new Panel();

            // Panel principal
            panelPrincipal = new Panel();
            
            // Panel izquierdo (operaciones)
            panelIzquierdo = new ModernPanel();
            panelContenido = new Panel();

            // Panel derecho
            panelDerecho = new Panel();
            
            // Panel superior derecho (controles)
            panelSuperiorDerecho = new ModernPanel();
            lblADB = new Label();
            cmbDispositivoADB = new ModernComboBox();
            lblCOM = new Label();
            cmbDispositivoCOM = new ModernComboBox();
            
            // Espaciador
            var espaciadorDerecho = new Panel();

            // Panel logs
            panelLogs = new ModernPanel();
            txtLogs = new RichTextBox();

            // Panel footer (barra de progreso)
            panelFooter = new ModernPanel();
            lblEstadoProgreso = new Label();
            progressBar = new ProgressBar();
            btnCancelar = new ModernButton();

            panelSuperior.SuspendLayout();
            panelPrincipal.SuspendLayout();
            panelIzquierdo.SuspendLayout();
            panelDerecho.SuspendLayout();
            panelSuperiorDerecho.SuspendLayout();
            panelLogs.SuspendLayout();
            panelFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            SuspendLayout();

            // 
            // panelSuperior (Botones de marcas + Logo)
            // 
            panelSuperior.BackColor = Color.FromArgb(248, 249, 250);
            panelSuperior.Controls.Add(picLogo);
            panelSuperior.Controls.Add(btnAndroid);
            panelSuperior.Controls.Add(btnSamsung);
            panelSuperior.Controls.Add(btnHonor);
            panelSuperior.Controls.Add(btnMotorola);
            panelSuperior.Controls.Add(btnQualcomm);
            panelSuperior.Dock = DockStyle.Top;
            panelSuperior.Location = new Point(0, 0);
            panelSuperior.Name = "panelSuperior";
            panelSuperior.Padding = new Padding(10, 6, 10, 6);
            panelSuperior.Size = new Size(1100, 58);
            panelSuperior.TabIndex = 0;
            
            // 
            // lineaDivisoria
            // 
            lineaDivisoria.BackColor = Color.FromArgb(225, 228, 232);
            lineaDivisoria.Dock = DockStyle.Top;
            lineaDivisoria.Location = new Point(0, 58);
            lineaDivisoria.Name = "lineaDivisoria";
            lineaDivisoria.Size = new Size(1100, 1);
            lineaDivisoria.TabIndex = 2;

            // 
            // btnAndroid
            // 
            btnAndroid.BackColor = Color.FromArgb(66, 133, 244);
            btnAndroid.BorderRadius = 8;
            btnAndroid.FlatAppearance.BorderSize = 0;
            btnAndroid.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAndroid.ForeColor = Color.White;
            btnAndroid.HoverColor = Color.FromArgb(51, 103, 214);
            btnAndroid.Location = new Point(10, 8);
            btnAndroid.Name = "btnAndroid";
            btnAndroid.Size = new Size(130, 40);
            btnAndroid.TabIndex = 0;
            btnAndroid.Text = "Android";
            btnAndroid.UseVisualStyleBackColor = false;
            btnAndroid.Click += BtnAndroid_Click;

            // 
            // btnSamsung
            // 
            btnSamsung.BackColor = Color.FromArgb(230, 232, 235);
            btnSamsung.BorderRadius = 8;
            btnSamsung.FlatAppearance.BorderSize = 0;
            btnSamsung.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnSamsung.ForeColor = Color.FromArgb(80, 85, 92);
            btnSamsung.HoverColor = Color.FromArgb(210, 215, 220);
            btnSamsung.Location = new Point(148, 8);
            btnSamsung.Name = "btnSamsung";
            btnSamsung.Size = new Size(130, 40);
            btnSamsung.TabIndex = 1;
            btnSamsung.Text = "Samsung";
            btnSamsung.UseVisualStyleBackColor = false;
            btnSamsung.Click += BtnSamsung_Click;

            // 
            // btnHonor
            // 
            btnHonor.BackColor = Color.FromArgb(230, 232, 235);
            btnHonor.BorderRadius = 8;
            btnHonor.FlatAppearance.BorderSize = 0;
            btnHonor.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnHonor.ForeColor = Color.FromArgb(80, 85, 92);
            btnHonor.HoverColor = Color.FromArgb(210, 215, 220);
            btnHonor.Location = new Point(286, 8);
            btnHonor.Name = "btnHonor";
            btnHonor.Size = new Size(130, 40);
            btnHonor.TabIndex = 2;
            btnHonor.Text = "Honor";
            btnHonor.UseVisualStyleBackColor = false;
            btnHonor.Click += BtnHonor_Click;

            // 
            // btnMotorola
            // 
            btnMotorola.BackColor = Color.FromArgb(230, 232, 235);
            btnMotorola.BorderRadius = 8;
            btnMotorola.FlatAppearance.BorderSize = 0;
            btnMotorola.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnMotorola.ForeColor = Color.FromArgb(80, 85, 92);
            btnMotorola.HoverColor = Color.FromArgb(210, 215, 220);
            btnMotorola.Location = new Point(424, 8);
            btnMotorola.Name = "btnMotorola";
            btnMotorola.Size = new Size(130, 40);
            btnMotorola.TabIndex = 3;
            btnMotorola.Text = "Motorola";
            btnMotorola.UseVisualStyleBackColor = false;
            btnMotorola.Click += BtnMotorola_Click;

            // 
            // btnQualcomm
            // 
            btnQualcomm.BackColor = Color.FromArgb(230, 232, 235);
            btnQualcomm.BorderRadius = 8;
            btnQualcomm.FlatAppearance.BorderSize = 0;
            btnQualcomm.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            btnQualcomm.ForeColor = Color.FromArgb(80, 85, 92);
            btnQualcomm.HoverColor = Color.FromArgb(210, 215, 220);
            btnQualcomm.Location = new Point(562, 8);
            btnQualcomm.Name = "btnQualcomm";
            btnQualcomm.Size = new Size(130, 40);
            btnQualcomm.TabIndex = 4;
            btnQualcomm.Text = "Xiaomi";
            btnQualcomm.UseVisualStyleBackColor = false;
            btnQualcomm.Click += BtnQualcomm_Click;

            // 
            // panelPrincipal
            // 
            panelPrincipal.BackColor = Color.FromArgb(240, 242, 245);
            panelPrincipal.Controls.Add(panelDerecho);
            panelPrincipal.Controls.Add(panelIzquierdo);
            panelPrincipal.Dock = DockStyle.Fill;
            panelPrincipal.Location = new Point(0, 59);
            panelPrincipal.Name = "panelPrincipal";
            panelPrincipal.Padding = new Padding(10, 10, 10, 0);
            panelPrincipal.Size = new Size(1100, 541);
            panelPrincipal.TabIndex = 1;

            // 
            // panelIzquierdo
            // 
            panelIzquierdo.BackColor = Color.FromArgb(240, 242, 245);
            panelIzquierdo.BorderColor = Color.FromArgb(240, 242, 245);
            panelIzquierdo.BorderRadius = 0;
            panelIzquierdo.BorderSize = 0;
            panelIzquierdo.Controls.Add(panelContenido);
            panelIzquierdo.Dock = DockStyle.Fill;
            panelIzquierdo.Location = new Point(10, 10);
            panelIzquierdo.Name = "panelIzquierdo";
            panelIzquierdo.Padding = new Padding(0);
            panelIzquierdo.Size = new Size(560, 524);
            panelIzquierdo.TabIndex = 0;

            // 
            // panelContenido
            // 
            panelContenido.BackColor = Color.FromArgb(240, 242, 245);
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.Location = new Point(0, 0);
            panelContenido.Name = "panelContenido";
            panelContenido.Size = new Size(560, 524);
            panelContenido.TabIndex = 0;

            // 
            // espaciadorDerecho
            // 
            espaciadorDerecho.BackColor = Color.Transparent;
            espaciadorDerecho.Dock = DockStyle.Top;
            espaciadorDerecho.Location = new Point(10, 110);
            espaciadorDerecho.Name = "espaciadorDerecho";
            espaciadorDerecho.Size = new Size(414, 10);
            espaciadorDerecho.TabIndex = 2;
            
            // 
            // panelDerecho
            // 
            panelDerecho.BackColor = Color.Transparent;
            panelDerecho.Controls.Add(panelLogs);
            panelDerecho.Controls.Add(espaciadorDerecho);
            panelDerecho.Controls.Add(panelSuperiorDerecho);
            panelDerecho.Dock = DockStyle.Right;
            panelDerecho.Location = new Point(580, 10);
            panelDerecho.Name = "panelDerecho";
            panelDerecho.Padding = new Padding(10, 0, 0, 0);
            panelDerecho.Size = new Size(424, 524);
            panelDerecho.TabIndex = 1;

            // 
            // panelSuperiorDerecho
            // 
            panelSuperiorDerecho.BackColor = Color.FromArgb(240, 242, 245);
            panelSuperiorDerecho.BorderColor = Color.FromArgb(240, 242, 245);
            panelSuperiorDerecho.BorderRadius = 0;
            panelSuperiorDerecho.BorderSize = 0;
            panelSuperiorDerecho.Controls.Add(cmbDispositivoCOM);
            panelSuperiorDerecho.Controls.Add(lblCOM);
            panelSuperiorDerecho.Controls.Add(cmbDispositivoADB);
            panelSuperiorDerecho.Controls.Add(lblADB);
            panelSuperiorDerecho.Dock = DockStyle.Top;
            panelSuperiorDerecho.Location = new Point(10, 0);
            panelSuperiorDerecho.Name = "panelSuperiorDerecho";
            panelSuperiorDerecho.Padding = new Padding(0, 0, 0, 0);
            panelSuperiorDerecho.Size = new Size(414, 110);
            panelSuperiorDerecho.TabIndex = 0;

            // 
            // picLogo
            // 
            picLogo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            picLogo.BackColor = Color.Transparent;
            picLogo.Location = new Point(840, -5);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(260, 90);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 10;
            picLogo.TabStop = false;
            try
            {
                string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "TT-TOOLNEWLOGO.png");
                if (File.Exists(logoPath))
                {
                    picLogo.Image = Image.FromFile(logoPath);
                }
            }
            catch { }

            // 
            // lblADB
            // 
            lblADB.AutoSize = false;
            lblADB.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblADB.ForeColor = Color.FromArgb(100, 105, 115);
            lblADB.Location = new Point(0, 0);
            lblADB.Name = "lblADB";
            lblADB.Size = new Size(310, 12);
            lblADB.TabIndex = 1;
            lblADB.Text = "🔌 USB PORT";

            // 
            // cmbDispositivoADB
            // 
            cmbDispositivoADB.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbDispositivoADB.BackColor = Color.FromArgb(248, 249, 250);
            cmbDispositivoADB.BorderColor = Color.FromArgb(225, 228, 232);
            cmbDispositivoADB.BorderFocusColor = Color.FromArgb(66, 133, 244);
            cmbDispositivoADB.BorderRadius = 5;
            cmbDispositivoADB.BorderSize = 1;
            cmbDispositivoADB.ButtonColor = Color.FromArgb(66, 133, 244);
            cmbDispositivoADB.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDispositivoADB.Font = new Font("Segoe UI", 8F);
            cmbDispositivoADB.ForeColor = Color.FromArgb(60, 64, 72);
            cmbDispositivoADB.Location = new Point(0, 15);
            cmbDispositivoADB.Name = "cmbDispositivoADB";
            cmbDispositivoADB.Size = new Size(414, 26);
            cmbDispositivoADB.TabIndex = 2;
            cmbDispositivoADB.DropDown += CmbDispositivoADB_DropDown;
            cmbDispositivoADB.SelectedIndexChanged += CmbDispositivoADB_SelectedIndexChanged;

            // 
            // lblCOM
            // 
            lblCOM.AutoSize = false;
            lblCOM.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblCOM.ForeColor = Color.FromArgb(100, 105, 115);
            lblCOM.Location = new Point(0, 50);
            lblCOM.Name = "lblCOM";
            lblCOM.Size = new Size(310, 12);
            lblCOM.TabIndex = 3;
            lblCOM.Text = "⚡ COM PORT";

            // 
            // cmbDispositivoCOM
            // 
            cmbDispositivoCOM.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbDispositivoCOM.BackColor = Color.FromArgb(248, 249, 250);
            cmbDispositivoCOM.BorderColor = Color.FromArgb(225, 228, 232);
            cmbDispositivoCOM.BorderFocusColor = Color.FromArgb(66, 133, 244);
            cmbDispositivoCOM.BorderRadius = 5;
            cmbDispositivoCOM.BorderSize = 1;
            cmbDispositivoCOM.ButtonColor = Color.FromArgb(66, 133, 244);
            cmbDispositivoCOM.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDispositivoCOM.Font = new Font("Segoe UI", 8F);
            cmbDispositivoCOM.ForeColor = Color.FromArgb(60, 64, 72);
            cmbDispositivoCOM.Location = new Point(0, 65);
            cmbDispositivoCOM.Name = "cmbDispositivoCOM";
            cmbDispositivoCOM.Size = new Size(414, 26);
            cmbDispositivoCOM.TabIndex = 4;
            cmbDispositivoCOM.DropDown += CmbDispositivoCOM_DropDown;
            cmbDispositivoCOM.SelectedIndexChanged += CmbDispositivoCOM_SelectedIndexChanged;

            // 
            // panelLogs
            // 
            panelLogs.BackColor = Color.FromArgb(240, 242, 245);
            panelLogs.BorderColor = Color.FromArgb(240, 242, 245);
            panelLogs.BorderRadius = 0;
            panelLogs.BorderSize = 0;
            panelLogs.Controls.Add(txtLogs);
            panelLogs.Dock = DockStyle.Fill;
            panelLogs.Location = new Point(10, 120);
            panelLogs.Name = "panelLogs";
            panelLogs.Padding = new Padding(0);
            panelLogs.Size = new Size(414, 404);
            panelLogs.TabIndex = 1;

            // 
            // txtLogs
            // 
            txtLogs.BackColor = Color.FromArgb(240, 242, 245);
            txtLogs.BorderStyle = BorderStyle.None;
            txtLogs.Dock = DockStyle.Fill;
            txtLogs.Font = new Font("Consolas", 9F, FontStyle.Bold);
            txtLogs.ForeColor = Color.FromArgb(60, 64, 72);
            txtLogs.Location = new Point(0, 0);
            txtLogs.Name = "txtLogs";
            txtLogs.ReadOnly = true;
            txtLogs.Size = new Size(414, 404);
            txtLogs.TabIndex = 0;
            txtLogs.Text = "";

            // 
            // panelFooter
            // 
            panelFooter.BackColor = Color.FromArgb(240, 242, 245);
            panelFooter.BorderColor = Color.FromArgb(240, 242, 245);
            panelFooter.BorderRadius = 0;
            panelFooter.BorderSize = 0;
            panelFooter.Controls.Add(btnCancelar);
            panelFooter.Controls.Add(progressBar);
            panelFooter.Controls.Add(lblEstadoProgreso);
            panelFooter.Dock = DockStyle.Bottom;
            panelFooter.Location = new Point(0, 600);
            panelFooter.Name = "panelFooter";
            panelFooter.Padding = new Padding(15, 8, 15, 8);
            panelFooter.Size = new Size(1100, 50);
            panelFooter.TabIndex = 3;

            // 
            // lblEstadoProgreso
            // 
            lblEstadoProgreso.AutoSize = true;
            lblEstadoProgreso.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            lblEstadoProgreso.ForeColor = Color.FromArgb(100, 105, 115);
            lblEstadoProgreso.Location = new Point(15, 16);
            lblEstadoProgreso.Name = "lblEstadoProgreso";
            lblEstadoProgreso.Size = new Size(40, 15);
            lblEstadoProgreso.TabIndex = 0;
            lblEstadoProgreso.Text = "Listo";

            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(200, 13);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(780, 24);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 1;
            progressBar.Visible = false;

            // 
            // btnCancelar
            // 
            btnCancelar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancelar.BackColor = Color.FromArgb(244, 67, 54);
            btnCancelar.BorderRadius = 5;
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btnCancelar.ForeColor = Color.White;
            btnCancelar.HoverColor = Color.FromArgb(211, 47, 47);
            btnCancelar.Location = new Point(990, 10);
            btnCancelar.Name = "btnCancelar";
            btnCancelar.Size = new Size(95, 30);
            btnCancelar.TabIndex = 2;
            btnCancelar.Text = "⏹ Cancelar";
            btnCancelar.UseVisualStyleBackColor = false;
            btnCancelar.Visible = false;
            btnCancelar.Click += BtnCancelar_Click;

            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(240, 242, 245);
            ClientSize = new Size(1024, 600);
            Controls.Add(panelPrincipal);
            Controls.Add(panelFooter);
            Controls.Add(lineaDivisoria);
            Controls.Add(panelSuperior);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimumSize = new Size(1040, 639);
            MaximumSize = new Size(1040, 639);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AREPA-TOOL By LeoPE-GSM.COM";
            Load += Form1_Load;
            panelSuperior.ResumeLayout(false);
            panelPrincipal.ResumeLayout(false);
            panelIzquierdo.ResumeLayout(false);
            panelDerecho.ResumeLayout(false);
            panelSuperiorDerecho.ResumeLayout(false);
            panelSuperiorDerecho.PerformLayout();
            panelLogs.ResumeLayout(false);
            panelFooter.ResumeLayout(false);
            panelFooter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelSuperior;
        private ModernButton btnAndroid;
        private ModernButton btnSamsung;
        private ModernButton btnHonor;
        private ModernButton btnMotorola;
        private ModernButton btnQualcomm;

        private Panel panelPrincipal;
        private ModernPanel panelIzquierdo;
        private Panel panelContenido;

        private Panel panelDerecho;
        private ModernPanel panelSuperiorDerecho;
        private PictureBox picLogo;
        private Label lblADB;
        private ModernComboBox cmbDispositivoADB;
        private Label lblCOM;
        private ModernComboBox cmbDispositivoCOM;

        private ModernPanel panelLogs;
        private RichTextBox txtLogs;

        private ModernPanel panelFooter;
        private Label lblEstadoProgreso;
        private ProgressBar progressBar;
        private ModernButton btnCancelar;
    }
}
