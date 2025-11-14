using TT_Tool.Controls;
using TT_Tool.Managers;
using System.Drawing;
using System.Net.Http;

namespace TT_Tool.Brands
{
    public class AndroidOperations : IBrandOperations
    {
        private ADBManager? _adbManager;
        private LogManager? _logManager;
        private ListView? _listViewApps;

        public string BrandName => "Android";

        public void Initialize()
        {
            // Inicialización específica de Android
        }

        public void SetLogManager(LogManager logManager)
        {
            _logManager = logManager;
            _adbManager = new ADBManager();
            _adbManager.OnLogMessage += (s, msg) => _logManager?.AgregarLog(msg);
        }

        public Panel GetOperationsPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(0)
            };

            // Crear TabControl
            var tabControl = new ModernTabControl
            {
                Dock = DockStyle.Fill
            };

            // Pestaña 1: Main Service
            var tabMainService = new TabPage("MAIN SERVICE");
            tabMainService.BackColor = Color.FromArgb(240, 242, 245);
            tabMainService.Controls.Add(CrearPanelMainService());
            tabControl.TabPages.Add(tabMainService);

            // Pestaña 2: App Manager
            var tabAppManager = new TabPage("APP MANAGER");
            tabAppManager.BackColor = Color.FromArgb(240, 242, 245);
            tabAppManager.Controls.Add(CrearPanelAppManager());
            tabControl.TabPages.Add(tabAppManager);

            // Pestaña 3: PM (ROOT)
            var tabPMRoot = new TabPage("PM [ ROOT ]");
            tabPMRoot.BackColor = Color.FromArgb(240, 242, 245);
            tabPMRoot.Controls.Add(CrearPanelPMRoot());
            tabControl.TabPages.Add(tabPMRoot);

            panel.Controls.Add(tabControl);
            return panel;
        }

        private RadioButton? _rbReadInfo, _rbRebootRecovery, _rbRebootFastboot, _rbRebootDownload, _rbBatteryInfo;
        private RadioButton? _rbDisableUpdates, _rbEnableDiag, _rbEraseFRP;

        private Panel CrearPanelMainService()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(240, 242, 245)
            };

            // GroupBox para agrupar los radio buttons
            var groupBox = new GroupBox
            {
                Location = new Point(10, 10),
                Size = new Size(520, 200),
                BackColor = Color.FromArgb(240, 242, 245),
                FlatStyle = FlatStyle.Flat,
                Text = ""
            };

            int yPos = 15;
            int leftColumnX = 10;
            int rightColumnX = 270;
            int radioHeight = 35;

            // Columna izquierda
            _rbReadInfo = CrearRadioButtonSimple("[ ADB ] READ INFORMATION", leftColumnX, yPos);
            groupBox.Controls.Add(_rbReadInfo);
            yPos += radioHeight;

            _rbRebootRecovery = CrearRadioButtonSimple("[ ADB ] REBOOT RECOVERY", leftColumnX, yPos);
            groupBox.Controls.Add(_rbRebootRecovery);
            yPos += radioHeight;

            _rbRebootFastboot = CrearRadioButtonSimple("[ ADB ] REBOOT FASTBOOT", leftColumnX, yPos);
            groupBox.Controls.Add(_rbRebootFastboot);
            yPos += radioHeight;

            _rbRebootDownload = CrearRadioButtonSimple("[ ADB ] REBOOT DOWNLOAD", leftColumnX, yPos);
            groupBox.Controls.Add(_rbRebootDownload);
            yPos += radioHeight;

            _rbBatteryInfo = CrearRadioButtonSimple("[ ADB ] BATTERY INFORMATION", leftColumnX, yPos);
            groupBox.Controls.Add(_rbBatteryInfo);

            // Columna derecha
            yPos = 15;
            _rbDisableUpdates = CrearRadioButtonSimple("[ ADB ] DISABLE UPDATES", rightColumnX, yPos);
            groupBox.Controls.Add(_rbDisableUpdates);
            yPos += radioHeight;

            _rbEnableDiag = CrearRadioButtonSimple("[ ADB ] ENABLE DIAG", rightColumnX, yPos);
            groupBox.Controls.Add(_rbEnableDiag);
            yPos += radioHeight;

            _rbEraseFRP = CrearRadioButtonSimple("[ ADB ] ERASE FRP GENERIC", rightColumnX, yPos);
            groupBox.Controls.Add(_rbEraseFRP);
            _rbEraseFRP.Checked = true; // Seleccionado por defecto

            panel.Controls.Add(groupBox);

            // Botón Terminal Start Process
            yPos = 220;
            var btnTerminal = new ModernButton
            {
                Text = "🖥️  [ ADB ] TERMINAL START PROCESS",
                Location = new Point(10, yPos),
                Size = new Size(510, 40),
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 6,
                HoverColor = Color.FromArgb(235, 238, 242),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };
            btnTerminal.FlatAppearance.BorderColor = Color.FromArgb(225, 228, 232);
            btnTerminal.FlatAppearance.BorderSize = 1;
            btnTerminal.Click += async (s, e) => await EjecutarOperacionSeleccionadaSimple();
            panel.Controls.Add(btnTerminal);

            // Botón Live Screen
            yPos += 50;
            var btnLiveScreen = new ModernButton
            {
                Text = "📡  [ ADB ] LIVE SCREEN",
                Location = new Point(10, yPos),
                Size = new Size(510, 40),
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 6,
                HoverColor = Color.FromArgb(235, 238, 242),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0)
            };
            btnLiveScreen.FlatAppearance.BorderColor = Color.FromArgb(225, 228, 232);
            btnLiveScreen.FlatAppearance.BorderSize = 1;
            btnLiveScreen.Click += (s, e) => IniciarScrcpy();
            panel.Controls.Add(btnLiveScreen);

            return panel;
        }

        private RadioButton CrearRadioButtonSimple(string texto, int x, int y)
        {
            return new RadioButton
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = Color.FromArgb(70, 75, 82),
                BackColor = Color.FromArgb(240, 242, 245),
                FlatStyle = FlatStyle.Flat,
                AutoSize = false
            };
        }

        private async Task EjecutarOperacionSeleccionadaSimple()
        {
            _logManager?.LimpiarLogs();

            RadioButton? seleccionado = null;
            
            if (_rbReadInfo?.Checked == true) seleccionado = _rbReadInfo;
            else if (_rbRebootRecovery?.Checked == true) seleccionado = _rbRebootRecovery;
            else if (_rbRebootFastboot?.Checked == true) seleccionado = _rbRebootFastboot;
            else if (_rbRebootDownload?.Checked == true) seleccionado = _rbRebootDownload;
            else if (_rbBatteryInfo?.Checked == true) seleccionado = _rbBatteryInfo;
            else if (_rbDisableUpdates?.Checked == true) seleccionado = _rbDisableUpdates;
            else if (_rbEnableDiag?.Checked == true) seleccionado = _rbEnableDiag;
            else if (_rbEraseFRP?.Checked == true) seleccionado = _rbEraseFRP;

            if (seleccionado == null)
            {
                _logManager?.AgregarLog("No operation selected", TipoLog.Advertencia);
                return;
            }

            if (_adbManager == null)
            {
                _logManager?.AgregarLog("ADB Manager not initialized", TipoLog.Error);
                return;
            }

            _logManager?.AgregarLog($"Executing: {seleccionado.Text}", TipoLog.Info);
            _logManager?.AgregarLog("");

            try
            {
                if (seleccionado.Text.Contains("READ INFORMATION"))
                {
                    await LeerInformacionDispositivo();
                }
                else if (seleccionado.Text.Contains("REBOOT RECOVERY"))
                {
                    await ReiniciarRecovery();
                }
                else if (seleccionado.Text.Contains("REBOOT FASTBOOT"))
                {
                    await ReiniciarFastboot();
                }
                else if (seleccionado.Text.Contains("REBOOT DOWNLOAD"))
                {
                    await ReiniciarDownload();
                }
                else if (seleccionado.Text.Contains("BATTERY INFORMATION"))
                {
                    await LeerInformacionBateria();
                }
                else if (seleccionado.Text.Contains("DISABLE UPDATES"))
                {
                    await DeshabilitarActualizaciones();
                }
                else if (seleccionado.Text.Contains("BYPASS MICLOUD"))
                {
                    
                }
                else if (seleccionado.Text.Contains("HIDE DEVELOPER"))
                {
                    
                }
                else if (seleccionado.Text.Contains("ENABLE DIAG"))
                {
                    await HabilitarDiag();
                }
                else if (seleccionado.Text.Contains("ERASE FRP"))
                {
                    await BorrarFRPGenerico();
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private Panel CrearPanelAppManager()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(0)
            };

            // Panel superior para búsqueda
            var searchPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(0, 8, 0, 8)
            };

            var txtSearch = new TextBox
            {
                Location = new Point(0, 8),
                Width = searchPanel.Width - 200,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(120, 120, 120),
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "🔍  Search by package name or app name..."
            };
            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == "🔍  Search by package name or app name...")
                {
                    txtSearch.Text = "🔍  ";
                    txtSearch.SelectionStart = txtSearch.Text.Length;
                    txtSearch.ForeColor = Color.FromArgb(50, 50, 50);
                }
            };
            txtSearch.LostFocus += (s, e) =>
            {
                if (txtSearch.Text == "🔍  " || string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "🔍  Search by package name or app name...";
                    txtSearch.ForeColor = Color.FromArgb(120, 120, 120);
                }
            };
            txtSearch.TextChanged += (s, e) =>
            {
                var searchText = txtSearch.Text.Replace("🔍  ", "").Trim();
                FiltrarAplicaciones(searchText);
            };
            searchPanel.Controls.Add(txtSearch);

            panel.Controls.Add(searchPanel);

            // ListView para las aplicaciones
            _listViewApps = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                BackColor = Color.FromArgb(240, 242, 245),
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0),
                OwnerDraw = true
            };

            // Configurar columnas con nombres
            _listViewApps.Columns.Add("", 40);
            _listViewApps.Columns.Add("APP NAME", 180);
            _listViewApps.Columns.Add("PACKAGE NAME", 280);

            // Dibujar encabezados de columnas
            _listViewApps.DrawColumnHeader += (s, e) =>
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(240, 242, 245)), e.Bounds);
                TextRenderer.DrawText(e.Graphics, e.Header.Text,
                    new Font("Segoe UI", 8F, FontStyle.Bold),
                    e.Bounds,
                    Color.FromArgb(70, 75, 82),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            };

            // Dibujar items
            _listViewApps.DrawItem += (s, e) =>
            {
                e.DrawBackground();
                e.DrawFocusRectangle();
            };

            _listViewApps.DrawSubItem += (s, e) =>
            {
                if (e.Item.Selected)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(225, 235, 255)), e.Bounds);
                }
                else
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(240, 242, 245)), e.Bounds);
                }

                TextRenderer.DrawText(e.Graphics, e.SubItem.Text,
                    new Font("Segoe UI", 8F, FontStyle.Regular),
                    e.Bounds,
                    Color.FromArgb(50, 50, 50),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            };

            // Agregar ImageList para los iconos
            _listViewApps.SmallImageList = new ImageList
            {
                ImageSize = new Size(32, 32),
                ColorDepth = ColorDepth.Depth32Bit
            };

            // Crear menú contextual (clic derecho)
            var contextMenu = new ContextMenuStrip();
            
            var menuSave = new ToolStripMenuItem("💾 Save APK");
            menuSave.Click += async (s, e) => await GuardarAPK();
            contextMenu.Items.Add(menuSave);

            contextMenu.Items.Add(new ToolStripSeparator());

            var menuUninstall = new ToolStripMenuItem("🗑️ Uninstall");
            menuUninstall.Click += async (s, e) => await DesinstalarApp();
            contextMenu.Items.Add(menuUninstall);

            var menuDisable = new ToolStripMenuItem("🚫 Disable");
            menuDisable.Click += async (s, e) => await DeshabilitarApp();
            contextMenu.Items.Add(menuDisable);

            var menuEnable = new ToolStripMenuItem("✅ Enable");
            menuEnable.Click += async (s, e) => await HabilitarApp();
            contextMenu.Items.Add(menuEnable);

            _listViewApps.ContextMenuStrip = contextMenu;

            panel.Controls.Add(_listViewApps);

            // Panel de botones inferior
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(252, 253, 254),
                Padding = new Padding(10, 8, 10, 8)
            };

            // Botón: Load Apps
            var btnLoadApps = new ModernButton
            {
                Text = "📥 LOAD APPS",
                Location = new Point(0, 8),
                Size = new Size(155, 34),
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 6,
                HoverColor = Color.FromArgb(235, 238, 242),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnLoadApps.FlatAppearance.BorderColor = Color.FromArgb(225, 228, 232);
            btnLoadApps.FlatAppearance.BorderSize = 1;
            btnLoadApps.Click += async (s, e) => await CargarAplicaciones();
            buttonPanel.Controls.Add(btnLoadApps);

            // Botón: Get Activity
            var btnGetActivity = new ModernButton
            {
                Text = "🔍 GET ACTIVITY",
                Location = new Point(165, 8),
                Size = new Size(155, 34),
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 6,
                HoverColor = Color.FromArgb(235, 238, 242),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnGetActivity.FlatAppearance.BorderColor = Color.FromArgb(225, 228, 232);
            btnGetActivity.FlatAppearance.BorderSize = 1;
            btnGetActivity.Click += async (s, e) => await ObtenerActivity();
            buttonPanel.Controls.Add(btnGetActivity);

            // Botón: Remove Virus
            var btnRemoveVirus = new ModernButton
            {
                Text = "🦠 REMOVE VIRUS",
                Location = new Point(330, 8),
                Size = new Size(155, 34),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                BorderRadius = 6,
                HoverColor = Color.FromArgb(192, 57, 43),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnRemoveVirus.FlatAppearance.BorderSize = 0;
            btnRemoveVirus.Click += async (s, e) => await RemoverVirus();
            buttonPanel.Controls.Add(btnRemoveVirus);

            panel.Controls.Add(buttonPanel);

            return panel;
        }

        private ListView? _listViewPartitions;
        private List<PartitionInfoData> _todasLasParticiones = new List<PartitionInfoData>();

        private Panel CrearPanelPMRoot()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(0)
            };

            // ListView para las particiones
            _listViewPartitions = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                BackColor = Color.FromArgb(240, 242, 245),
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0),
                CheckBoxes = true
            };

            // Configurar columnas
            _listViewPartitions.Columns.Add("✓", 35);
            _listViewPartitions.Columns.Add("Partition Name", 120);
            _listViewPartitions.Columns.Add("Address", 140);
            _listViewPartitions.Columns.Add("Filename", 200);

            // Personalizar colores sin OwnerDraw para que los checkboxes funcionen
            _listViewPartitions.BackColor = Color.White;
            _listViewPartitions.ForeColor = Color.FromArgb(50, 50, 50);

            // Evento de doble clic para seleccionar archivo
            _listViewPartitions.DoubleClick += (s, e) =>
            {
                if (_listViewPartitions.SelectedItems.Count > 0)
                {
                    var item = _listViewPartitions.SelectedItems[0];
                    SeleccionarArchivoParaParticion(item);
                }
            };

            panel.Controls.Add(_listViewPartitions);

            // Panel de botones inferior
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(0, 8, 0, 8)
            };

            // Botón: List Partitions
            var btnListPartitions = new ModernButton
            {
                Text = "📋 LIST PARTITIONS",
                Location = new Point(0, 8),
                Size = new Size(155, 34),
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 6,
                HoverColor = Color.FromArgb(235, 238, 242),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnListPartitions.FlatAppearance.BorderColor = Color.FromArgb(225, 228, 232);
            btnListPartitions.FlatAppearance.BorderSize = 1;
            btnListPartitions.Click += async (s, e) => await ListarParticiones();
            buttonPanel.Controls.Add(btnListPartitions);

            // Botón: Read Partition
            var btnReadPartition = new ModernButton
            {
                Text = "📥 READ PARTITION",
                Location = new Point(165, 8),
                Size = new Size(155, 34),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                BorderRadius = 6,
                HoverColor = Color.FromArgb(39, 174, 96),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnReadPartition.FlatAppearance.BorderSize = 0;
            btnReadPartition.Click += async (s, e) => await LeerParticion();
            buttonPanel.Controls.Add(btnReadPartition);

            // Botón: Write Partition
            var btnWritePartition = new ModernButton
            {
                Text = "📤 WRITE PARTITION",
                Location = new Point(330, 8),
                Size = new Size(155, 34),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                BorderRadius = 6,
                HoverColor = Color.FromArgb(41, 128, 185),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnWritePartition.FlatAppearance.BorderSize = 0;
            btnWritePartition.Click += async (s, e) => await EscribirParticion();
            buttonPanel.Controls.Add(btnWritePartition);

            panel.Controls.Add(buttonPanel);

            return panel;
        }


        // Métodos de funcionalidad

        private List<AppInfo> _todasLasApps = new List<AppInfo>();

        private Image? CrearIconoPorDefecto()
        {
            try
            {
                // Crear un icono simple de Android (círculo verde con "A")
                var bitmap = new Bitmap(32, 32);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    
                    // Fondo verde Android
                    using (var brush = new SolidBrush(Color.FromArgb(164, 198, 57)))
                    {
                        g.FillEllipse(brush, 2, 2, 28, 28);
                    }
                    
                    // Letra "A" blanca
                    using (var font = new Font("Segoe UI", 14F, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.White))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString("A", font, brush, new RectangleF(0, 0, 32, 32), sf);
                    }
                }
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private async Task CargarAplicaciones()
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("Loading applications...", TipoLog.Info);
            _logManager?.AgregarLog("");

            try
            {
                if (_adbManager == null || _listViewApps == null)
                {
                    _logManager?.AgregarLog("Error: ADB Manager not initialized", TipoLog.Error);
                    return;
                }

                _logManager?.AgregarLog("Step 1: Loading all applications...", TipoLog.Info);

                // Obtener TODAS las apps (sistema + usuario)
                var allPackagesResult = await _adbManager.ExecuteAdbCommandAsync("shell pm list packages");
                var allPackages = allPackagesResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(p => p.StartsWith("package:"))
                    .Select(p => p.Replace("package:", "").Trim())
                    .ToList();

                // Obtener solo apps de usuario
                var userPackagesResult = await _adbManager.ExecuteAdbCommandAsync("shell pm list packages -3");
                var userPackages = userPackagesResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(p => p.StartsWith("package:"))
                    .Select(p => p.Replace("package:", "").Trim())
                    .ToHashSet();

                _logManager?.AgregarLog($"Found {allPackages.Count} total packages ({userPackages.Count} user apps, {allPackages.Count - userPackages.Count} system apps)", TipoLog.Info);

                _listViewApps.Items.Clear();
                _listViewApps.SmallImageList?.Images.Clear();
                _todasLasApps.Clear();

                // Agregar grupo "User Apps"
                var userGroup = new ListViewGroup("📱 User Apps", HorizontalAlignment.Left);
                _listViewApps.Groups.Add(userGroup);

                // Agregar grupo "System Apps"
                var systemGroup = new ListViewGroup("⚙️ System Apps", HorizontalAlignment.Left);
                _listViewApps.Groups.Add(systemGroup);

                // Cargar todas las apps
                foreach (var packageName in allPackages.OrderBy(p => p))
                {
                    bool isUserApp = userPackages.Contains(packageName);
                    
                    // Usar el último segmento del package name como nombre de app
                    string appName = packageName.Split('.').LastOrDefault() ?? packageName;
                    
                    // Capitalizar primera letra
                    if (!string.IsNullOrEmpty(appName))
                    {
                        appName = char.ToUpper(appName[0]) + appName.Substring(1);
                    }

                    var appInfo = new AppInfo
                    {
                        PackageName = packageName,
                        AppName = appName,
                        IsSystemApp = !isUserApp
                    };

                    _todasLasApps.Add(appInfo);

                    // Agregar a ListView
                    var item = new ListViewItem();
                    item.ImageIndex = 0;
                    item.SubItems.Add(appName);
                    item.SubItems.Add(packageName);
                    item.Tag = appInfo;
                    item.Group = isUserApp ? userGroup : systemGroup;

                    _listViewApps.Items.Add(item);
                }

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Loaded {_todasLasApps.Count} applications successfully!", TipoLog.Exito);
                _logManager?.AgregarLog($"  User Apps: {userPackages.Count}", TipoLog.Info);
                _logManager?.AgregarLog($"  System Apps: {allPackages.Count - userPackages.Count}", TipoLog.Info);

                // Paso 2: Cargar iconos por defecto
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("Step 2: Setting app icons...", TipoLog.Info);

                // Crear icono por defecto
                var defaultIcon = CrearIconoPorDefecto();
                if (defaultIcon != null)
                {
                    _listViewApps.SmallImageList?.Images.Add("default", defaultIcon);
                    
                    // Asignar icono por defecto a todas las apps
                    foreach (ListViewItem item in _listViewApps.Items)
                    {
                        item.ImageKey = "default";
                    }
                }

                _logManager?.AgregarLog("Default icons set", TipoLog.Exito);
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("Apps loaded and ready to use!", TipoLog.Exito);
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("Note: For custom app icons, use specialized tools like:", TipoLog.Info);
                _logManager?.AgregarLog("  - ADB App Control", TipoLog.Info);
                _logManager?.AgregarLog("  - Scrcpy with icon extraction", TipoLog.Info);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error loading applications:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FiltrarAplicaciones(string filtro)
        {
            if (_listViewApps == null || string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            _listViewApps.Items.Clear();

            var appsFiltradas = _todasLasApps.Where(app =>
                app.PackageName.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                app.AppName.Contains(filtro, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            foreach (var app in appsFiltradas)
            {
                var item = new ListViewItem();
                item.ImageKey = "default";
                item.SubItems.Add(app.AppName);
                item.SubItems.Add(app.PackageName);
                item.Tag = app;
                _listViewApps.Items.Add(item);
            }
        }

        private async Task ObtenerActivity()
        {
            if (_listViewApps == null || _listViewApps.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an application first", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("Getting main activity...", TipoLog.Info);
            _logManager?.AgregarLog("");

            try
            {
                var selectedItem = _listViewApps.SelectedItems[0];
                var appInfo = selectedItem.Tag as AppInfo;

                if (appInfo == null || _adbManager == null)
                {
                    return;
                }

                _logManager?.AgregarLog($"Package: {appInfo.PackageName}", TipoLog.Info);

                // Obtener la actividad principal
                var activityResult = await _adbManager.ExecuteAdbCommandAsync($"shell cmd package resolve-activity --brief {appInfo.PackageName}");
                
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("Main Activity:", TipoLog.Exito);
                _logManager?.AgregarLog(activityResult.Trim(), TipoLog.Info);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private async Task RemoverVirus()
        {
            var result = MessageBox.Show(
                "This will remove known virus/malware packages from your device.\n\n" +
                "Do you want to continue?",
                "Remove Virus",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("Removing virus/malware packages...", TipoLog.Info);
            _logManager?.AgregarLog("");

            if (_adbManager == null)
            {
                return;
            }

            // Lista de paquetes maliciosos
            string[] virusPackages = new string[]
            {
                "com.fido.asm",
                "com.wawrd.ycfbo",
                "game.mind.teaser",
                "com.catproxy.fast",
                "com.heytap.pictorial",
                "com.appxy.tinyscanner",
                "com.core.rate.heart.vita",
                "com.gsgames.pixel.combat",
                "com.inspiredsquare.blocks",
                "com.inspiredsquare.number",
                "uddy.file.manage.scan.acee",
                "com.wallpaper.hd.beauty.paper",
                "com.pixelpulse.wallpapers.app",
                "de.szalkowski.activitylauncher",
                "jp.co.goodroid.hyper.armwrestling",
                "com.splashtop.streamer.addon.knox",
                "merge.blocks.drop.number.puzzle.games",
                "com.antivirus.viruscleaner.mobilesecurity",
                "com.jewel.sliding.gemstone.tetra.block.puzzle",
                "mobi.infolife.ezweather.widget.weather.location"
            };

            int removed = 0;
            foreach (var package in virusPackages)
            {
                // Intentar desinstalar para user 0
                await _adbManager.ExecuteAdbCommandAsync($"shell pm uninstall -k --user 0 {package}");
                
                // Intentar desinstalar completamente
                await _adbManager.ExecuteAdbCommandAsync($"shell pm uninstall {package}");
                
                removed++;
            }

            _logManager?.AgregarLog($"Processed {removed} packages", TipoLog.Exito);
            _logManager?.AgregarLog("Removal completed!", TipoLog.Exito);

            MessageBox.Show(
                $"Virus removal completed!\n\nProcessed {removed} known malware packages.",
                "Completed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async Task GuardarAPK()
        {
            if (_listViewApps == null || _listViewApps.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an application first", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = _listViewApps.SelectedItems[0];
            var appInfo = selectedItem.Tag as AppInfo;

            if (appInfo == null || _adbManager == null)
            {
                return;
            }

            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog($"Saving APK: {appInfo.PackageName}", TipoLog.Info);
            _logManager?.AgregarLog("");

            try
            {
                // Obtener ruta del APK en el dispositivo
                var pathResult = await _adbManager.ExecuteAdbCommandAsync($"shell pm path {appInfo.PackageName}");
                var apkPath = pathResult.Replace("package:", "").Trim();

                if (string.IsNullOrEmpty(apkPath))
                {
                    _logManager?.AgregarLog("APK path not found", TipoLog.Error);
                    return;
                }

                _logManager?.AgregarLog($"APK path: {apkPath}", TipoLog.Info);

                // Seleccionar dónde guardar
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "APK Files (*.apk)|*.apk";
                    saveDialog.FileName = $"{appInfo.PackageName}.apk";
                    saveDialog.Title = "Save APK";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        _logManager?.AgregarLog($"Pulling APK...", TipoLog.Info);
                        await _adbManager.ExecuteAdbCommandAsync($"pull {apkPath} \"{saveDialog.FileName}\"");
                        
                        _logManager?.AgregarLog("");
                        _logManager?.AgregarLog($"APK saved to: {saveDialog.FileName}", TipoLog.Exito);

                        MessageBox.Show($"APK saved successfully!\n\n{saveDialog.FileName}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error saving APK:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DesinstalarApp()
        {
            if (_listViewApps == null || _listViewApps.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an application first", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = _listViewApps.SelectedItems[0];
            var appInfo = selectedItem.Tag as AppInfo;

            if (appInfo == null || _adbManager == null)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to uninstall:\n\n{appInfo.AppName}\n({appInfo.PackageName})?",
                "Confirm Uninstall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog($"Uninstalling: {appInfo.PackageName}", TipoLog.Info);
            _logManager?.AgregarLog("");

            try
            {
                // Intentar desinstalar
                var uninstallResult = await _adbManager.ExecuteAdbCommandAsync($"shell pm uninstall {appInfo.PackageName}");
                
                if (uninstallResult.Contains("Success"))
                {
                    _logManager?.AgregarLog("Uninstalled successfully!", TipoLog.Exito);
                    
                    // Remover de la lista
                    _listViewApps.Items.Remove(selectedItem);
                    _todasLasApps.Remove(appInfo);

                    MessageBox.Show($"Application uninstalled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _logManager?.AgregarLog($"Result: {uninstallResult.Trim()}", TipoLog.Advertencia);
                    MessageBox.Show($"Uninstall result:\n{uninstallResult}", "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error uninstalling:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DeshabilitarApp()
        {
            if (_listViewApps == null || _listViewApps.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an application first", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = _listViewApps.SelectedItems[0];
            var appInfo = selectedItem.Tag as AppInfo;

            if (appInfo == null || _adbManager == null)
            {
                return;
            }

            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog($"Disabling: {appInfo.PackageName}", TipoLog.Info);
            _logManager?.AgregarLog("");

            try
            {
                var disableResult = await _adbManager.ExecuteAdbCommandAsync($"shell pm disable-user {appInfo.PackageName}");
                
                _logManager?.AgregarLog($"Result: {disableResult.Trim()}", TipoLog.Info);
                _logManager?.AgregarLog("Application disabled!", TipoLog.Exito);

                MessageBox.Show($"Application disabled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error disabling:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task HabilitarApp()
        {
            if (_listViewApps == null || _listViewApps.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select an application first", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItem = _listViewApps.SelectedItems[0];
            var appInfo = selectedItem.Tag as AppInfo;

            if (appInfo == null || _adbManager == null)
            {
                return;
            }

            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog($"Enabling: {appInfo.PackageName}", TipoLog.Info);
            _logManager?.AgregarLog("");

            try
            {
                var enableResult = await _adbManager.ExecuteAdbCommandAsync($"shell pm enable {appInfo.PackageName}");
                
                _logManager?.AgregarLog($"Result: {enableResult.Trim()}", TipoLog.Info);
                _logManager?.AgregarLog("Application enabled!", TipoLog.Exito);

                MessageBox.Show($"Application enabled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error enabling:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Métodos para PM (ROOT) - Particiones

        private async Task ListarParticiones()
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("Listing partitions (requires ROOT)...", TipoLog.Info);
            _logManager?.AgregarLog("");

            try
            {
                if (_adbManager == null || _listViewPartitions == null)
                {
                    _logManager?.AgregarLog("Error: ADB Manager not initialized", TipoLog.Error);
                    return;
                }

                // Verificar root
                var rootCheck = await _adbManager.ExecuteAdbCommandAsync("shell su -c 'id'");
                if (!rootCheck.Contains("uid=0"))
                {
                    _logManager?.AgregarLog("Root access not available!", TipoLog.Error);
                    MessageBox.Show("This feature requires ROOT access!", "Root Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _logManager?.AgregarLog("Root access confirmed", TipoLog.Exito);
                _logManager?.AgregarLog("");

                // Listar particiones desde /dev/block/by-name
                var partitionsResult = await _adbManager.ExecuteAdbCommandAsync("shell su -c 'ls -l /dev/block/by-name'");
                
                _listViewPartitions.Items.Clear();
                _todasLasParticiones.Clear();

                var lines = partitionsResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var line in lines)
                {
                    if (line.Contains("->"))
                    {
                        // Formato: lrwxrwxrwx 1 root root 21 2024-01-01 00:00 boot -> /dev/block/sda21
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        
                        if (parts.Length >= 3)
                        {
                            var partitionName = parts[parts.Length - 3];
                            var address = parts[parts.Length - 1];

                            var partitionInfo = new PartitionInfoData
                            {
                                Name = partitionName,
                                Address = address,
                                Filename = "Double click to select file"
                            };

                            _todasLasParticiones.Add(partitionInfo);

                            var item = new ListViewItem();
                            item.Checked = false;
                            item.SubItems.Add(partitionName);
                            item.SubItems.Add(address);
                            item.SubItems.Add("Double click to select file");
                            item.Tag = partitionInfo;

                            _listViewPartitions.Items.Add(item);
                        }
                    }
                }

                _logManager?.AgregarLog($"Found {_todasLasParticiones.Count} partitions", TipoLog.Exito);
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("Double-click on a partition to select a file for writing", TipoLog.Info);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error listing partitions:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SeleccionarArchivoParaParticion(ListViewItem item)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "Image Files (*.img)|*.img|All Files (*.*)|*.*";
                openDialog.Title = "Select file for partition";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var partitionInfo = item.Tag as PartitionInfoData;
                    if (partitionInfo != null)
                    {
                        partitionInfo.Filename = openDialog.FileName;
                        item.SubItems[3].Text = Path.GetFileName(openDialog.FileName);
                        item.Checked = true;

                        _logManager?.AgregarLog($"File selected for {partitionInfo.Name}: {Path.GetFileName(openDialog.FileName)}", TipoLog.Info);
                    }
                }
            }
        }

        private async Task LeerParticion()
        {
            if (_listViewPartitions == null || _listViewPartitions.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one partition to read", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("=== READING PARTITIONS ===", TipoLog.Info);
            _logManager?.AgregarLog("");

            try
            {
                if (_adbManager == null) return;

                foreach (ListViewItem item in _listViewPartitions.CheckedItems)
                {
                    var partitionInfo = item.Tag as PartitionInfoData;
                    if (partitionInfo == null) continue;

                    _logManager?.AgregarLog($"Reading partition: {partitionInfo.Name}", TipoLog.Info);

                    // Seleccionar dónde guardar
                    using (SaveFileDialog saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "Image Files (*.img)|*.img|All Files (*.*)|*.*";
                        saveDialog.FileName = $"{partitionInfo.Name}.img";
                        saveDialog.Title = $"Save {partitionInfo.Name} partition";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            // Leer partición con dd
                            var tempFile = $"/sdcard/{partitionInfo.Name}.img";
                            
                            _logManager?.AgregarLog($"  Creating backup on device...", TipoLog.Info);
                            await _adbManager.ExecuteAdbCommandAsync($"shell su -c 'dd if={partitionInfo.Address} of={tempFile}'");
                            
                            _logManager?.AgregarLog($"  Pulling to PC...", TipoLog.Info);
                            await _adbManager.ExecuteAdbCommandAsync($"pull {tempFile} \"{saveDialog.FileName}\"");
                            
                            _logManager?.AgregarLog($"  Cleaning up...", TipoLog.Info);
                            await _adbManager.ExecuteAdbCommandAsync($"shell rm {tempFile}");
                            
                            _logManager?.AgregarLog($"✓ {partitionInfo.Name} saved to: {saveDialog.FileName}", TipoLog.Exito);
                            _logManager?.AgregarLog("");
                        }
                    }
                }

                _logManager?.AgregarLog("Read operation completed!", TipoLog.Exito);
                MessageBox.Show("Partitions read successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error reading partitions:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task EscribirParticion()
        {
            if (_listViewPartitions == null || _listViewPartitions.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one partition to write", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Verificar que todas las particiones seleccionadas tengan archivo
            foreach (ListViewItem item in _listViewPartitions.CheckedItems)
            {
                var partitionInfo = item.Tag as PartitionInfoData;
                if (partitionInfo == null || partitionInfo.Filename == "Double click to select file")
                {
                    MessageBox.Show($"Please select a file for partition: {partitionInfo?.Name}", "File Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            var result = MessageBox.Show(
                "⚠️ WARNING ⚠️\n\n" +
                "Writing to partitions can BRICK your device!\n" +
                "Make sure you have the correct files.\n\n" +
                "Do you want to continue?",
                "Confirm Write",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("=== WRITING PARTITIONS ===", TipoLog.Advertencia);
            _logManager?.AgregarLog("");

            try
            {
                if (_adbManager == null) return;

                foreach (ListViewItem item in _listViewPartitions.CheckedItems)
                {
                    var partitionInfo = item.Tag as PartitionInfoData;
                    if (partitionInfo == null) continue;

                    _logManager?.AgregarLog($"Writing partition: {partitionInfo.Name}", TipoLog.Info);
                    _logManager?.AgregarLog($"  File: {Path.GetFileName(partitionInfo.Filename)}", TipoLog.Info);

                    // Subir archivo al dispositivo
                    var tempFile = $"/sdcard/{Path.GetFileName(partitionInfo.Filename)}";
                    
                    _logManager?.AgregarLog($"  Uploading file...", TipoLog.Info);
                    await _adbManager.ExecuteAdbCommandAsync($"push \"{partitionInfo.Filename}\" {tempFile}");
                    
                    _logManager?.AgregarLog($"  Writing to partition...", TipoLog.Advertencia);
                    await _adbManager.ExecuteAdbCommandAsync($"shell su -c 'dd if={tempFile} of={partitionInfo.Address}'");
                    
                    _logManager?.AgregarLog($"  Cleaning up...", TipoLog.Info);
                    await _adbManager.ExecuteAdbCommandAsync($"shell rm {tempFile}");
                    
                    _logManager?.AgregarLog($"✓ {partitionInfo.Name} written successfully", TipoLog.Exito);
                    _logManager?.AgregarLog("");
                }

                _logManager?.AgregarLog("Write operation completed!", TipoLog.Exito);
                MessageBox.Show("Partitions written successfully!\n\nReboot your device to apply changes.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error writing partitions:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Métodos para Main Service Operations

        private async Task EjecutarOperacionSeleccionada(params ModernRadioButton[] radioButtons)
        {
            _logManager?.LimpiarLogs();

            var seleccionado = radioButtons.FirstOrDefault(rb => rb.Checked);
            if (seleccionado == null)
            {
                _logManager?.AgregarLog("No operation selected", TipoLog.Advertencia);
                return;
            }

            if (_adbManager == null)
            {
                _logManager?.AgregarLog("ADB Manager not initialized", TipoLog.Error);
                return;
            }

            _logManager?.AgregarLog($"Executing: {seleccionado.Text}", TipoLog.Info);
            _logManager?.AgregarLog("");

            try
            {
                if (seleccionado.Text.Contains("READ INFORMATION"))
                {
                    await LeerInformacionDispositivo();
                }
                else if (seleccionado.Text.Contains("REBOOT RECOVERY"))
                {
                    await ReiniciarRecovery();
                }
                else if (seleccionado.Text.Contains("REBOOT FASTBOOT"))
                {
                    await ReiniciarFastboot();
                }
                else if (seleccionado.Text.Contains("REBOOT DOWNLOAD"))
                {
                    await ReiniciarDownload();
                }
                else if (seleccionado.Text.Contains("BATTERY INFORMATION"))
                {
                    await LeerInformacionBateria();
                }
                else if (seleccionado.Text.Contains("DISABLE UPDATES"))
                {
                    await DeshabilitarActualizaciones();
                }
                else if (seleccionado.Text.Contains("BYPASS MICLOUD"))
                {
                    
                }
                else if (seleccionado.Text.Contains("HIDE DEVELOPER"))
                {
                    
                }
                else if (seleccionado.Text.Contains("ENABLE DIAG"))
                {
                    await HabilitarDiag();
                }
                else if (seleccionado.Text.Contains("ERASE FRP"))
                {
                    await BorrarFRPGenerico();
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private async Task LeerInformacionDispositivo()
        {
            if (_adbManager == null) return;

            _logManager?.AgregarLog("Reading device information...", TipoLog.Info);
            _logManager?.AgregarLog("");

            var comandos = new Dictionary<string, string>
            {
                { "Model", "shell getprop ro.product.model" },
                { "Carrier ID", "shell getprop ro.csc.country_code" },
                { "Sales Code", "shell getprop ro.csc.sales_code" },
                { "Country Code", "shell getprop ro.csc.country_code" },
                { "Timezone", "shell getprop persist.sys.timezone" },
                { "Android Version", "shell getprop ro.build.version.release" },
                { "SDK Version", "shell getprop ro.build.version.sdk" },
                { "Build Date", "shell getprop ro.build.date" },
                { "PDA Version", "shell getprop ro.build.version.incremental" },
                { "Baseband", "shell getprop gsm.version.baseband" },
                { "Board Platform", "shell getprop ro.board.platform" },
                { "Serial Number", "shell getprop ro.serialno" },
                { "Network Type", "shell getprop gsm.network.type" },
                { "SIM Status", "shell getprop gsm.sim.state" },
                { "Security Patch", "shell getprop ro.build.version.security_patch" },
                { "EM DID", "shell getprop ro.boot.em.did" }
            };

            foreach (var comando in comandos)
            {
                var resultado = await _adbManager.ExecuteAdbCommandAsync(comando.Value);
                _logManager?.AgregarLog($"{comando.Key}: {resultado.Trim()}", TipoLog.Info);
            }

            _logManager?.AgregarLog("");
            _logManager?.AgregarLog("Device information read successfully!", TipoLog.Exito);
        }

        private async Task ReiniciarRecovery()
        {
            if (_adbManager == null) return;

            _logManager?.AgregarLog("Rebooting to Recovery mode...", TipoLog.Info);
            await _adbManager.ExecuteAdbCommandAsync("reboot recovery");
            _logManager?.AgregarLog("Device rebooting to Recovery", TipoLog.Exito);
        }

        private async Task ReiniciarFastboot()
        {
            if (_adbManager == null) return;

            _logManager?.AgregarLog("Rebooting to Fastboot mode...", TipoLog.Info);
            await _adbManager.ExecuteAdbCommandAsync("reboot bootloader");
            _logManager?.AgregarLog("Device rebooting to Fastboot", TipoLog.Exito);
        }

        private async Task ReiniciarDownload()
        {
            if (_adbManager == null) return;

            _logManager?.AgregarLog("Rebooting to Download mode...", TipoLog.Info);
            await _adbManager.ExecuteAdbCommandAsync("reboot download");
            _logManager?.AgregarLog("Device rebooting to Download mode", TipoLog.Exito);
        }

        private async Task LeerInformacionBateria()
        {
            if (_adbManager == null) return;

            _logManager?.AgregarLog("Reading battery information...", TipoLog.Info);
            _logManager?.AgregarLog("");

            var resultado = await _adbManager.ExecuteAdbCommandAsync("shell dumpsys battery");
            _logManager?.AgregarLog(resultado, TipoLog.Info);
            _logManager?.AgregarLog("");
            _logManager?.AgregarLog("Battery information read successfully!", TipoLog.Exito);
        }

        private async Task DeshabilitarActualizaciones()
        {
            if (_adbManager == null) return;

            _logManager?.AgregarLog("Disabling system updates...", TipoLog.Info);
            await _adbManager.ExecuteAdbCommandAsync("shell pm disable-user --user 0 com.wssyncmldm");
            _logManager?.AgregarLog("System updates disabled", TipoLog.Exito);
        }



        private async Task HabilitarDiag()
        {
            if (_adbManager == null) return;

            _logManager?.AgregarLog("Enabling DIAG mode...", TipoLog.Info);
            await _adbManager.ExecuteAdbCommandAsync("shell setprop sys.usb.config diag,adb");
            _logManager?.AgregarLog("DIAG mode enabled", TipoLog.Exito);
        }

        private async Task BorrarFRPGenerico()
        {
            if (_adbManager == null) return;

            _logManager?.AgregarLog("=== ERASING FRP (GENERIC METHOD) ===", TipoLog.Advertencia);
            _logManager?.AgregarLog("");

            var comandos = new[]
            {
                ("Setting user setup complete", "shell content insert --uri content://settings/secure --bind name:s:user_setup_complete --bind value:s:1"),
                ("Removing Google account", "shell content delete --uri content://accounts/account --where \"name='com.google'\""),
                ("Broadcasting master clear", "shell am broadcast -a android.intent.action.MASTER_CLEAR"),
                ("Wiping data", "shell recovery --wipe_data"),
                ("Clearing settings provider", "shell pm clear com.android.providers.settings"),
                ("Clearing settings", "shell pm clear com.android.settings"),
                ("Setting user setup complete (secure)", "shell settings put secure user_setup_complete 1"),
                ("Setting device provisioned", "shell settings put global device_provisioned 1"),
                ("Setting setup wizard run", "shell settings put global setupwizard_has_run 1"),
                ("Starting home activity", "shell am start -a android.intent.action.MAIN -c android.intent.category.HOME")
            };

            foreach (var (descripcion, comando) in comandos)
            {
                _logManager?.AgregarLog($"{descripcion}...", TipoLog.Info);
                await _adbManager.ExecuteAdbCommandAsync(comando);
                await Task.Delay(500);
            }

            _logManager?.AgregarLog("");
            _logManager?.AgregarLog("Rebooting device...", TipoLog.Info);
            await _adbManager.ExecuteAdbCommandAsync("reboot");

            _logManager?.AgregarLog("");
            _logManager?.AgregarLog("FRP erase completed! Device is rebooting.", TipoLog.Exito);
        }

        private async void IniciarScrcpy()
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
            _logManager?.AgregarLog("      INITIALIZING LIVE SCREEN", TipoLog.Titulo);
            _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
            _logManager?.AgregarLog("");

            var startTime = DateTime.Now;

            await Task.Run(() =>
            {
                try
                {
                    // Buscar scrcpy en la carpeta Resources/Tools
                    string toolsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Tools");
                    string scrcpyPath = "";
                    string scrcpyDir = "";

                    // Buscar en subdirectorios
                    if (Directory.Exists(toolsPath))
                    {
                        var scrcpyFiles = Directory.GetFiles(toolsPath, "scrcpy.exe", SearchOption.AllDirectories);
                        if (scrcpyFiles.Length > 0)
                        {
                            scrcpyPath = scrcpyFiles[0];
                            scrcpyDir = Path.GetDirectoryName(scrcpyPath) ?? "";
                        }
                    }

                    if (string.IsNullOrEmpty(scrcpyPath) || !File.Exists(scrcpyPath))
                    {
                        _logManager?.AgregarLog("      ✗ scrcpy not found", TipoLog.Error);
                        _logManager?.AgregarLog("      Please extract scrcpy to Resources/Tools/", TipoLog.Advertencia);
                        return;
                    }

                    _logManager?.AgregarLog("Starting scrcpy process...", TipoLog.Proceso);

                    // Buscar el archivo scrcpy-noconsole.vbs
                    string vbsPath = Path.Combine(scrcpyDir, "scrcpy-noconsole.vbs");
                    
                    // Ejecutar con parámetros optimizados para estabilidad
                    var processInfo = new System.Diagnostics.ProcessStartInfo();
                    
                    if (File.Exists(vbsPath))
                    {
                        // Ejecutar wscript.exe directamente con el VBS
                        processInfo.FileName = "wscript.exe";
                        processInfo.Arguments = $"\"{vbsPath}\"";
                        processInfo.WorkingDirectory = scrcpyDir;
                        processInfo.UseShellExecute = true; // Proceso completamente independiente
                        processInfo.CreateNoWindow = true;
                    }
                    else
                    {
                        // Fallback: ejecutar scrcpy directamente con parámetros de estabilidad
                        processInfo.FileName = scrcpyPath;
                        // Agregar parámetros para mejor estabilidad
                        processInfo.Arguments = "--stay-awake --turn-screen-off=false";
                        processInfo.WorkingDirectory = scrcpyDir;
                        processInfo.UseShellExecute = true; // Proceso completamente independiente
                        processInfo.CreateNoWindow = false;
                    }

                    // Iniciar el proceso sin mantener referencia para que sea independiente
                    var proceso = System.Diagnostics.Process.Start(processInfo);
                    
                    if (proceso != null)
                    {
                        // No mantener referencia al proceso para que sea completamente independiente
                        proceso = null;
                    }
                    
                    var elapsed = (DateTime.Now - startTime).TotalSeconds;
                    _logManager?.AgregarLog("      ✓ OK", TipoLog.Exito);
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
                    _logManager?.AgregarLog($"⏱  Execution time: {elapsed:F2}s", TipoLog.Comando);
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog("✓✓✓ LIVE SCREEN STARTED ✓✓✓", TipoLog.Exito);
                    _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog("ℹ  Live Screen is now running independently", TipoLog.Info);
                    _logManager?.AgregarLog("ℹ  You can continue using other ADB operations", TipoLog.Info);
                }
                catch (Exception ex)
                {
                    var elapsed = (DateTime.Now - startTime).TotalSeconds;
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog($"⏱  Execution time: {elapsed:F2}s", TipoLog.Comando);
                    _logManager?.AgregarLog($"      ✗ Error: {ex.Message}", TipoLog.Error);
                }
            });
        }
    }

    // Clases auxiliares
    public class AppInfo
    {
        public string PackageName { get; set; } = "";
        public string AppName { get; set; } = "";
        public bool IsSystemApp { get; set; } = false;
    }

    public class PartitionInfoData
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string Filename { get; set; } = "";
    }
}
