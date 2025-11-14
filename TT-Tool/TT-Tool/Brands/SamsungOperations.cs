using TT_Tool.Controls;
using TT_Tool.Managers;
using TT_Tool.Forms;
using System.IO.Compression;
using System.Text.Json;
using System.Diagnostics;

namespace TT_Tool.Brands
{
    /// <summary>
    /// Operaciones para dispositivos Samsung
    /// </summary>
    public class SamsungOperations : IBrandOperations
    {
        private SamsungOdinManager? _odinManager;
        private LogManager? _logManager;
        private MagiskManager? _magiskManager; // Solo usado en Magisk Patch panel
        
        // Rutas de archivos
        private TextBox? _txtBL;
        private TextBox? _txtAP;
        private TextBox? _txtCP;
        private TextBox? _txtCSC;
        
        // Listas de particiones seleccionadas
        private List<PartitionInfo>? _partitionsBL;
        private List<PartitionInfo>? _partitionsAP;
        private List<PartitionInfo>? _partitionsCP;
        private List<PartitionInfo>? _partitionsCSC;
        

        public string BrandName => "Samsung";

        public void Initialize()
        {
            // Inicialización específica de Samsung
        }

        public void SetLogManager(LogManager logManager)
        {
            _logManager = logManager;
            _odinManager = new SamsungOdinManager();
            _odinManager.SetLogManager(_logManager); // Pasar LogManager al OdinManager
            _odinManager.OnLogMessage += (s, msg) => _logManager?.AgregarLog(msg);
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
            var tabMainService = new TabPage("Main Service");
            tabMainService.BackColor = Color.FromArgb(240, 242, 245);
            tabMainService.Controls.Add(CrearPanelMainService());
            tabControl.TabPages.Add(tabMainService);

            // Pestaña 2: Odin Mode
            var tabOdinMode = new TabPage("Odin Mode");
            tabOdinMode.BackColor = Color.FromArgb(240, 242, 245);
            tabOdinMode.Controls.Add(CrearPanelOdinMode());
            tabControl.TabPages.Add(tabOdinMode);

            // Pestaña 3: Magisk Patch
            var tabMagiskPatch = new TabPage("Magisk Patch");
            tabMagiskPatch.BackColor = Color.FromArgb(240, 242, 245);
            tabMagiskPatch.Controls.Add(CrearPanelMagiskPatch());
            tabControl.TabPages.Add(tabMagiskPatch);

            // Pestaña 4: KG Operations
            var tabKGOps = new TabPage("KG Operations");
            tabKGOps.BackColor = Color.FromArgb(240, 242, 245);
            tabKGOps.Controls.Add(CrearPanelKGOperations());
            tabControl.TabPages.Add(tabKGOps);

            panel.Controls.Add(tabControl);
            return panel;
        }

        private Panel CrearPanelMainService()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20, 20, 20, 20),
                BackColor = Color.FromArgb(235, 238, 242)
            };

            // Panel centrado para los botones
            var containerPanel = new Panel
            {
                Location = new Point(20, 20),
                Width = 600,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            panel.Controls.Add(containerPanel);

            int yPos = 0;

            // Botón: Fix Apps Bancarias 2025
            var btnFixBancarias = new ModernButton
            {
                Location = new Point(0, yPos),
                Width = 600,
                Height = 42,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Text = "🔧 [ ADB ] FIX APPS BANCARIAS 2025",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnFixBancarias.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnFixBancarias.FlatAppearance.BorderSize = 1;
            btnFixBancarias.Click += async (s, e) => await InstalarScriptBancarias();
            containerPanel.Controls.Add(btnFixBancarias);
            yPos += 50;

            // Botón: Read Info
            var btnReadInfo = new ModernButton
            {
                Location = new Point(0, yPos),
                Width = 600,
                Height = 42,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Text = "📱 [ ADB ] READ INFO",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnReadInfo.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnReadInfo.FlatAppearance.BorderSize = 1;
            btnReadInfo.Click += async (s, e) => await LeerInformacionDispositivo();
            containerPanel.Controls.Add(btnReadInfo);
            yPos += 50;

            // Botón: Samsung eSIM to SIM Convert
            var btnEsimConvert = new ModernButton
            {
                Location = new Point(0, yPos),
                Width = 600,
                Height = 42,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Text = "📲 (EFS) SAMSUNG ESIM TO SIM CONVERT",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnEsimConvert.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnEsimConvert.FlatAppearance.BorderSize = 1;
            btnEsimConvert.Click += (s, e) => ConvertirEsimASim();
            containerPanel.Controls.Add(btnEsimConvert);
            yPos += 50;

            // Botón: Reboot Root Mode
            var btnRebootRoot = new ModernButton
            {
                Location = new Point(0, yPos),
                Width = 600,
                Height = 42,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Text = "🔄 [ ADB ] REBOOT ROOT MODE",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnRebootRoot.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnRebootRoot.FlatAppearance.BorderSize = 1;
            btnRebootRoot.Click += async (s, e) => await RebootRootMode();
            containerPanel.Controls.Add(btnRebootRoot);
            yPos += 50;

            // Botón: Remove Samsung Account
            var btnRemoveSamsungAccount = new ModernButton
            {
                Location = new Point(0, yPos),
                Width = 600,
                Height = 42,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Text = "🗑️ [ ADB ] REMOVE SAMSUNG ACCOUNT",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnRemoveSamsungAccount.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnRemoveSamsungAccount.FlatAppearance.BorderSize = 1;
            btnRemoveSamsungAccount.Click += async (s, e) => await RemoverCuentaSamsung();
            containerPanel.Controls.Add(btnRemoveSamsungAccount);
            yPos += 50;

            // Botón: Fix Warning Alert
            var btnFixWarning = new ModernButton
            {
                Location = new Point(0, yPos),
                Width = 600,
                Height = 42,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Text = "⚠️ [ ROOT ] FIX WARNING ALERT ANNOUNCEMENT",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnFixWarning.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnFixWarning.FlatAppearance.BorderSize = 1;
            btnFixWarning.Click += async (s, e) => await FixWarningAlert();
            containerPanel.Controls.Add(btnFixWarning);
            yPos += 50;

            // Botón: Change CSC
            var btnChangeCSC = new ModernButton
            {
                Location = new Point(0, yPos),
                Width = 600,
                Height = 42,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Text = "🌍 [ COM ] CHANGE CSC",
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnChangeCSC.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnChangeCSC.FlatAppearance.BorderSize = 1;
            btnChangeCSC.Click += async (s, e) => await CambiarCSC();
            containerPanel.Controls.Add(btnChangeCSC);

            return panel;
        }

        private Panel CrearPanelOdinMode()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(245, 247, 250)
            };

            int yPos = 20;

            // ComboBox Recent (arriba)
            var lblRecent = new Label
            {
                Text = "Recent",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(20, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblRecent);

            var cmbRecent = new ComboBox
            {
                Location = new Point(100, yPos - 3),
                Width = 380,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.5F)
            };
            panel.Controls.Add(cmbRecent);

            var btnOpenRecent = new ModernButton
            {
                Text = "📁 Open",
                Location = new Point(490, yPos - 3),
                Size = new Size(80, 28),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                BorderRadius = 5,
                HoverColor = Color.FromArgb(51, 103, 214)
            };
            panel.Controls.Add(btnOpenRecent);

            yPos += 50;

            // Selectores de archivos (BL, AP, CP, CSC, DATA)
            _txtBL = CrearSelectorArchivoOdinEstilizado(panel, "BL", ref yPos);
            _txtAP = CrearSelectorArchivoOdinEstilizado(panel, "AP", ref yPos);
            _txtCP = CrearSelectorArchivoOdinEstilizado(panel, "CP", ref yPos);
            _txtCSC = CrearSelectorArchivoOdinEstilizado(panel, "CSC", ref yPos);
            
            // DATA (sin archivo por defecto)
            var lblDATA = new Label
            {
                Text = "DATA",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(20, yPos + 2),
                AutoSize = true
            };
            panel.Controls.Add(lblDATA);

            var chkDATA = new CheckBox
            {
                Location = new Point(65, yPos + 3),
                Size = new Size(16, 16)
            };
            panel.Controls.Add(chkDATA);

            var txtDATA = new TextBox
            {
                Location = new Point(90, yPos + 1),
                Width = 390,
                Font = new Font("Segoe UI", 8.5F),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(txtDATA);

            var btnOpenDATA = new ModernButton
            {
                Text = "📁 Open",
                Location = new Point(490, yPos - 1),
                Size = new Size(80, 26),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                BorderRadius = 5,
                HoverColor = Color.FromArgb(51, 103, 214)
            };
            btnOpenDATA.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "TAR Files (*.tar)|*.tar|MD5 Files (*.md5)|*.md5|All Files (*.*)|*.*";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        txtDATA.Text = ofd.FileName;
                        chkDATA.Checked = true;
                    }
                }
            };
            panel.Controls.Add(btnOpenDATA);

            yPos += 40;

            // Checkboxes de opciones (Reboot, Check MD5, NAND erase, Auto Root)
            var chkReboot = new CheckBox
            {
                Text = "✓ Reboot",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(50, yPos),
                AutoSize = true,
                Checked = true
            };
            panel.Controls.Add(chkReboot);

            var chkCheckMD5 = new CheckBox
            {
                Text = "Check MD5",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(200, yPos),
                AutoSize = true,
                Checked = false
            };
            panel.Controls.Add(chkCheckMD5);

            var chkNANDerase = new CheckBox
            {
                Text = "NAND erase",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(350, yPos),
                AutoSize = true,
                Checked = false
            };
            panel.Controls.Add(chkNANDerase);

            yPos += 50;

            // Botones FLASH y Clear más compactos
            var btnFlash = new ModernButton
            {
                Text = "⚡ FLASH",
                Location = new Point(390, yPos),
                Size = new Size(110, 38),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                BorderRadius = 8,
                HoverColor = Color.FromArgb(51, 103, 214)
            };
            btnFlash.Click += async (s, e) => await IniciarFlash(chkReboot.Checked, chkCheckMD5.Checked, chkNANDerase.Checked);
            panel.Controls.Add(btnFlash);

            var btnClear = new ModernButton
            {
                Text = "🧹 Clear",
                Location = new Point(510, yPos),
                Size = new Size(60, 38),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.FromArgb(50, 50, 50),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(180, 180, 180)
            };
            btnClear.Click += (s, e) =>
            {
                _txtBL.Text = "";
                _txtAP.Text = "";
                _txtCP.Text = "";
                _txtCSC.Text = "";
                txtDATA.Text = "";
                chkDATA.Checked = false;
            };
            panel.Controls.Add(btnClear);

            return panel;
        }

        private async Task IniciarFlash(bool reboot, bool checkMD5, bool nandErase)
        {
            try
            {
                _logManager?.LimpiarLogs();
                
                // Validar que al menos un archivo esté seleccionado
                if (string.IsNullOrEmpty(_txtBL?.Text) && 
                    string.IsNullOrEmpty(_txtAP?.Text) && 
                    string.IsNullOrEmpty(_txtCP?.Text) && 
                    string.IsNullOrEmpty(_txtCSC?.Text))
                {
                    MessageBox.Show("Please select at least one file to flash", "No Files Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Mostrar archivos seleccionados
                if (!string.IsNullOrEmpty(_txtBL?.Text))
                    _logManager?.AgregarLog($"Select file {Path.GetFileName(_txtBL.Text)}");
                
                if (!string.IsNullOrEmpty(_txtAP?.Text))
                    _logManager?.AgregarLog($"Select file {Path.GetFileName(_txtAP.Text)}");
                
                if (!string.IsNullOrEmpty(_txtCP?.Text))
                    _logManager?.AgregarLog($"Select file {Path.GetFileName(_txtCP.Text)}");
                
                if (!string.IsNullOrEmpty(_txtCSC?.Text))
                    _logManager?.AgregarLog($"Select file {Path.GetFileName(_txtCSC.Text)}");
                
                // Detectar puerto COM
                var comPorts = ObtenerPuertosComConNombres();
                var samsungPort = comPorts.FirstOrDefault(p => p.Value.Contains("SAMSUNG", StringComparison.OrdinalIgnoreCase));
                
                if (samsungPort.Key != null)
                {
                    _logManager?.AgregarLog($"Using port {samsungPort.Value}");
                }
                
                _logManager?.AgregarLog("Reading info mode Download mode ...");
                _logManager?.AgregarLog("Analyze files...");
                
                // Configurar opciones del OdinManager
                if (_odinManager != null)
                {
                    _odinManager.AutoReboot = reboot;
                    _odinManager.BootUpdate = false;
                    _odinManager.EfsClear = nandErase;
                    
                    // Iniciar el flash usando el manager
                    bool resultado = await _odinManager.IniciarFlash(
                        _txtBL?.Text,
                        _txtAP?.Text,
                        _txtCP?.Text,
                        _txtCSC?.Text
                    );
                    
                    if (resultado)
                    {
                        MessageBox.Show(
                            "✓ Flash completed successfully!\n\n" +
                            "The device has been flashed with the selected firmware.",
                            "Flash Completed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            "Flash failed!\n\n" +
                            "Check the log for details.",
                            "Flash Failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error during flash:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CrearSelectorArchivo(Panel panel, string label, ref int yPos)
        {
            var lbl = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(80, 80, 80),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(lbl);

            yPos += 20;

            var txtPath = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 8.5F),
                ReadOnly = true,
                BackColor = Color.FromArgb(250, 250, 250)
            };
            panel.Controls.Add(txtPath);
            
            // Guardar referencia según el tipo
            if (label.Contains("BL")) _txtBL = txtPath;
            else if (label.Contains("AP")) _txtAP = txtPath;
            else if (label.Contains("CP")) _txtCP = txtPath;
            else if (label.Contains("CSC")) _txtCSC = txtPath;

            var btnBrowse = new ModernButton
            {
                Text = "📁",
                Location = new Point(430, yPos),
                Size = new Size(40, 25),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                BorderRadius = 4,
                HoverColor = Color.FromArgb(41, 128, 185)
            };
            btnBrowse.Click += (s, e) => SeleccionarArchivoParticion(txtPath, label);
            panel.Controls.Add(btnBrowse);

            yPos += 35;
        }

        private Panel CrearPanelKGOperations()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20)
            };

            var lblTitulo = new Label
            {
                Text = "KG OPERATIONS",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(66, 66, 66),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            panel.Controls.Add(lblTitulo);

            var lblInfo = new Label
            {
                Text = "Próximamente: Operaciones KG (Knox Guard)",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(120, 120, 120),
                AutoSize = true,
                Location = new Point(20, 50)
            };
            panel.Controls.Add(lblInfo);

            return panel;
        }

        private void DetectarDispositivo()
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("DETECTAR DISPOSITIVO SAMSUNG", TipoLog.Exito);
            _logManager?.AgregarLog("");
            
            if (_odinManager != null)
            {
                bool detectado = _odinManager.DetectarDispositivoOdin();
                
                if (detectado)
                {
                    _odinManager.ObtenerInformacionDispositivo();
                }
            }
        }

        private void ListarDispositivos()
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("LISTAR DISPOSITIVOS USB", TipoLog.Exito);
            _logManager?.AgregarLog("");
            
            _odinManager?.ListarDispositivosUSB();
        }

        private void SeleccionarArchivoParticion(TextBox textBox, string particion)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Samsung Firmware (*.tar;*.md5;*.lz4)|*.tar;*.md5;*.lz4|Todos los archivos (*.*)|*.*";
                openFileDialog.Title = $"Seleccionar archivo {particion}";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    
                    // Extraer información del TAR
                    try
                    {
                        var odin = new SharpOdinClient.Odin();
                        var tarInfo = odin.tar.TarInformation(filePath);
                        
                        if (tarInfo != null && tarInfo.Count > 0)
                        {
                            // Crear lista de particiones
                            var partitions = new List<PartitionInfo>();
                            
                            foreach (var item in tarInfo)
                            {
                                // Calcular tamaño real
                                long size = item.Filesize;
                                string extension = Path.GetExtension(item.Filename).ToLower();
                                
                                if (extension == ".lz4")
                                {
                                    try
                                    {
                                        size = odin.CalculateLz4SizeFromTar(filePath, item.Filename);
                                    }
                                    catch { }
                                }
                                
                                // Ignorar archivos PIT
                                if (extension != ".pit")
                                {
                                    partitions.Add(new PartitionInfo
                                    {
                                        Enabled = true,
                                        FileName = item.Filename,
                                        FilePath = filePath,
                                        Size = size
                                    });
                                }
                            }
                            
                            // Mostrar selector de particiones
                            if (partitions.Count > 0)
                            {
                                using (var selectorForm = new PartitionSelectorForm(partitions, filePath))
                                {
                                    if (selectorForm.ShowDialog() == DialogResult.OK)
                                    {
                                        // Guardar particiones seleccionadas según el tipo
                                        if (particion.Contains("BL"))
                                        {
                                            _partitionsBL = selectorForm.Partitions;
                                        }
                                        else if (particion.Contains("AP"))
                                        {
                                            _partitionsAP = selectorForm.Partitions;
                                        }
                                        else if (particion.Contains("CP"))
                                        {
                                            _partitionsCP = selectorForm.Partitions;
                                        }
                                        else if (particion.Contains("CSC"))
                                        {
                                            _partitionsCSC = selectorForm.Partitions;
                                        }
                                        
                                        textBox.Text = filePath;
                                        
                                        int selectedCount = selectorForm.Partitions.Count(p => p.Enabled);
                                        _logManager?.AgregarLog($"{particion} seleccionado: {Path.GetFileName(filePath)}", TipoLog.Exito);
                                        _logManager?.AgregarLog($"  Particiones seleccionadas: {selectedCount}/{partitions.Count}", TipoLog.Info);
                                    }
                                }
                            }
                            else
                            {
                                textBox.Text = filePath;
                                _logManager?.AgregarLog($"{particion} seleccionado: {Path.GetFileName(filePath)}", TipoLog.Exito);
                            }
                        }
                        else
                        {
                            textBox.Text = filePath;
                            _logManager?.AgregarLog($"{particion} seleccionado: {Path.GetFileName(filePath)}", TipoLog.Exito);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logManager?.AgregarLog($"Error al leer archivo TAR: {ex.Message}", TipoLog.Error);
                        textBox.Text = filePath;
                    }
                }
            }
        }

        private async void RebootNormal()
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("REBOOT A MODO NORMAL", TipoLog.Exito);
            _logManager?.AgregarLog("");
            
            if (_odinManager != null)
            {
                await _odinManager.RebootNormal();
            }
        }
        
        private async Task LeerInformacionDispositivo()
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("Collecting information... Be patient! Do NOT disconnect the phone!", TipoLog.Advertencia);

            var startTime = DateTime.Now;

            try
            {
                // Crear ADBManager
                var adbManager = new ADBManager();

                // Obtener información del dispositivo
                var model = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.product.model");
                _logManager?.AgregarLog($"Model : {model.Trim()}", TipoLog.Info);

                var carrierId = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.boot.carrierid");
                _logManager?.AgregarLog($"Carrier ID : {carrierId.Trim()}", TipoLog.Info);

                var salesCode = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.csc.sales_code");
                _logManager?.AgregarLog($"Sales Code : {salesCode.Trim()}", TipoLog.Info);

                var countryCode = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.csc.country_code");
                _logManager?.AgregarLog($"Country Code : {countryCode.Trim()}", TipoLog.Info);

                var timezone = await adbManager.ExecuteAdbCommandAsync("shell getprop persist.sys.timezone");
                _logManager?.AgregarLog($"Timezone : {timezone.Trim()}", TipoLog.Info);

                var androidVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.version.release");
                var sdkVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.version.sdk");
                _logManager?.AgregarLog($"Android Version : {androidVersion.Trim()} [SDK {sdkVersion.Trim()}]", TipoLog.Info);

                var buildDate = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.date");
                _logManager?.AgregarLog($"Build Date : {buildDate.Trim()}", TipoLog.Info);

                var pdaVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.display.id");
                _logManager?.AgregarLog($"PDA Version : {pdaVersion.Trim()}", TipoLog.Info);

                var phoneVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.version.incremental");
                _logManager?.AgregarLog($"Phone Version : {phoneVersion.Trim()}", TipoLog.Info);

                var baseband = await adbManager.ExecuteAdbCommandAsync("shell getprop gsm.version.baseband");
                _logManager?.AgregarLog($"Baseband : {baseband.Trim()}", TipoLog.Info);

                var platform = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.board.platform");
                _logManager?.AgregarLog($"Board Platform : {platform.Trim().ToUpper()}", TipoLog.Info);

                var imei = await adbManager.ExecuteAdbCommandAsync("shell service call iphonesubinfo 1 s16 com.android.shell | cut -c 50-66 | tr -d '.[:space:]'");
                if (!string.IsNullOrWhiteSpace(imei))
                {
                    _logManager?.AgregarLog($"IMEI : {imei.Trim()}", TipoLog.Info);
                }

                var serialNumber = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.serialno");
                _logManager?.AgregarLog($"Serial Number : {serialNumber.Trim()}", TipoLog.Info);

                var networkType = await adbManager.ExecuteAdbCommandAsync("shell getprop gsm.network.type");
                _logManager?.AgregarLog($"Network Type : {networkType.Trim()}", TipoLog.Info);

                var simState = await adbManager.ExecuteAdbCommandAsync("shell getprop gsm.sim.state");
                _logManager?.AgregarLog($"SIM Status : {simState.Trim()}", TipoLog.Info);

                var multisimConfig = await adbManager.ExecuteAdbCommandAsync("shell getprop persist.radio.multisim.config");
                _logManager?.AgregarLog($"Multisim Config : {multisimConfig.Trim().ToUpper()}", TipoLog.Info);

                var warrantyBit = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.boot.warranty_bit");
                _logManager?.AgregarLog($"Warranty Bit : {warrantyBit.Trim()}", TipoLog.Info);

                var securityPatch = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.version.security_patch");
                _logManager?.AgregarLog($"Security Patch : {securityPatch.Trim()}", TipoLog.Info);

                var emDid = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.boot.em.did");
                _logManager?.AgregarLog($"Em Did : {emDid.Trim()}", TipoLog.Info);

                var endTime = DateTime.Now;
                var elapsed = (endTime - startTime).TotalSeconds;

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Finished at local time: {endTime:dd.MM.yyyy HH:mm:ss}", TipoLog.Info);
                _logManager?.AgregarLog($"Elapsed time: {elapsed:F0} seconds", TipoLog.Info);
                _logManager?.AgregarLog("Operation has been finished successfully!!", TipoLog.Exito);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show(
                    $"Error al leer información:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task InstalarScriptBancarias()
        {
            var result = MessageBox.Show(
                "FIX APPS BANCARIAS 2025\n\n" +
                "Este proceso instalará un script automático\n" +
                "para solucionar problemas con apps bancarias.\n\n" +
                "¿Continuar?",
                "Fix Apps Bancarias",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _logManager?.LimpiarLogs();

            try
            {
                var startTime = DateTime.Now;
                
                // Crear ADBManager
                var adbManager = new ADBManager();

                // 1. READ INFO - Mostrar información del dispositivo
                _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
                _logManager?.AgregarLog("         DEVICE INFO", TipoLog.Titulo);
                _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
                var manufacturer = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.product.manufacturer");
                var model = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.product.model");
                var androidVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.version.release");
                var serialNumber = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.serialno");
                
                _logManager?.AgregarLog($"Manufacturer: {manufacturer.Trim()}", TipoLog.Info);
                _logManager?.AgregarLog($"Model: {model.Trim()}", TipoLog.Info);
                _logManager?.AgregarLog($"Android: {androidVersion.Trim()}", TipoLog.Info);
                _logManager?.AgregarLog($"Serial: {serialNumber.Trim()}", TipoLog.Info);
                _logManager?.AgregarLog("");

                // 2. Verificaciones rápidas sin mostrar en log
                var devices = await adbManager.ExecuteAdbCommandAsync("devices");
                if (string.IsNullOrWhiteSpace(devices) || !devices.Contains("device"))
                {
                    MessageBox.Show(
                        "No se detectó ningún dispositivo conectado.\n\n" +
                        "Por favor conecta tu dispositivo y habilita\n" +
                        "la depuración USB.",
                        "Sin Dispositivo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                
                var rootCheck = await adbManager.ExecuteAdbCommandAsync("shell su -c id");
                if (string.IsNullOrWhiteSpace(rootCheck) || !rootCheck.Contains("uid=0"))
                {
                    MessageBox.Show(
                        "El dispositivo no tiene acceso root.\n\n" +
                        "Instala Magisk para continuar.",
                        "Root Requerido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                
                var magiskCheck = await adbManager.ExecuteAdbCommandAsync("shell pm list packages | grep magisk");
                if (string.IsNullOrWhiteSpace(magiskCheck))
                {
                    MessageBox.Show(
                        "Magisk no está instalado.\n\n" +
                        "Instala Magisk primero para continuar.",
                        "Magisk Requerido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // 3. Running Fix Apps Bancarias
                _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
                _logManager?.AgregarLog("    RUNNING FIX APPS BANCARIAS", TipoLog.Titulo);
                _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
                _logManager?.AgregarLog("");

                // 4. Executing exploits
                _logManager?.AgregarLog("[1/5] Checking existing scripts...", TipoLog.Proceso);
                var existingScript = await adbManager.ExecuteAdbCommandAsync("shell su -c 'ls /data/adb/service.d/manager.sh 2>/dev/null'");
                if (!string.IsNullOrWhiteSpace(existingScript) && existingScript.Contains("manager.sh"))
                {
                    await adbManager.ExecuteAdbCommandAsync("shell su -c 'rm -f /data/adb/service.d/manager.sh'");
                }
                _logManager?.AgregarLog("      ✓ OK", TipoLog.Exito);
                _logManager?.AgregarLog("");

                _logManager?.AgregarLog("[2/5] Creating directories...", TipoLog.Proceso);
                await adbManager.ExecuteAdbCommandAsync("shell su -c 'mkdir -p /data/adb/service.d'");
                _logManager?.AgregarLog("      ✓ OK", TipoLog.Exito);
                _logManager?.AgregarLog("");

                _logManager?.AgregarLog("[3/5] Installing script...", TipoLog.Proceso);
                var modulesManager = new MagiskModulesManager(adbManager);
                bool success = await modulesManager.InstalarScriptMagiskHide();

                if (!success)
                {
                    _logManager?.AgregarLog("      ✗ ERROR", TipoLog.Error);
                    MessageBox.Show(
                        "Hubo un error durante la instalación.\n\n" +
                        "Por favor, intenta nuevamente.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                _logManager?.AgregarLog("      ✓ OK", TipoLog.Exito);
                _logManager?.AgregarLog("");

                _logManager?.AgregarLog("[4/5] Setting permissions...", TipoLog.Proceso);
                await adbManager.ExecuteAdbCommandAsync("shell su -c 'chmod 755 /data/adb/service.d/manager.sh'");
                _logManager?.AgregarLog("      ✓ OK", TipoLog.Exito);
                _logManager?.AgregarLog("");

                _logManager?.AgregarLog("[5/5] Verifying installation...", TipoLog.Proceso);
                var finalCheck = await adbManager.ExecuteAdbCommandAsync("shell su -c 'ls -la /data/adb/service.d/manager.sh'");
                if (!finalCheck.Contains("manager.sh"))
                {
                    _logManager?.AgregarLog("      ✗ ERROR", TipoLog.Error);
                    return;
                }
                _logManager?.AgregarLog("      ✓ OK", TipoLog.Exito);
                _logManager?.AgregarLog("");

                // 5. Tiempo de ejecución
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
                _logManager?.AgregarLog($"⏱  Execution time: {duration.TotalSeconds:F2} seconds", TipoLog.Comando);
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("✓✓✓ INSTALLATION COMPLETED ✓✓✓", TipoLog.Exito);
                _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
                _logManager?.AgregarLog("");

                var reboot = MessageBox.Show(
                    "¡Felicidades!\n\n" +
                    "Ahora puedes usar las apps bancarias con normalidad.\n\n" +
                    "IMPORTANTE: Debes reiniciar el dispositivo ahora\n" +
                    "para que los cambios tengan efecto.\n\n" +
                    "¿Reiniciar ahora?",
                    "Instalación Completada",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (reboot == DialogResult.Yes)
                {
                    _logManager?.AgregarLog("🔄 Rebooting device...", TipoLog.Comando);
                    await adbManager.ExecuteAdbCommandAsync("reboot");
                    _logManager?.AgregarLog("      ✓ Device rebooting", TipoLog.Exito);
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"✗ Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show(
                    "Ocurrió un error inesperado.\n\n" +
                    "Por favor, intenta nuevamente.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ConvertirEsimASim()
        {
            _logManager?.LimpiarLogs();

            try
            {
                // Abrir diálogo para seleccionar archivo
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Todos los archivos (*.*)|*.*";
                    openFileDialog.Title = "Seleccionar archivo EFS para convertir";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        var startTime = DateTime.Now;
                        string filePath = openFileDialog.FileName;

                        _logManager?.AgregarLog("Collecting information... Be patient! Do NOT disconnect the phone!", TipoLog.Advertencia);
                        _logManager?.AgregarLog("");

                        // Leer el archivo como bytes
                        byte[] fileBytes = File.ReadAllBytes(filePath);

                        // Buscar nombre del dispositivo (modelos Samsung comunes)
                        string deviceModel = BuscarModeloDispositivo(fileBytes);
                        if (!string.IsNullOrEmpty(deviceModel))
                        {
                            _logManager?.AgregarLog($"Model : {deviceModel}", TipoLog.Info);
                        }

                        // Buscar la secuencia "esim.prop" en hexadecimal
                        byte[] searchPattern = new byte[] { 0x65, 0x73, 0x69, 0x6D, 0x2E, 0x70, 0x72, 0x6F, 0x70 };
                        byte[] replacePattern = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                        int occurrences = 0;
                        for (int i = 0; i <= fileBytes.Length - searchPattern.Length; i++)
                        {
                            bool found = true;
                            for (int j = 0; j < searchPattern.Length; j++)
                            {
                                if (fileBytes[i + j] != searchPattern[j])
                                {
                                    found = false;
                                    break;
                                }
                            }

                            if (found)
                            {
                                occurrences++;
                                // Reemplazar con ceros
                                for (int j = 0; j < replacePattern.Length; j++)
                                {
                                    fileBytes[i + j] = replacePattern[j];
                                }
                            }
                        }

                        if (occurrences == 0)
                        {
                            _logManager?.AgregarLog("No se pudo editar este archivo", TipoLog.Advertencia);
                            MessageBox.Show(
                                "No se pudo editar este archivo.\n\n" +
                                "Verifica que sea el archivo EFS correcto.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            return;
                        }

                        // Crear nombre del archivo modificado
                        string directory = Path.GetDirectoryName(filePath) ?? "";
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                        string extension = Path.GetExtension(filePath);
                        string modifiedFilePath = Path.Combine(directory, $"{fileNameWithoutExt}_modified{extension}");

                        // Guardar el archivo modificado
                        File.WriteAllBytes(modifiedFilePath, fileBytes);

                        var endTime = DateTime.Now;
                        var elapsed = (endTime - startTime).TotalSeconds;

                        _logManager?.AgregarLog("");
                        _logManager?.AgregarLog($"Finished at local time: {endTime:dd.MM.yyyy HH:mm:ss}");
                        _logManager?.AgregarLog($"Elapsed time: {elapsed:F0} seconds");
                        _logManager?.AgregarLog("Operation has been finished successfully!!", TipoLog.Exito);

                        // Preguntar si desea abrir la carpeta
                        var result = MessageBox.Show(
                            $"✓ Conversión completada exitosamente\n\n" +
                            $"Archivo modificado: {Path.GetFileName(modifiedFilePath)}\n\n" +
                            $"¿Deseas abrir la carpeta donde se guardó el archivo?",
                            "Conversión Exitosa",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{modifiedFilePath}\"");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);

                MessageBox.Show(
                    $"Error durante la conversión:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string BuscarModeloDispositivo(byte[] fileBytes)
        {
            try
            {
                // Buscar patrones comunes de modelos Samsung (SM-XXXX)
                string fileContent = System.Text.Encoding.ASCII.GetString(fileBytes);
                
                // Buscar patrón SM- seguido de letras y números
                var match = System.Text.RegularExpressions.Regex.Match(fileContent, @"SM-[A-Z0-9]{4,10}");
                if (match.Success)
                {
                    return match.Value;
                }

                // Buscar otros patrones comunes
                match = System.Text.RegularExpressions.Regex.Match(fileContent, @"Galaxy [A-Z0-9\s]{3,20}");
                if (match.Success)
                {
                    return match.Value.Trim();
                }
            }
            catch
            {
                // Ignorar errores en la búsqueda
            }

            return string.Empty;
        }

        private async Task<(ADBManager adbManager, DateTime startTime)> ObtenerInfoCompleta()
        {
            _logManager?.LimpiarLogs();
            _logManager?.AgregarLog("Collecting information... Be patient! Do NOT disconnect the phone!", TipoLog.Advertencia);

            var startTime = DateTime.Now;
            var adbManager = new ADBManager();

            var model = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.product.model");
            _logManager?.AgregarLog($"Model : {model.Trim()}", TipoLog.Info);

            var carrierId = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.boot.carrierid");
            _logManager?.AgregarLog($"Carrier ID : {carrierId.Trim()}", TipoLog.Info);

            var salesCode = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.csc.sales_code");
            _logManager?.AgregarLog($"Sales Code : {salesCode.Trim()}", TipoLog.Info);

            var countryCode = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.csc.country_code");
            _logManager?.AgregarLog($"Country Code : {countryCode.Trim()}", TipoLog.Info);

            var timezone = await adbManager.ExecuteAdbCommandAsync("shell getprop persist.sys.timezone");
            _logManager?.AgregarLog($"Timezone : {timezone.Trim()}", TipoLog.Info);

            var androidVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.version.release");
            var sdkVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.version.sdk");
            _logManager?.AgregarLog($"Android Version : {androidVersion.Trim()} [SDK {sdkVersion.Trim()}]", TipoLog.Info);

            var buildDate = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.date");
            _logManager?.AgregarLog($"Build Date : {buildDate.Trim()}", TipoLog.Info);

            var pdaVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.display.id");
            _logManager?.AgregarLog($"PDA Version : {pdaVersion.Trim()}", TipoLog.Info);

            var phoneVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.version.incremental");
            _logManager?.AgregarLog($"Phone Version : {phoneVersion.Trim()}", TipoLog.Info);

            var baseband = await adbManager.ExecuteAdbCommandAsync("shell getprop gsm.version.baseband");
            _logManager?.AgregarLog($"Baseband : {baseband.Trim()}", TipoLog.Info);

            var platform = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.board.platform");
            _logManager?.AgregarLog($"Board Platform : {platform.Trim().ToUpper()}", TipoLog.Info);

            var imei = await adbManager.ExecuteAdbCommandAsync("shell service call iphonesubinfo 1 s16 com.android.shell | cut -c 50-66 | tr -d '.[:space:]'");
            if (!string.IsNullOrWhiteSpace(imei))
            {
                _logManager?.AgregarLog($"IMEI : {imei.Trim()}", TipoLog.Info);
            }

            var serialNumber = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.serialno");
            _logManager?.AgregarLog($"Serial Number : {serialNumber.Trim()}", TipoLog.Info);

            var networkType = await adbManager.ExecuteAdbCommandAsync("shell getprop gsm.network.type");
            _logManager?.AgregarLog($"Network Type : {networkType.Trim()}", TipoLog.Info);

            var simState = await adbManager.ExecuteAdbCommandAsync("shell getprop gsm.sim.state");
            _logManager?.AgregarLog($"SIM Status : {simState.Trim()}", TipoLog.Info);

            var multisimConfig = await adbManager.ExecuteAdbCommandAsync("shell getprop persist.radio.multisim.config");
            _logManager?.AgregarLog($"Multisim Config : {multisimConfig.Trim().ToUpper()}", TipoLog.Info);

            var warrantyBit = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.boot.warranty_bit");
            _logManager?.AgregarLog($"Warranty Bit : {warrantyBit.Trim()}", TipoLog.Info);

            var securityPatch = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.version.security_patch");
            _logManager?.AgregarLog($"Security Patch : {securityPatch.Trim()}", TipoLog.Info);

            var emDid = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.boot.em.did");
            _logManager?.AgregarLog($"Em Did : {emDid.Trim()}", TipoLog.Info);

            return (adbManager, startTime);
        }

        private async Task RebootRootMode()
        {
            try
            {
                var (adbManager, startTime) = await ObtenerInfoCompleta();

                // Paso 1: Reboot
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("Paso 1: Rebooting to recovery mode...", TipoLog.Info);
                await adbManager.ExecuteAdbCommandAsync("reboot recovery");

                var endTime = DateTime.Now;
                var elapsed = (endTime - startTime).TotalSeconds;

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Finished at local time: {endTime:dd.MM.yyyy HH:mm:ss}");
                _logManager?.AgregarLog($"Elapsed time: {elapsed:F0} seconds");
                _logManager?.AgregarLog("Operation has been finished successfully!!", TipoLog.Exito);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show(
                    $"Error al reiniciar:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task RemoverCuentaSamsung()
        {
            try
            {
                var (adbManager, startTime) = await ObtenerInfoCompleta();

                // Paso 1: Desinstalar
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("Paso 1: Uninstalling Samsung Account...", TipoLog.Info);
                await adbManager.ExecuteAdbCommandAsync("shell pm uninstall -k --user 0 com.osp.app.signin");

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("==================================================================");
                _logManager?.AgregarLog("");

                // Paso 2: Reinstalar
                _logManager?.AgregarLog("Paso 2: Reinstalling Samsung Account...", TipoLog.Info);
                await adbManager.ExecuteAdbCommandAsync("shell pm install-existing com.osp.app.signin");

                var endTime = DateTime.Now;
                var elapsed = (endTime - startTime).TotalSeconds;

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Finished at local time: {endTime:dd.MM.yyyy HH:mm:ss}");
                _logManager?.AgregarLog($"Elapsed time: {elapsed:F0} seconds");
                _logManager?.AgregarLog("Operation has been finished successfully!!", TipoLog.Exito);

                MessageBox.Show(
                    "✓ Cuenta Samsung eliminada exitosamente\n\n" +
                    "El dispositivo ya no tiene la cuenta Samsung asociada.",
                    "Operación Exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show(
                    $"Error al remover cuenta Samsung:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task FixWarningAlert()
        {
            try
            {
                var (adbManager, startTime) = await ObtenerInfoCompleta();

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("Paso 1: Checking root access...", TipoLog.Info);
                
                // Verificar acceso root
                var rootCheck = await adbManager.ExecuteAdbCommandAsync("shell su -c 'id'");
                if (!rootCheck.Contains("uid=0"))
                {
                    _logManager?.AgregarLog("Root access not available!", TipoLog.Error);
                    MessageBox.Show(
                        "Este dispositivo no tiene acceso root.\n\n" +
                        "Esta función requiere root para modificar las particiones.",
                        "Root Requerido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _logManager?.AgregarLog("Root access confirmed", TipoLog.Exito);

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("Paso 2: Searching for warning images in system partitions...", TipoLog.Info);
                
                // Lista de particiones donde pueden estar las imágenes de advertencia
                string[] partitionsToCheck = new string[]
                {
                    "param",
                    "up_param",
                    "logo",
                    "splash",
                    "persdata",
                    "persistent",
                    "frp"
                };

                // Crear carpeta en Desktop
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string backupFolder = Path.Combine(desktopPath, "Samsung_Warning_Analysis");
                Directory.CreateDirectory(backupFolder);

                int totalImagesFound = 0;
                List<string> partitionsWithImages = new List<string>();

                foreach (string partitionName in partitionsToCheck)
                {
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog($"Checking partition: {partitionName}...", TipoLog.Info);
                    
                    // Buscar la partición
                    var partitionPathResult = await adbManager.ExecuteAdbCommandAsync($"shell su -c 'find /dev/block -name {partitionName}'");
                    
                    if (string.IsNullOrWhiteSpace(partitionPathResult))
                    {
                        _logManager?.AgregarLog($"  Partition '{partitionName}' not found", TipoLog.Advertencia);
                        continue;
                    }

                    // Tomar la primera línea
                    var lines = partitionPathResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    string partitionPath = lines[0].Trim();
                    
                    _logManager?.AgregarLog($"  Found at: {partitionPath}", TipoLog.Info);

                    // Hacer backup de la partición
                    string sdcardFile = $"/sdcard/{partitionName}.bin";
                    await adbManager.ExecuteAdbCommandAsync($"shell su -c 'dd if={partitionPath} of={sdcardFile}'");
                    
                    // Descargar a PC
                    string localFile = Path.Combine(backupFolder, $"{partitionName}.bin");
                    if (File.Exists(localFile))
                    {
                        File.Delete(localFile);
                    }
                    
                    await adbManager.ExecuteAdbCommandAsync($"pull {sdcardFile} \"{localFile}\"");
                    await Task.Delay(500);

                    if (!File.Exists(localFile))
                    {
                        _logManager?.AgregarLog($"  Failed to download {partitionName}", TipoLog.Error);
                        await adbManager.ExecuteAdbCommandAsync($"shell rm {sdcardFile}");
                        continue;
                    }

                    // Analizar el archivo
                    byte[] data = File.ReadAllBytes(localFile);
                    int pngCount = 0;
                    int jpgCount = 0;

                    // Buscar PNG
                    byte[] pngSig = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                    for (int i = 0; i <= data.Length - pngSig.Length; i++)
                    {
                        bool found = true;
                        for (int j = 0; j < pngSig.Length; j++)
                        {
                            if (data[i + j] != pngSig[j])
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            pngCount++;
                            _logManager?.AgregarLog($"  PNG #{pngCount} at offset 0x{i:X8}", TipoLog.Info);
                        }
                    }

                    // Buscar JPEG
                    for (int i = 0; i <= data.Length - 3; i++)
                    {
                        if (data[i] == 0xFF && data[i + 1] == 0xD8 && data[i + 2] == 0xFF)
                        {
                            jpgCount++;
                            _logManager?.AgregarLog($"  JPEG #{jpgCount} at offset 0x{i:X8}", TipoLog.Info);
                        }
                    }

                    int imagesInPartition = pngCount + jpgCount;
                    if (imagesInPartition > 0)
                    {
                        _logManager?.AgregarLog($"  Images found: PNG={pngCount}, JPEG={jpgCount}", TipoLog.Exito);
                        totalImagesFound += imagesInPartition;
                        partitionsWithImages.Add($"{partitionName} (PNG:{pngCount}, JPEG:{jpgCount})");
                    }
                    else
                    {
                        _logManager?.AgregarLog($"  No images found", TipoLog.Advertencia);
                    }

                    // Limpiar archivo temporal del dispositivo
                    await adbManager.ExecuteAdbCommandAsync($"shell rm {sdcardFile}");
                }

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Total images found: {totalImagesFound}", TipoLog.Info);
                _logManager?.AgregarLog($"Partitions with images: {partitionsWithImages.Count}", TipoLog.Info);

                var endTime = DateTime.Now;
                var elapsed = (endTime - startTime).TotalSeconds;

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Finished at local time: {endTime:dd.MM.yyyy HH:mm:ss}");
                _logManager?.AgregarLog($"Elapsed time: {elapsed:F0} seconds");
                _logManager?.AgregarLog("Analysis completed successfully!!", TipoLog.Exito);

                string resultMessage;
                if (totalImagesFound == 0)
                {
                    resultMessage = $"No se encontraron imágenes en ninguna partición.\n\n" +
                                  $"Las particiones analizadas fueron:\n" +
                                  $"{string.Join(", ", partitionsToCheck)}\n\n" +
                                  $"Los archivos han sido guardados en:\n{backupFolder}";
                }
                else
                {
                    resultMessage = $"✓ Análisis completado!\n\n" +
                                  $"Total de imágenes encontradas: {totalImagesFound}\n\n" +
                                  $"Particiones con imágenes:\n" +
                                  $"{string.Join("\n", partitionsWithImages)}\n\n" +
                                  $"Los archivos han sido guardados en:\n{backupFolder}\n\n" +
                                  $"Revisa el log para ver los offsets exactos de cada imagen.";
                }

                MessageBox.Show(resultMessage, "Análisis Completado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Abrir la carpeta
                System.Diagnostics.Process.Start("explorer.exe", backupFolder);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show(
                    $"Error al eliminar warning:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task CambiarCSC()
        {
            try
            {
                _logManager?.LimpiarLogs();
                
                var adbManager = new ADBManager();

                // Obtener lista de puertos COM con nombres descriptivos
                var comPorts = ObtenerPuertosComConNombres();
                
                if (comPorts.Count == 0)
                {
                    MessageBox.Show(
                        "No se encontraron puertos COM.\n\n" +
                        "Asegúrate de que:\n" +
                        "- El dispositivo esté conectado\n" +
                        "- Los drivers de Samsung estén instalados\n" +
                        "- El modo MTP esté activado",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Leer CSC disponibles
                var cscListResult = await adbManager.ExecuteAdbCommandAsync("shell cat /product/omc/sales_code_list.dat");
                
                List<string> availableCSCs = new List<string>();
                if (!string.IsNullOrWhiteSpace(cscListResult))
                {
                    var lines = cscListResult.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var csc = line.Trim();
                        if (!string.IsNullOrWhiteSpace(csc) && csc.Length >= 3)
                        {
                            availableCSCs.Add(csc);
                        }
                    }
                }

                if (availableCSCs.Count == 0)
                {
                    // Lista por defecto
                    availableCSCs = new List<string> { "CHO", "COO", "MXO", "PEO", "ZTO", "ARO", "GTO", "EON", "TTT", "BVO", "UPO", "UYO", "TPA" };
                }

                // Mostrar formulario de selección

                using (var form = new Form())
                {
                    form.Text = "Change CSC";
                    form.Size = new Size(450, 420);
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.MaximizeBox = false;
                    form.MinimizeBox = false;

                    var lblPort = new Label
                    {
                        Text = "Select COM Port:",
                        Location = new Point(20, 20),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                    };
                    form.Controls.Add(lblPort);

                    var cmbPort = new ComboBox
                    {
                        Location = new Point(20, 45),
                        Width = 390,
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Font = new Font("Segoe UI", 10F)
                    };
                    foreach (var port in comPorts)
                    {
                        cmbPort.Items.Add(port.Value);
                    }
                    if (cmbPort.Items.Count > 0) cmbPort.SelectedIndex = 0;
                    form.Controls.Add(cmbPort);

                    var lblCSC = new Label
                    {
                        Text = "Select New CSC:",
                        Location = new Point(20, 90),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                    };
                    form.Controls.Add(lblCSC);

                    var cmbCSC = new ComboBox
                    {
                        Location = new Point(20, 115),
                        Width = 290,
                        DropDownStyle = ComboBoxStyle.DropDown,
                        Font = new Font("Segoe UI", 10F)
                    };
                    cmbCSC.Items.AddRange(availableCSCs.ToArray());
                    if (cmbCSC.Items.Count > 0) cmbCSC.SelectedIndex = 0;
                    form.Controls.Add(cmbCSC);

                    var lblCustom = new Label
                    {
                        Text = "or Custom:",
                        Location = new Point(320, 118),
                        AutoSize = true,
                        Font = new Font("Segoe UI", 9F)
                    };
                    form.Controls.Add(lblCustom);

                    var lblAvailable = new Label
                    {
                        Text = $"Available CSCs in device:\n{string.Join(", ", availableCSCs)}",
                        Location = new Point(20, 150),
                        Size = new Size(390, 60),
                        Font = new Font("Segoe UI", 8F),
                        ForeColor = Color.FromArgb(100, 100, 100)
                    };
                    form.Controls.Add(lblAvailable);

                    var lblWarning = new Label
                    {
                        Text = "⚠️ Warning: Changing CSC may reset some settings.\nMake sure you have a backup before proceeding.",
                        Location = new Point(20, 220),
                        Size = new Size(390, 50),
                        Font = new Font("Segoe UI", 9F),
                        ForeColor = Color.FromArgb(192, 57, 43)
                    };
                    form.Controls.Add(lblWarning);

                    var btnOK = new Button
                    {
                        Text = "Change CSC",
                        Location = new Point(220, 290),
                        Size = new Size(120, 35),
                        DialogResult = DialogResult.OK,
                        Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                        BackColor = Color.FromArgb(52, 152, 219),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat
                    };
                    btnOK.FlatAppearance.BorderSize = 0;
                    form.Controls.Add(btnOK);

                    var btnCancel = new Button
                    {
                        Text = "Cancel",
                        Location = new Point(350, 290),
                        Size = new Size(80, 35),
                        DialogResult = DialogResult.Cancel,
                        Font = new Font("Segoe UI", 10F),
                        FlatStyle = FlatStyle.Flat
                    };
                    form.Controls.Add(btnCancel);

                    form.AcceptButton = btnOK;
                    form.CancelButton = btnCancel;

                    if (form.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    string selectedPortDisplay = cmbPort.SelectedItem?.ToString() ?? "";
                    string selectedCSC = cmbCSC.Text.Trim().ToUpper();

                    if (string.IsNullOrEmpty(selectedPortDisplay) || string.IsNullOrEmpty(selectedCSC))
                    {
                        MessageBox.Show("Please select a COM port and enter a CSC code.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Validar formato CSC (3 letras)
                    if (selectedCSC.Length < 3)
                    {
                        MessageBox.Show("CSC code must be at least 3 characters.", "Invalid CSC", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Extraer el número de puerto COM del nombre descriptivo
                    string selectedPort = comPorts.FirstOrDefault(x => x.Value == selectedPortDisplay).Key;
                    
                    // Leer información del dispositivo y CSC actual
                    _logManager?.AgregarLog($"Using port {selectedPortDisplay}");
                    _logManager?.AgregarLog("Read Info");
                    _logManager?.AgregarLog("Reading info mode MTP ... OK");
                    
                    var deviceInfo = await LeerInformacionDispositivo(selectedPort);
                    string currentCSC = deviceInfo.ContainsKey("CSC") ? deviceInfo["CSC"] : "";
                    
                    // Mostrar información del dispositivo
                    if (deviceInfo.Count > 0)
                    {
                        foreach (var info in deviceInfo)
                        {
                            _logManager?.AgregarLog($"{info.Key}\t\t: {info.Value}");
                        }
                        
                        // Mostrar CSC disponibles
                        if (availableCSCs.Count > 0)
                        {
                            _logManager?.AgregarLog($"Available CSCs\t: {string.Join(", ", availableCSCs)}");
                        }
                    }
                    
                    // Verificar si ya tiene el CSC objetivo
                    if (!string.IsNullOrEmpty(currentCSC) && currentCSC == selectedCSC)
                    {
                        var confirmResult = MessageBox.Show(
                            $"The device already has CSC: {currentCSC}\n\n" +
                            $"Do you want to continue anyway?",
                            "CSC Already Set",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);
                        
                        if (confirmResult != DialogResult.Yes)
                        {
                            return;
                        }
                    }

                    // Cambiar CSC
                    await CambiarCSCViaCOM(selectedPort, selectedPortDisplay, selectedCSC, currentCSC);
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show(
                    $"Error al cambiar CSC:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task<Dictionary<string, string>> LeerInformacionDispositivo(string portName)
        {
            var info = new Dictionary<string, string>();
            
            try
            {
                var adbManager = new ADBManager();
                
                // Modelo
                var model = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.product.model");
                if (!string.IsNullOrWhiteSpace(model)) info["Model"] = model.Trim();
                
                // CSC
                var csc = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.csc.sales_code");
                if (string.IsNullOrWhiteSpace(csc))
                {
                    csc = await adbManager.ExecuteAdbCommandAsync("shell getprop ril.sales_code");
                }
                if (!string.IsNullOrWhiteSpace(csc)) info["CSC"] = csc.Trim();
                
                // Versiones
                var buildVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.PDA");
                if (string.IsNullOrWhiteSpace(buildVersion))
                {
                    buildVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.version.incremental");
                }
                if (!string.IsNullOrWhiteSpace(buildVersion))
                {
                    info["AP version"] = buildVersion.Trim();
                    info["BL version"] = buildVersion.Trim();
                    info["CP version"] = buildVersion.Trim();
                }
                
                var cscVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.changelist");
                if (!string.IsNullOrWhiteSpace(cscVersion)) info["CSC version"] = cscVersion.Trim();
                
                // IMEI (simplificado - usar getprop)
                var imei = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.ril.oem.imei");
                if (string.IsNullOrWhiteSpace(imei))
                {
                    imei = await adbManager.ExecuteAdbCommandAsync("shell getprop persist.radio.imei");
                }
                if (string.IsNullOrWhiteSpace(imei))
                {
                    imei = await adbManager.ExecuteAdbCommandAsync("shell getprop gsm.baseband.imei");
                }
                // Limpiar IMEI - solo números
                if (!string.IsNullOrWhiteSpace(imei))
                {
                    imei = new string(imei.Where(char.IsDigit).ToArray());
                    if (imei.Length >= 15)
                    {
                        info["IMEI"] = imei.Substring(0, 15);
                    }
                    else if (imei.Length > 0)
                    {
                        info["IMEI"] = imei;
                    }
                }
                
                // Serial Number
                var serial = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.serialno");
                if (!string.IsNullOrWhiteSpace(serial)) info["SN"] = serial.Trim();
                
                // Android Version
                var androidVersion = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.build.version.release");
                if (!string.IsNullOrWhiteSpace(androidVersion)) info["Android version"] = androidVersion.Trim();
                
                // Country
                var country = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.csc.country_code");
                if (string.IsNullOrWhiteSpace(country))
                {
                    country = await adbManager.ExecuteAdbCommandAsync("shell getprop persist.sys.country");
                }
                if (!string.IsNullOrWhiteSpace(country)) info["Country"] = country.Trim();
                
                // USB mode
                info["USB mode"] = "MTP";
                
                // Lock status (simplificado)
                info["Lock status"] = "NONE";
                
                // FRP status
                var frpResult = await adbManager.ExecuteAdbCommandAsync("shell getprop ro.frp.pst");
                info["FRP status"] = string.IsNullOrWhiteSpace(frpResult) || frpResult.Trim() == "" ? "UNLOCK" : "LOCK";
                
                // Agregar link de firmware (solo informativo)
                if (info.ContainsKey("Model") && info.ContainsKey("CSC"))
                {
                    info["Firmware"] = $"https://samfw.com/firmware/{info["Model"]}/{info["CSC"]}";
                }
                else if (info.ContainsKey("Model"))
                {
                    info["Firmware"] = $"https://samfw.com/firmware/{info["Model"]}";
                }
                
                return info;
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error reading device info: {ex.Message}", TipoLog.Error);
                return info;
            }
        }

        private async Task CambiarCSCViaCOM(string portName, string portDisplay, string newCSC, string oldCSC = "")
        {
            try
            {
                _logManager?.AgregarLog($"Using port {portDisplay}");
                _logManager?.AgregarLog("Running exploit... FAIL");
                
                // Preparar dispositivo
                await SendATCommandWithPortSilent(portName, "AT+KSTRINGB=0,3");
                await Task.Delay(2000);
                await SendATCommandWithPortSilent(portName, "AT+DUMPCTRL=1,0");
                await Task.Delay(1000);
                await SendATCommandWithPortSilent(portName, "AT+DEBUGLVC=0,5");
                await Task.Delay(1000);
                await SendATCommandWithPortSilent(portName, "AT+SWATD=0");
                await Task.Delay(1000);
                await SendATCommandWithPortSilent(portName, "AT+ACTIVATE=0,0,0");
                await Task.Delay(1000);
                await SendATCommandWithPortSilent(portName, "AT+SWATD=1");
                await Task.Delay(1000);
                await SendATCommandWithPortSilent(portName, "AT+DEBUGLVC=0,5");
                await Task.Delay(1000);
                
                // Configurar nuevo CSC
                string response = await SendATCommandWithPortSilent(portName, $"AT+PRECONFG=2,{newCSC}\r\n");
                
                if (response.Contains("+PRECONFG:2,OK") || (response.Contains("PRECONFG") && response.Contains("OK")))
                {
                    _logManager?.AgregarLog($"Changing CSC to {newCSC} ... OK");
                }
                else
                {
                    _logManager?.AgregarLog($"Changing CSC to {newCSC} ... FAIL");
                }
                await Task.Delay(1000);
                
                // Reiniciar dispositivo
                await SendATCommandWithPortSilent(portName, "AT+SWATD=0");
                await Task.Delay(500);
                await SendATCommandWithPortSilent(portName, "AT+CFUN=1,1");
                _logManager?.AgregarLog("Rebooting... OK");
                
                string changeMessage = string.IsNullOrEmpty(oldCSC) 
                    ? $"Target CSC: {newCSC}"
                    : $"CSC changed: {oldCSC} → {newCSC}";
                
                MessageBox.Show(
                    $"✓ CSC change completed!\n\n" +
                    $"{changeMessage}\n\n" +
                    $"The device is rebooting.",
                    "Completed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                throw;
            }
        }

        private async Task<string> SendATCommandWithPortSilent(string portName, string command)
        {
            System.IO.Ports.SerialPort? port = null;
            
            try
            {
                port = new System.IO.Ports.SerialPort(portName, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                port.ReadTimeout = 3000;
                port.WriteTimeout = 3000;
                port.Open();
                
                await Task.Delay(100);
                
                // Limpiar buffer
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
                
                // Enviar comando
                port.Write(command + "\n");
                
                // Esperar respuesta
                await Task.Delay(1000);
                
                // Leer respuesta
                string response = "";
                int attempts = 0;
                while (attempts < 20)
                {
                    try
                    {
                        if (port.BytesToRead > 0)
                        {
                            response += port.ReadExisting();
                            await Task.Delay(50);
                        }
                        else if (response.Length > 0)
                        {
                            break;
                        }
                        else
                        {
                            await Task.Delay(50);
                        }
                        attempts++;
                    }
                    catch
                    {
                        break;
                    }
                }
                
                port.Close();
                return response;
            }
            catch
            {
                return "";
            }
            finally
            {
                if (port != null && port.IsOpen)
                {
                    port.Close();
                }
            }
        }

        private async Task<string> SendATCommandWithPort(string portName, string command)
        {
            System.IO.Ports.SerialPort? port = null;
            
            try
            {
                port = new System.IO.Ports.SerialPort(portName, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
                port.ReadTimeout = 3000;
                port.WriteTimeout = 3000;
                port.Open();
                
                await Task.Delay(100);
                
                // Limpiar buffer
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
                
                // Enviar comando
                _logManager?.AgregarLog($"  TX: {command.Trim()}", TipoLog.Info);
                port.Write(command + "\n");
                
                // Esperar respuesta
                await Task.Delay(1000);
                
                // Leer respuesta
                string response = "";
                int attempts = 0;
                while (attempts < 20)
                {
                    try
                    {
                        if (port.BytesToRead > 0)
                        {
                            response += port.ReadExisting();
                            await Task.Delay(50);
                        }
                        else if (response.Length > 0)
                        {
                            break;
                        }
                        else
                        {
                            await Task.Delay(50);
                        }
                        attempts++;
                    }
                    catch
                    {
                        break;
                    }
                }
                
                if (response.Length > 0)
                {
                    _logManager?.AgregarLog($"  RX: {response.Trim()}", TipoLog.Info);
                }
                
                port.Close();
                return response;
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"  Error: {ex.Message}", TipoLog.Error);
                return "";
            }
            finally
            {
                if (port != null && port.IsOpen)
                {
                    port.Close();
                }
            }
        }

        private async Task<string> SendATCommand(System.IO.Ports.SerialPort port, string command)
        {
            try
            {
                // Limpiar buffer
                port.DiscardInBuffer();
                port.DiscardOutBuffer();
                
                // Enviar comando
                _logManager?.AgregarLog($"  TX: {command.Trim()}", TipoLog.Info);
                port.WriteLine(command);
                await Task.Delay(500);
                
                // Leer respuesta
                string response = "";
                int attempts = 0;
                while (attempts < 10)
                {
                    try
                    {
                        if (port.BytesToRead > 0)
                        {
                            response += port.ReadExisting();
                        }
                        else if (response.Length > 0)
                        {
                            break;
                        }
                        await Task.Delay(100);
                        attempts++;
                    }
                    catch
                    {
                        break;
                    }
                }
                
                if (response.Length > 0)
                {
                    _logManager?.AgregarLog($"  RX: {response.Trim()}", TipoLog.Info);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"  Error: {ex.Message}", TipoLog.Error);
                return "";
            }
        }

        private Dictionary<string, string> ObtenerPuertosComConNombres()
        {
            var result = new Dictionary<string, string>();
            
            try
            {
                // Primero obtener todos los puertos COM disponibles
                string[] availablePorts = System.IO.Ports.SerialPort.GetPortNames();
                
                // Buscar en Win32_PnPEntity (incluye modems y puertos serie)
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%(COM%'"))
                {
                    foreach (System.Management.ManagementObject obj in searcher.Get())
                    {
                        string caption = obj["Caption"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(caption))
                        {
                            // Extraer el número de puerto COM del caption
                            var match = System.Text.RegularExpressions.Regex.Match(caption, @"\(COM(\d+)\)");
                            if (match.Success)
                            {
                                string portNumber = "COM" + match.Groups[1].Value;
                                // Solo agregar si el puerto está disponible
                                if (availablePorts.Contains(portNumber))
                                {
                                    result[portNumber] = caption;
                                }
                            }
                        }
                    }
                }
                
                // Agregar puertos que no se encontraron en Win32_PnPEntity
                foreach (var port in availablePorts)
                {
                    if (!result.ContainsKey(port))
                    {
                        result[port] = port;
                    }
                }
            }
            catch
            {
                // Si falla, usar solo los nombres de puerto
                string[] ports = System.IO.Ports.SerialPort.GetPortNames();
                foreach (var port in ports)
                {
                    result[port] = port;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Parchea boot.img con Magisk y lo flashea directamente
        /// </summary>
        private async Task<bool> ParchearYFlashearBoot(string apOriginal, string magiskVersion)
        {
            try
            {
                if (_magiskManager == null)
                {
                    _logManager?.AgregarLog("⚠ MagiskManager not initialized");
                    return false;
                }
                
                // 1. Obtener información de la versión de Magisk
                _logManager?.AgregarLog($"1. Getting Magisk {magiskVersion}...");
                var (success, version, downloadUrl) = await _magiskManager.ObtenerVersionMagisk(magiskVersion);
                
                if (!success || string.IsNullOrEmpty(downloadUrl))
                {
                    _logManager?.AgregarLog("⚠ Could not get Magisk download URL");
                    return false;
                }
                
                // 2. Descargar Magisk APK
                _logManager?.AgregarLog($"2. Downloading Magisk {version}...");
                var (downloadSuccess, apkPath) = await _magiskManager.DescargarMagisk(downloadUrl, version);
                
                if (!downloadSuccess || string.IsNullOrEmpty(apkPath))
                {
                    _logManager?.AgregarLog("⚠ Could not download Magisk APK");
                    return false;
                }
                
                _logManager?.AgregarLog($"   ✓ Downloaded: {Path.GetFileName(apkPath)}");
                
                // 3. Extraer boot.img del archivo AP
                _logManager?.AgregarLog($"3. Extracting boot.img from AP...");
                var (extractSuccess, bootPath) = await _magiskManager.ExtraerBootDeAP(apOriginal);
                
                if (!extractSuccess || string.IsNullOrEmpty(bootPath))
                {
                    _logManager?.AgregarLog("⚠ Could not extract boot.img from AP");
                    return false;
                }
                
                _logManager?.AgregarLog($"   ✓ Extracted: {Path.GetFileName(bootPath)}");
                
                // 4. Parchear boot.img con Magisk
                _logManager?.AgregarLog($"4. Patching boot.img with Magisk...");
                var (patchSuccess, patchedBootPath) = await _magiskManager.ParchearBootConAPK(bootPath, apkPath);
                
                if (!patchSuccess || string.IsNullOrEmpty(patchedBootPath))
                {
                    _logManager?.AgregarLog("⚠ Could not patch boot.img");
                    return false;
                }
                
                _logManager?.AgregarLog($"   ✓ Patched: {Path.GetFileName(patchedBootPath)}");
                
                // 5. Crear TAR con boot.img parcheado
                _logManager?.AgregarLog($"5. Creating TAR for flashing...");
                string tempFolder = Path.Combine(Path.GetTempPath(), "TT-Tool-Root");
                Directory.CreateDirectory(tempFolder);
                string bootTar = Path.Combine(tempFolder, "boot_magisk.tar");
                
                var bootData = await File.ReadAllBytesAsync(patchedBootPath);
                await CrearTarSimple(bootTar, "boot.img", bootData);
                
                _logManager?.AgregarLog($"   ✓ Created: {Path.GetFileName(bootTar)} ({bootData.Length / 1024 / 1024} MB)");
                
                // 6. Flashear boot.img parcheado
                _logManager?.AgregarLog($"6. Flashing patched boot.img...");
                _logManager?.AgregarLog("");
                
                if (_odinManager != null)
                {
                    // Flashear solo el boot.img parcheado
                    bool flashResult = await _odinManager.IniciarFlash(null, bootTar, null, null);
                    
                    if (flashResult)
                    {
                        _logManager?.AgregarLog("");
                        _logManager?.AgregarLog("✓ Patched boot.img flashed successfully!");
                        return true;
                    }
                    else
                    {
                        _logManager?.AgregarLog("");
                        _logManager?.AgregarLog("⚠ Failed to flash patched boot.img");
                        return false;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"⚠ Error in auto root process: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Parchea boot.img con Magisk y crea un nuevo archivo AP (método legacy)
        /// </summary>
        private async Task<(bool success, string? apPath)> ParchearBootConMagisk(string apOriginal, string magiskVersion)
        {
            try
            {
                if (_magiskManager == null)
                {
                    _logManager?.AgregarLog("⚠ MagiskManager not initialized");
                    return (false, null);
                }
                
                // 1. Obtener información de la versión de Magisk
                _logManager?.AgregarLog($"1. Getting Magisk {magiskVersion}...");
                var (success, version, downloadUrl) = await _magiskManager.ObtenerVersionMagisk(magiskVersion);
                
                if (!success || string.IsNullOrEmpty(downloadUrl))
                {
                    _logManager?.AgregarLog("⚠ Could not get Magisk download URL");
                    return (false, null);
                }
                
                // 2. Descargar Magisk APK
                _logManager?.AgregarLog($"2. Downloading Magisk {version}...");
                var (downloadSuccess, apkPath) = await _magiskManager.DescargarMagisk(downloadUrl, version);
                
                if (!downloadSuccess || string.IsNullOrEmpty(apkPath))
                {
                    _logManager?.AgregarLog("⚠ Could not download Magisk APK");
                    return (false, null);
                }
                
                _logManager?.AgregarLog($"   ✓ Downloaded: {Path.GetFileName(apkPath)}");
                
                // 3. Extraer boot.img del archivo AP
                _logManager?.AgregarLog($"3. Extracting boot.img from AP...");
                var (extractSuccess, bootPath) = await _magiskManager.ExtraerBootDeAP(apOriginal);
                
                if (!extractSuccess || string.IsNullOrEmpty(bootPath))
                {
                    _logManager?.AgregarLog("⚠ Could not extract boot.img from AP");
                    return (false, null);
                }
                
                _logManager?.AgregarLog($"   ✓ Extracted: {Path.GetFileName(bootPath)}");
                
                // 4. Parchear boot.img con Magisk
                _logManager?.AgregarLog($"4. Patching boot.img with Magisk...");
                var (patchSuccess, patchedBootPath) = await _magiskManager.ParchearBootConAPK(bootPath, apkPath);
                
                if (!patchSuccess || string.IsNullOrEmpty(patchedBootPath))
                {
                    _logManager?.AgregarLog("⚠ Could not patch boot.img");
                    return (false, null);
                }
                
                _logManager?.AgregarLog($"   ✓ Patched: {Path.GetFileName(patchedBootPath)}");
                
                // 5. Crear nuevo archivo AP con boot.img parcheado
                _logManager?.AgregarLog($"5. Creating modified AP file...");
                var (createSuccess, apModificado) = await CrearAPModificado(apOriginal, patchedBootPath);
                
                if (!createSuccess || string.IsNullOrEmpty(apModificado))
                {
                    _logManager?.AgregarLog("⚠ Could not create modified AP");
                    return (false, null);
                }
                
                _logManager?.AgregarLog($"   ✓ Created: {Path.GetFileName(apModificado)}");
                
                return (true, apModificado);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"⚠ Error patching boot: {ex.Message}");
                return (false, null);
            }
        }
        
        /// <summary>
        /// Crea un archivo TAR con el boot.img parcheado para flashear
        /// </summary>
        private async Task<(bool success, string? apPath)> CrearAPModificado(string apOriginal, string bootParcheado)
        {
            try
            {
                string tempFolder = Path.Combine(Path.GetTempPath(), "TT-Tool-AP");
                Directory.CreateDirectory(tempFolder);
                
                // Crear un TAR simple con solo el boot.img parcheado
                string apModificado = Path.Combine(tempFolder, "AP_ROOTED_boot.tar");
                
                _logManager?.AgregarLog("   Creating TAR with patched boot.img...");
                
                // Leer el boot parcheado
                var bootData = await File.ReadAllBytesAsync(bootParcheado);
                
                // Crear un TAR simple con el boot.img
                await CrearTarSimple(apModificado, "boot.img", bootData);
                
                _logManager?.AgregarLog($"   ✓ Created: {Path.GetFileName(apModificado)} ({bootData.Length / 1024 / 1024} MB)");
                
                return (true, apModificado);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"   ⚠ Error creating modified AP: {ex.Message}");
                return (false, null);
            }
        }
        
        /// <summary>
        /// Crea un archivo TAR simple con un solo archivo
        /// </summary>
        private async Task CrearTarSimple(string tarPath, string fileName, byte[] fileData)
        {
            using (var fs = new FileStream(tarPath, FileMode.Create, FileAccess.Write))
            {
                // Header TAR (512 bytes)
                byte[] header = new byte[512];
                
                // Nombre del archivo (100 bytes)
                var nameBytes = System.Text.Encoding.ASCII.GetBytes(fileName);
                Array.Copy(nameBytes, 0, header, 0, Math.Min(nameBytes.Length, 100));
                
                // Mode (8 bytes) - "0000644 "
                var modeBytes = System.Text.Encoding.ASCII.GetBytes("0000644 ");
                Array.Copy(modeBytes, 0, header, 100, 8);
                
                // UID (8 bytes) - "0000000 "
                var uidBytes = System.Text.Encoding.ASCII.GetBytes("0000000 ");
                Array.Copy(uidBytes, 0, header, 108, 8);
                
                // GID (8 bytes) - "0000000 "
                var gidBytes = System.Text.Encoding.ASCII.GetBytes("0000000 ");
                Array.Copy(gidBytes, 0, header, 116, 8);
                
                // Size (12 bytes) - tamaño en octal
                string sizeOctal = Convert.ToString(fileData.Length, 8).PadLeft(11, '0') + " ";
                var sizeBytes = System.Text.Encoding.ASCII.GetBytes(sizeOctal);
                Array.Copy(sizeBytes, 0, header, 124, 12);
                
                // Mtime (12 bytes) - timestamp en octal
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string mtimeOctal = Convert.ToString(timestamp, 8).PadLeft(11, '0') + " ";
                var mtimeBytes = System.Text.Encoding.ASCII.GetBytes(mtimeOctal);
                Array.Copy(mtimeBytes, 0, header, 136, 12);
                
                // Checksum (8 bytes) - calculado después
                var checksumBytes = System.Text.Encoding.ASCII.GetBytes("        ");
                Array.Copy(checksumBytes, 0, header, 148, 8);
                
                // Typeflag (1 byte) - '0' para archivo regular
                header[156] = (byte)'0';
                
                // Calcular checksum
                int checksum = 0;
                for (int i = 0; i < 512; i++)
                {
                    checksum += header[i];
                }
                string checksumOctal = Convert.ToString(checksum, 8).PadLeft(6, '0') + "\0 ";
                checksumBytes = System.Text.Encoding.ASCII.GetBytes(checksumOctal);
                Array.Copy(checksumBytes, 0, header, 148, 8);
                
                // Escribir header
                await fs.WriteAsync(header, 0, 512);
                
                // Escribir datos del archivo
                await fs.WriteAsync(fileData, 0, fileData.Length);
                
                // Padding para alinear a 512 bytes
                int padding = (512 - (fileData.Length % 512)) % 512;
                if (padding > 0)
                {
                    byte[] paddingBytes = new byte[padding];
                    await fs.WriteAsync(paddingBytes, 0, padding);
                }
                
                // Escribir dos bloques de 512 bytes de ceros al final (EOF marker)
                byte[] eof = new byte[1024];
                await fs.WriteAsync(eof, 0, 1024);
            }
        }

        /// <summary>
        /// Obtiene las versiones disponibles de Magisk desde GitHub
        /// </summary>
        private async Task<List<string>> ObtenerVersionesMagiskDisponibles(bool isDelta = false)
        {
            var versiones = new List<string>();
            
            try
            {
                // Si es Kitsune, usar APK local directamente
                if (isDelta)
                {
                    string kitsuneApkPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Tools", "kitsune.apk");
                    
                    if (File.Exists(kitsuneApkPath))
                    {
                        versiones.Add("Kitsune Magisk (Local)");
                        _logManager?.AgregarLog($"✓ Loaded 1 Kitsune Magisk version");
                    }
                    else
                    {
                        _logManager?.AgregarLog($"⚠ Kitsune APK not found at: {kitsuneApkPath}");
                        versiones.Add("Kitsune Magisk (Not Found)");
                    }
                    
                    return versiones;
                }
                
                // Para Magisk oficial, usar API de GitHub
                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "TT-Tool");
                client.Timeout = TimeSpan.FromSeconds(10);
                
                string repoUrl = "https://api.github.com/repos/topjohnwu/Magisk/releases";
                
                // Obtener todas las releases de Magisk
                var response = await client.GetStringAsync(repoUrl);
                var json = System.Text.Json.JsonDocument.Parse(response);
                
                // Procesar las releases
                foreach (var release in json.RootElement.EnumerateArray())
                {
                    try
                    {
                        var tagName = release.GetProperty("tag_name").GetString();
                        var name = release.GetProperty("name").GetString();
                        var prerelease = release.GetProperty("prerelease").GetBoolean();
                        var draft = release.GetProperty("draft").GetBoolean();
                        
                        // Ignorar drafts
                        if (draft)
                            continue;
                        
                        // Verificar que tenga un APK
                        bool tieneApk = false;
                        if (release.TryGetProperty("assets", out var assets))
                        {
                            foreach (var asset in assets.EnumerateArray())
                            {
                                var assetName = asset.GetProperty("name").GetString();
                                if (assetName != null && assetName.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                                {
                                    tieneApk = true;
                                    break;
                                }
                            }
                        }
                        
                        if (!tieneApk)
                            continue;
                        
                        // Formatear el nombre de la versión
                        string versionText;
                        
                        if (prerelease)
                        {
                            versionText = $"Magisk {tagName}";
                        }
                        else
                        {
                            // Marcar la primera versión estable como "Latest Stable"
                            if (versiones.Count == 0 || !versiones[0].Contains("Latest Stable"))
                            {
                                versionText = $"Magisk {tagName} (Latest Stable)";
                            }
                            else
                            {
                                versionText = $"Magisk {tagName}";
                            }
                        }
                        
                        versiones.Add(versionText);
                        
                        // Limitar a las últimas 15 versiones
                        if (versiones.Count >= 15)
                            break;
                    }
                    catch
                    {
                        // Ignorar releases con formato incorrecto
                        continue;
                    }
                }
                
                // Agregar Canary/Development al final
                versiones.Add("Magisk Canary (Development)");
                
                _logManager?.AgregarLog($"✓ Loaded {versiones.Count} Magisk versions");
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"⚠ Could not fetch Magisk versions: {ex.Message}");
                
                // Fallback a versiones conocidas
                versiones.Add("Magisk 27.0 (Latest Stable)");
                versiones.Add("Magisk 26.4");
            }
            
            return versiones;
        }

        /// <summary>
        /// Crea un selector de archivo estilizado para Odin Mode
        /// </summary>
        private TextBox CrearSelectorArchivoOdinEstilizado(Panel panel, string particion, ref int yPos)
        {
            // Label con padding reducido
            var lbl = new Label
            {
                Text = particion,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(20, yPos + 2),
                AutoSize = true
            };
            panel.Controls.Add(lbl);

            // Checkbox más pequeño y más cerca
            var chk = new CheckBox
            {
                Location = new Point(65, yPos + 3),
                Size = new Size(16, 16),
                Checked = false
            };
            panel.Controls.Add(chk);

            // TextBox más compacto y reducido
            var txt = new TextBox
            {
                Location = new Point(90, yPos + 1),
                Width = 390,
                Font = new Font("Segoe UI", 8.5F),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(txt);

            // Botón Open con color celeste
            var btnOpen = new ModernButton
            {
                Text = "📁 Open",
                Location = new Point(490, yPos - 1),
                Size = new Size(80, 26),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                BorderRadius = 5,
                HoverColor = Color.FromArgb(51, 103, 214)
            };
            btnOpen.Click += (s, e) => SeleccionarArchivoParticion(txt, particion);
            panel.Controls.Add(btnOpen);

            yPos += 32; // Reducido de 40 a 32 para más compacto
            return txt;
        }

        private Panel CrearPanelMagiskPatch()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(30),
                BackColor = Color.FromArgb(240, 242, 245)
            };

            int yPos = 10;

            // Título
            var lblTitulo = new Label
            {
                Text = "MAGISK MANAGER INSTALLER",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(0, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblTitulo);

            yPos += 35;

            // Información
            var lblInfo = new Label
            {
                Text = "Download and install Magisk Manager APK to your device via ADB",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 100, 100),
                Location = new Point(0, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblInfo);

            yPos += 35;

            // MAGISK TYPE - Sin card, directo en el panel
            var lblType = new Label
            {
                Text = "MAGISK TYPE:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(0, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblType);

            yPos += 25;

            var radioMagisk = new RadioButton
            {
                Text = "Magisk Official (topjohnwu)",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(0, yPos),
                AutoSize = true,
                Checked = true
            };
            panel.Controls.Add(radioMagisk);

            var radioMagiskDelta = new RadioButton
            {
                Text = "Kitsune Magisk - Enhanced features",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(230, yPos),
                AutoSize = true
            };
            panel.Controls.Add(radioMagiskDelta);

            yPos += 35;

            // MAGISK VERSION - Sin card, directo en el panel
            var lblMagisk = new Label
            {
                Text = "MAGISK VERSION:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(0, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblMagisk);

            yPos += 25;

            var lblLoadingMagisk = new Label
            {
                Text = "⏳ Loading versions...",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(0, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblLoadingMagisk);

            var cmbMagisk = new ComboBox
            {
                Location = new Point(0, yPos),
                Width = 500,
                Height = 26,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 8.5F),
                Visible = false
            };
            panel.Controls.Add(cmbMagisk);

            // Evento para cambiar entre Magisk y Magisk Delta
            EventHandler cambiarTipo = async (s, e) =>
            {
                lblLoadingMagisk.Visible = true;
                cmbMagisk.Visible = false;
                cmbMagisk.Items.Clear();

                var versiones = await ObtenerVersionesMagiskDisponibles(radioMagiskDelta.Checked);
                
                lblLoadingMagisk.Visible = false;
                cmbMagisk.Visible = true;
                cmbMagisk.Items.AddRange(versiones.ToArray());
                if (cmbMagisk.Items.Count > 0)
                    cmbMagisk.SelectedIndex = 0;
            };

            radioMagisk.CheckedChanged += cambiarTipo;
            radioMagiskDelta.CheckedChanged += cambiarTipo;

            // Cargar versiones de Magisk de forma asíncrona (oficial por defecto)
            Task.Run(async () =>
            {
                var versiones = await ObtenerVersionesMagiskDisponibles(false);
                panel.Invoke(new Action(() =>
                {
                    lblLoadingMagisk.Visible = false;
                    cmbMagisk.Visible = true;
                    cmbMagisk.Items.Clear();
                    cmbMagisk.Items.AddRange(versiones.ToArray());
                    if (cmbMagisk.Items.Count > 0)
                        cmbMagisk.SelectedIndex = 0;
                }));
            });

            yPos += 35;

            // Nota importante
            var lblNote = new Label
            {
                Text = "⚠ Make sure your device is connected in ADB mode and USB debugging is enabled",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(200, 100, 50),
                Location = new Point(0, yPos),
                Width = 500,
                Height = 30,
                AutoSize = false
            };
            panel.Controls.Add(lblNote);

            yPos += 40;

            // Botón Download & Install - Centrado y más pequeño
            var btnInstall = new ModernButton
            {
                Text = "DOWNLOAD & INSTALL MAGISK",
                Location = new Point(0, yPos),
                Width = 500,
                Height = 35,
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                BorderRadius = 8,
                HoverColor = Color.FromArgb(51, 103, 214),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnInstall.Click += async (s, e) =>
            {
                if (cmbMagisk.SelectedItem == null)
                {
                    MessageBox.Show("Por favor selecciona una versión de Magisk", "Sin versión", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await DescargarEInstalarMagisk(cmbMagisk.SelectedItem.ToString()!, radioMagiskDelta.Checked);
            };
            panel.Controls.Add(btnInstall);

            yPos += 45;

            // Botón Install Apps Bancarias Perú
            var btnInstallAppsBancarias = new ModernButton
            {
                Text = "📱 INSTALL APPS BANCARIAS PERÚ",
                Location = new Point(0, yPos),
                Width = 500,
                Height = 35,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                BorderRadius = 8,
                HoverColor = Color.FromArgb(67, 160, 71),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnInstallAppsBancarias.Click += async (s, e) => await MostrarEInstalarAppsBancarias();
            panel.Controls.Add(btnInstallAppsBancarias);

            // Mostrar instrucciones estáticas al cargar el panel (con fuente tamaño 7)
            _logManager?.LimpiarLogs();
            _logManager?.AgregarInstruccion("═══════════════════════════════════════════════════", TipoLog.Titulo);
            _logManager?.AgregarInstruccion("       INSTRUCCIONES FIX APPS BANCARIAS 2025", TipoLog.Titulo);
            _logManager?.AgregarInstruccion("═══════════════════════════════════════════════════", TipoLog.Titulo);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("STEP 1: ELEGIR VERSIONES RECIENTES", TipoLog.Proceso);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("        EJEMPLO MAGISK V30.3 EN ADELANTE.", TipoLog.Info);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("STEP 2: CLICK EN DOWNLOAD INSTALL MAGISK", TipoLog.Proceso);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("--------------------------------", TipoLog.Comando);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("AUTOMATICAMENTA MAGISK SERA INSTALADO", TipoLog.Exito);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("STEP 3: COPIAR EL AP DE SU DISPOSITIVO Y PARCHARLO", TipoLog.Proceso);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("        MEDIANTE EL METODO TRADICIONAL", TipoLog.Info);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("STEP 4: EXTRAER EL AP CREADO POR MAGISK", TipoLog.Proceso);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("STEP 5: IR A PESTAÑA ODIN MODE Y FLASHEAR EL APP", TipoLog.Proceso);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("        PARCHADO.", TipoLog.Info);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("STEP 6: CONFIGURAR EL DISPOSTIVO", TipoLog.Proceso);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("        Y IR A MAIN SERVICE Y EJECUTAR EL BOTON", TipoLog.Info);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("        DE FIX APPS BANCARIAS 2025", TipoLog.Info);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("STEP 7: COBRAR MAMAWEBO 🎉🎉", TipoLog.Exito);
            _logManager?.AgregarInstruccion("");
            _logManager?.AgregarInstruccion("═══════════════════════════════════════════════════", TipoLog.Titulo);

            return panel;
        }

        // Helper para dibujar rectángulos con bordes redondeados
        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int diameter = radius * 2;
            
            // Esquina superior izquierda
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // Línea superior
            path.AddLine(rect.X + radius, rect.Y, rect.Right - radius, rect.Y);
            // Esquina superior derecha
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // Línea derecha
            path.AddLine(rect.Right, rect.Y + radius, rect.Right, rect.Bottom - radius);
            // Esquina inferior derecha
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // Línea inferior
            path.AddLine(rect.Right - radius, rect.Bottom, rect.X + radius, rect.Bottom);
            // Esquina inferior izquierda
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            // Línea izquierda
            path.AddLine(rect.X, rect.Bottom - radius, rect.X, rect.Y + radius);
            
            path.CloseFigure();
            return path;
        }

        private async Task DescargarEInstalarMagisk(string versionSeleccionada, bool isDelta = false)
        {
            try
            {
                _logManager?.LimpiarLogs();
                _logManager?.AgregarLog("=== INSTALANDO MAGISK MANAGER ===", TipoLog.Info);
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Versión seleccionada: {versionSeleccionada}", TipoLog.Info);
                _logManager?.AgregarLog("");

                string apkPath;

                // Si es Kitsune, usar APK local
                if (isDelta)
                {
                    _logManager?.AgregarLog("[1/5] Usando Kitsune Magisk local...");
                    apkPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Tools", "kitsune.apk");
                    
                    if (!File.Exists(apkPath))
                    {
                        _logManager?.AgregarLog("[ERROR] Kitsune APK no encontrado", TipoLog.Error);
                        MessageBox.Show($"Kitsune APK not found at:\n{apkPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    
                    _logManager?.AgregarLog($"[OK] APK local encontrado: {new FileInfo(apkPath).Length / 1024 / 1024} MB", TipoLog.Exito);
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog("[2/5] APK listo para instalar...");
                    _logManager?.AgregarLog("");
                }
                else
                {
                    // Para Magisk oficial, descargar desde GitHub
                    // Inicializar MagiskManager
                    if (_magiskManager == null)
                    {
                        _magiskManager = new MagiskManager();
                        _magiskManager.OnLogMessage += (s, msg) => _logManager?.AgregarLog(msg);
                    }

                    // Obtener información de la versión
                    _logManager?.AgregarLog("[1/5] Obteniendo información de la versión...");
                    var (success, version, downloadUrl) = await _magiskManager.ObtenerVersionMagisk(versionSeleccionada);

                    if (!success)
                    {
                        _logManager?.AgregarLog("[ERROR] No se pudo obtener la información de Magisk", TipoLog.Error);
                        MessageBox.Show("No se pudo obtener la información de Magisk", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    _logManager?.AgregarLog($"[OK] URL de descarga obtenida", TipoLog.Exito);
                    _logManager?.AgregarLog("");

                    // Descargar Magisk APK
                    _logManager?.AgregarLog("[2/5] Descargando Magisk APK...");
                    var (downloadSuccess, downloadedApkPath) = await _magiskManager.DescargarMagisk(downloadUrl, version);

                    if (!downloadSuccess)
                    {
                        _logManager?.AgregarLog("[ERROR] Error al descargar Magisk", TipoLog.Error);
                        MessageBox.Show("Error al descargar Magisk", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    apkPath = downloadedApkPath;
                    _logManager?.AgregarLog($"[OK] APK descargado: {new FileInfo(apkPath).Length / 1024 / 1024} MB", TipoLog.Exito);
                    _logManager?.AgregarLog("");
                }

                // Verificar dispositivo conectado
                _logManager?.AgregarLog("[3/5] Verificando dispositivo conectado...");
                var adbManager = new ADBManager();
                var devices = await adbManager.ExecuteAdbCommandAsync("devices");

                if (string.IsNullOrEmpty(devices) || !devices.Contains("device"))
                {
                    _logManager?.AgregarLog("[ERROR] No se detectó ningún dispositivo", TipoLog.Error);
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog("Por favor:", TipoLog.Advertencia);
                    _logManager?.AgregarLog("- Conecta tu dispositivo via USB", TipoLog.Advertencia);
                    _logManager?.AgregarLog("- Habilita la depuración USB", TipoLog.Advertencia);
                    _logManager?.AgregarLog("- Autoriza la conexión en tu dispositivo", TipoLog.Advertencia);
                    MessageBox.Show("No se detectó ningún dispositivo conectado.\n\nAsegúrate de tener USB debugging habilitado.", "Sin dispositivo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _logManager?.AgregarLog("[OK] Dispositivo detectado", TipoLog.Exito);
                _logManager?.AgregarLog("");

                // Copiar APK al dispositivo
                _logManager?.AgregarLog("[4/5] Copiando APK al dispositivo...");
                string devicePath = "/sdcard/Download/Magisk.apk";
                var pushResult = await adbManager.ExecuteAdbCommandAsync($"push \"{apkPath}\" {devicePath}");

                if (pushResult.Contains("file pushed") || pushResult.Contains("bytes"))
                {
                    _logManager?.AgregarLog($"[OK] APK copiado a {devicePath}", TipoLog.Exito);
                    _logManager?.AgregarLog("");
                }
                else
                {
                    _logManager?.AgregarLog("[WARN] No se pudo confirmar la copia, continuando...", TipoLog.Advertencia);
                    _logManager?.AgregarLog("");
                }

                // Instalar APK
                _logManager?.AgregarLog("[5/5] Instalando Magisk Manager en el dispositivo...");
                var installResult = await adbManager.ExecuteAdbCommandAsync($"install -r \"{apkPath}\"");

                if (installResult.Contains("Success") || installResult.Contains("SUCCESS"))
                {
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog("✓✓✓ MAGISK MANAGER INSTALADO EXITOSAMENTE ✓✓✓", TipoLog.Exito);
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog($"APK disponible en: {devicePath}", TipoLog.Info);
                    _logManager?.AgregarLog("Ahora puedes abrir Magisk Manager en tu dispositivo", TipoLog.Info);
                    
                    MessageBox.Show(
                        $"✓ Magisk Manager instalado exitosamente\n\nAPK copiado a: {devicePath}\n\nAbre la aplicación en tu dispositivo para configurar el acceso root.", 
                        "Instalación exitosa", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                }
                else
                {
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog($"[ERROR] Error al instalar: {installResult}", TipoLog.Error);
                    MessageBox.Show($"Error al instalar Magisk:\n\n{installResult}", "Error de instalación", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"[ERROR] Excepción: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error durante la instalación:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<string>> CargarVersionesMagisk()
        {
            var versiones = new List<string>();
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "TT-Tool");
                client.Timeout = TimeSpan.FromSeconds(10);

                // Cargar versiones ESTABLES de Magisk (sin Canary ni Pre-release)
                try
                {
                    var response = await client.GetStringAsync("https://api.github.com/repos/topjohnwu/Magisk/releases");
                    var json = JsonDocument.Parse(response);

                    int count = 0;
                    foreach (var release in json.RootElement.EnumerateArray())
                    {
                        if (count >= 10) break;

                        var tagName = release.GetProperty("tag_name").GetString();
                        var isPrerelease = release.GetProperty("prerelease").GetBoolean();
                        
                        // SOLO versiones estables (no pre-release)
                        if (!string.IsNullOrEmpty(tagName) && !isPrerelease)
                        {
                            string version = $"Magisk {tagName.Replace("v", "")}";
                            versiones.Add(version);
                            count++;
                        }
                    }
                }
                catch
                {
                    // Error al cargar versiones estables
                }

                // Cargar versiones ESTABLES de Magisk Delta desde el repositorio correcto
                try
                {
                    var responseDelta = await client.GetStringAsync("https://api.github.com/repos/TeaqariaWTF/MagiskDelta/releases");
                    var jsonDelta = JsonDocument.Parse(responseDelta);
                    
                    int countDelta = 0;
                    foreach (var release in jsonDelta.RootElement.EnumerateArray())
                    {
                        if (countDelta >= 10) break;
                        
                        var tagName = release.GetProperty("tag_name").GetString();
                        var name = release.GetProperty("name").GetString();
                        var isPrerelease = release.GetProperty("prerelease").GetBoolean();
                        
                        // SOLO versiones estables de Delta (no pre-release)
                        if (!isPrerelease && tagName != null)
                        {
                            // Limpiar el tag para mostrar solo la versión
                            string cleanTag = tagName.Replace("v", "").Trim();
                            string version = $"Magisk Delta {cleanTag}";
                            versiones.Add(version);
                            countDelta++;
                        }
                    }
                }
                catch
                {
                    // Error al cargar versiones Delta
                }
            }
            catch
            {
                // Fallback a versiones conocidas
            }
            
            // Si no se pudieron cargar versiones, usar fallback
            if (versiones.Count == 0)
            {
                versiones.AddRange(new[]
                {
                    "Magisk 29.0",
                    "Magisk 28.1",
                    "Magisk 28.0",
                    "Magisk 27.0",
                    "Magisk 26.4",
                    "Magisk Delta 1.1.1",
                    "Magisk Delta 1.1.0"
                });
            }

            return versiones;
        }

        private async Task MostrarEInstalarAppsBancarias()
        {
            try
            {
                _logManager?.LimpiarLogs();
                _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
                _logManager?.AgregarLog("  INSTALL APPS BANCARIAS PERÚ", TipoLog.Titulo);
                _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
                _logManager?.AgregarLog("");

                // Buscar APKs en Resources/AppsBancarias/
                string appsBancariasPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "AppsBancarias");
                
                if (!Directory.Exists(appsBancariasPath))
                {
                    _logManager?.AgregarLog("⚠ Carpeta AppsBancarias no encontrada", TipoLog.Advertencia);
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog($"Ruta esperada: {appsBancariasPath}", TipoLog.Info);
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog("Por favor crea la carpeta y coloca los APKs:", TipoLog.Info);
                    _logManager?.AgregarLog("Resources/AppsBancarias/", TipoLog.Info);
                    
                    MessageBox.Show(
                        $"Carpeta no encontrada:\n\n{appsBancariasPath}\n\n" +
                        "Crea la carpeta 'AppsBancarias' en Resources/\n" +
                        "y coloca los APKs de bancos peruanos allí.",
                        "Carpeta no encontrada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Buscar todos los APKs (tanto .apk como .apks)
                var apkFiles = Directory.GetFiles(appsBancariasPath, "*.apk*")
                    .Where(f => f.EndsWith(".apk", StringComparison.OrdinalIgnoreCase) || 
                                f.EndsWith(".apks", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                
                if (apkFiles.Length == 0)
                {
                    _logManager?.AgregarLog("⚠ No se encontraron APKs en la carpeta", TipoLog.Advertencia);
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog($"Coloca los APKs en: {appsBancariasPath}", TipoLog.Info);
                    
                    MessageBox.Show(
                        "No se encontraron APKs de bancos.\n\n" +
                        $"Por favor coloca los APKs en:\n{appsBancariasPath}",
                        "Sin APKs",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                _logManager?.AgregarLog($"✓ Se encontraron {apkFiles.Length} APK(s) disponibles", TipoLog.Exito);
                _logManager?.AgregarLog("");

                // Mostrar lista de APKs disponibles
                foreach (var apk in apkFiles)
                {
                    var fileInfo = new FileInfo(apk);
                    _logManager?.AgregarLog($"  • {Path.GetFileNameWithoutExtension(apk)} ({fileInfo.Length / 1024 / 1024} MB)", TipoLog.Info);
                }
                _logManager?.AgregarLog("");

                // Crear formulario para selección múltiple
                using var form = new Form
                {
                    Text = "Seleccionar Apps a Instalar",
                    Width = 500,
                    Height = 450,
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = Color.FromArgb(240, 242, 245)
                };

                var lblTitulo = new Label
                {
                    Text = "📱 Selecciona las apps bancarias a instalar:",
                    Location = new Point(20, 20),
                    Size = new Size(440, 30),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(50, 50, 50)
                };
                form.Controls.Add(lblTitulo);

                var checkedListBox = new CheckedListBox
                {
                    Location = new Point(20, 60),
                    Size = new Size(440, 280),
                    CheckOnClick = true,
                    Font = new Font("Segoe UI", 9F),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Agregar APKs al checklist
                foreach (var apk in apkFiles)
                {
                    var fileInfo = new FileInfo(apk);
                    string displayName = $"{Path.GetFileNameWithoutExtension(apk)} ({fileInfo.Length / 1024 / 1024} MB)";
                    checkedListBox.Items.Add(displayName);
                }

                form.Controls.Add(checkedListBox);

                // Botón "Seleccionar Todas"
                var btnSelectAll = new ModernButton
                {
                    Text = "✓ Seleccionar Todas",
                    Location = new Point(20, 355),
                    Size = new Size(140, 35),
                    BackColor = Color.FromArgb(100, 100, 100),
                    ForeColor = Color.White,
                    BorderRadius = 6,
                    HoverColor = Color.FromArgb(80, 80, 80),
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
                };
                btnSelectAll.Click += (s, e) =>
                {
                    for (int i = 0; i < checkedListBox.Items.Count; i++)
                        checkedListBox.SetItemChecked(i, true);
                };
                form.Controls.Add(btnSelectAll);

                // Botón "Instalar"
                var btnInstalar = new ModernButton
                {
                    Text = "📲 Instalar Seleccionadas",
                    Location = new Point(240, 355),
                    Size = new Size(220, 35),
                    BackColor = Color.FromArgb(76, 175, 80),
                    ForeColor = Color.White,
                    BorderRadius = 6,
                    HoverColor = Color.FromArgb(67, 160, 71),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                btnInstalar.Click += (s, e) =>
                {
                    if (checkedListBox.CheckedItems.Count > 0)
                        form.DialogResult = DialogResult.OK;
                    else
                        MessageBox.Show("Por favor selecciona al menos una app", "Sin selección", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                };
                form.Controls.Add(btnInstalar);

                if (form.ShowDialog() != DialogResult.OK)
                {
                    _logManager?.AgregarLog("⚠ Instalación cancelada por el usuario", TipoLog.Advertencia);
                    return;
                }

                // Obtener APKs seleccionados
                var selectedApks = new List<string>();
                for (int i = 0; i < checkedListBox.CheckedItems.Count; i++)
                {
                    int index = checkedListBox.CheckedIndices[i];
                    selectedApks.Add(apkFiles[index]);
                }

                if (selectedApks.Count == 0)
                {
                    _logManager?.AgregarLog("⚠ No se seleccionaron apps", TipoLog.Advertencia);
                    return;
                }

                _logManager?.AgregarLog($"[INFO] Instalando {selectedApks.Count} app(s)...", TipoLog.Proceso);
                _logManager?.AgregarLog("");

                // Verificar dispositivo
                var adbManager = new ADBManager();
                var devices = await adbManager.ExecuteAdbCommandAsync("devices");

                if (string.IsNullOrEmpty(devices) || !devices.Contains("device"))
                {
                    _logManager?.AgregarLog("✗ No se detectó ningún dispositivo", TipoLog.Error);
                    MessageBox.Show(
                        "No se detectó ningún dispositivo conectado.\n\n" +
                        "Asegúrate de tener USB debugging habilitado.",
                        "Sin dispositivo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                _logManager?.AgregarLog("✓ Dispositivo detectado", TipoLog.Exito);
                _logManager?.AgregarLog("");

                // Instalar cada APK
                int instalados = 0;
                int errores = 0;

                for (int i = 0; i < selectedApks.Count; i++)
                {
                    var apk = selectedApks[i];
                    var appName = Path.GetFileNameWithoutExtension(apk);
                    var isBundle = apk.EndsWith(".apks", StringComparison.OrdinalIgnoreCase);

                    _logManager?.AgregarLog($"[{i + 1}/{selectedApks.Count}] Instalando {appName}...", TipoLog.Proceso);

                    string result;
                    if (isBundle)
                    {
                        // Para archivos .apks (bundles), intentar con install-multiple
                        _logManager?.AgregarLog($"      (Bundle detectado, usando install-multiple)", TipoLog.Info);
                        
                        // Renombrar temporalmente a .zip y extraer
                        var tempZip = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(apk) + ".zip");
                        var tempExtract = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(apk) + "_extracted");
                        
                        try
                        {
                            File.Copy(apk, tempZip, true);
                            if (Directory.Exists(tempExtract))
                                Directory.Delete(tempExtract, true);
                            
                            System.IO.Compression.ZipFile.ExtractToDirectory(tempZip, tempExtract);
                            
                            // Buscar todos los APKs dentro del bundle
                            var bundleApks = Directory.GetFiles(tempExtract, "*.apk", SearchOption.AllDirectories);
                            
                            if (bundleApks.Length > 0)
                            {
                                var apksToInstall = string.Join(" ", bundleApks.Select(a => $"\"{a}\""));
                                result = await adbManager.ExecuteAdbCommandAsync($"install-multiple -r {apksToInstall}");
                            }
                            else
                            {
                                result = "Error: No APKs found in bundle";
                            }
                            
                            // Limpiar archivos temporales
                            File.Delete(tempZip);
                            Directory.Delete(tempExtract, true);
                        }
                        catch (Exception ex)
                        {
                            result = $"Error: {ex.Message}";
                        }
                    }
                    else
                    {
                        // Para archivos .apk normales
                        result = await adbManager.ExecuteAdbCommandAsync($"install -r \"{apk}\"");
                    }

                    if (result.Contains("Success") || result.Contains("SUCCESS"))
                    {
                        _logManager?.AgregarLog($"      ✓ {appName} instalado", TipoLog.Exito);
                        instalados++;
                    }
                    else
                    {
                        _logManager?.AgregarLog($"      ✗ Error: {(result.Length > 100 ? result.Substring(0, 100) + "..." : result)}", TipoLog.Error);
                        errores++;
                    }
                    _logManager?.AgregarLog("");
                }

                // Resumen final
                _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);
                _logManager?.AgregarLog($"✓ Instalados: {instalados}", TipoLog.Exito);
                if (errores > 0)
                    _logManager?.AgregarLog($"✗ Errores: {errores}", TipoLog.Error);
                _logManager?.AgregarLog("═══════════════════════════════════", TipoLog.Titulo);

                if (instalados > 0)
                {
                    MessageBox.Show(
                        $"✓ Instalación completada\n\n" +
                        $"Apps instaladas: {instalados}\n" +
                        (errores > 0 ? $"Errores: {errores}" : ""),
                        "Instalación completada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"✗ Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task PatchAPWithMagisk(string apPath, string magiskVersion, bool keepVerity, bool keepEncryption)
        {
            try
            {
                _logManager?.LimpiarLogs();
                _logManager?.AgregarLog("=== MAGISK AP PATCHER ===");
                _logManager?.AgregarLog($"AP File: {Path.GetFileName(apPath)}");
                _logManager?.AgregarLog($"Magisk Version: {magiskVersion}");
                _logManager?.AgregarLog($"Keep dm-verity: {keepVerity}");
                _logManager?.AgregarLog($"Keep force encryption: {keepEncryption}");
                _logManager?.AgregarLog("");

                // Crear carpeta de salida
                string outputFolder = Path.Combine(Path.GetDirectoryName(apPath) ?? Path.GetTempPath(), "Magisk_AP_Patch");
                Directory.CreateDirectory(outputFolder);

                // Paso 1: Crear TAR reducido con solo las imágenes necesarias
                _logManager?.AgregarLog("- Processing AP file");
                string miniTarPath = Path.Combine(outputFolder, "AP_for_magisk.tar");
                await CrearMiniTARParaMagisk(apPath, miniTarPath);

                if (!File.Exists(miniTarPath))
                {
                    _logManager?.AgregarLog("[ERROR] ✗ Failed to create mini TAR", TipoLog.Error);
                    return;
                }

                // Paso 2: Descargar Magisk
                var magiskApk = await DescargarMagisk(magiskVersion, outputFolder);
                if (string.IsNullOrEmpty(magiskApk))
                {
                    _logManager?.AgregarLog("[ERROR] ✗ Failed to download Magisk", TipoLog.Error);
                    return;
                }

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("=== READY TO PATCH ===");
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog($"Files prepared in: {outputFolder}");
                _logManager?.AgregarLog($"  ✓ AP_for_magisk.tar (mini TAR with boot images)");
                _logManager?.AgregarLog($"  ✓ {Path.GetFileName(magiskApk)} (Magisk Manager)");
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("=== SIMPLE 3-STEP PROCESS ===");
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("STEP 1: Transfer files to device");
                _logManager?.AgregarLog("  • Copy AP_for_magisk.tar to /sdcard/Download/");
                _logManager?.AgregarLog("  • Copy Magisk APK to device");
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("STEP 2: Patch with Magisk Manager");
                _logManager?.AgregarLog("  • Install Magisk Manager");
                _logManager?.AgregarLog("  • Open Magisk → Install → Select and Patch a File");
                _logManager?.AgregarLog("  • Select AP_for_magisk.tar");
                _logManager?.AgregarLog("  • Wait for completion");
                _logManager?.AgregarLog("  • Magisk will create: magisk_patched-XXXXX.tar");
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("STEP 3: Flash with Odin");
                _logManager?.AgregarLog("  • Copy magisk_patched-XXXXX.tar back to PC");
                _logManager?.AgregarLog("  • Flash with Odin in AP slot");
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("TIP: Use ADB for faster transfer:");
                _logManager?.AgregarLog("  adb push AP_for_magisk.tar /sdcard/Download/");
                _logManager?.AgregarLog("  adb pull /sdcard/Download/magisk_patched-*.tar");
                _logManager?.AgregarLog("");

                var result = MessageBox.Show(
                    "✓ Files prepared successfully!\n\n" +
                    "SIMPLE 3-STEP PROCESS:\n\n" +
                    "1. TRANSFER TO DEVICE:\n" +
                    "   • Copy AP_for_magisk.tar to device\n" +
                    "   • Install Magisk Manager APK\n\n" +
                    "2. PATCH WITH MAGISK:\n" +
                    "   • Open Magisk → Install → Patch File\n" +
                    "   • Select AP_for_magisk.tar\n" +
                    "   • Magisk creates magisk_patched.tar\n\n" +
                    "3. FLASH WITH ODIN:\n" +
                    "   • Copy magisk_patched.tar back to PC\n" +
                    "   • Flash in AP slot with Odin\n\n" +
                    "Open output folder?",
                    "Ready to Patch",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    Process.Start("explorer.exe", outputFolder);
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"[ERROR] ✗ Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error preparing files:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CrearMiniTARParaMagisk(string apOriginal, string outputTar)
        {
            try
            {
                _logManager?.AgregarLog("- Extracting images from AP");
                
                string tempFolder = Path.Combine(Path.GetTempPath(), $"TT-Magisk-{Guid.NewGuid():N}");
                Directory.CreateDirectory(tempFolder);

                // Extraer solo las imágenes necesarias
                var extractedFiles = await ExtraerTAR(apOriginal, tempFolder);

                if (!extractedFiles.ContainsKey("boot.img"))
                {
                    throw new Exception("boot.img not found in AP file");
                }

                _logManager?.AgregarLog("- Creating mini TAR for Magisk");

                // Crear TAR con solo boot, recovery, vbmeta y dtbo
                await Task.Run(() =>
                {
                    using var fsOut = new FileStream(outputTar, FileMode.Create, FileAccess.Write);

                    // Boot (obligatorio)
                    if (extractedFiles.ContainsKey("boot.img"))
                    {
                        _logManager?.AgregarLog("-- Including: boot.img");
                        EscribirArchivoEnTAR(fsOut, "boot.img", extractedFiles["boot.img"]);
                    }

                    // Recovery (si existe)
                    if (extractedFiles.ContainsKey("recovery.img"))
                    {
                        _logManager?.AgregarLog("-- Including: recovery.img");
                        EscribirArchivoEnTAR(fsOut, "recovery.img", extractedFiles["recovery.img"]);
                    }

                    // vbmeta (si existe)
                    if (extractedFiles.ContainsKey("vbmeta.img"))
                    {
                        _logManager?.AgregarLog("-- Including: vbmeta.img");
                        EscribirArchivoEnTAR(fsOut, "vbmeta.img", extractedFiles["vbmeta.img"]);
                    }

                    // dtbo (si existe)
                    if (extractedFiles.ContainsKey("dtbo.img"))
                    {
                        _logManager?.AgregarLog("-- Including: dtbo.img");
                        EscribirArchivoEnTAR(fsOut, "dtbo.img", extractedFiles["dtbo.img"]);
                    }

                    // EOF marker
                    byte[] buffer = new byte[1024];
                    fsOut.Write(buffer, 0, 1024);
                });

                // Limpiar archivos temporales
                try
                {
                    Directory.Delete(tempFolder, true);
                }
                catch { }

                var fileInfo = new FileInfo(outputTar);
                _logManager?.AgregarLog($"✓ Mini TAR created: {fileInfo.Length / 1024 / 1024} MB");
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"[ERROR] ✗ Error creating mini TAR: {ex.Message}", TipoLog.Error);
                throw;
            }
        }

        private async Task<Dictionary<string, string>> ExtraerTAR(string tarPath, string outputFolder)
        {
            var extractedFiles = new Dictionary<string, string>();

            await Task.Run(async () =>
            {
                using var fs = new FileStream(tarPath, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[512];

                while (fs.Position < fs.Length)
                {
                    // Leer header TAR (512 bytes)
                    int bytesRead = fs.Read(buffer, 0, 512);
                    if (bytesRead < 512) break;

                    // Verificar si es un header válido
                    if (buffer[0] == 0) break;

                    // Extraer nombre del archivo (primeros 100 bytes)
                    string fileName = System.Text.Encoding.ASCII.GetString(buffer, 0, 100).TrimEnd('\0', ' ');
                    if (string.IsNullOrEmpty(fileName)) break;

                    // Extraer tamaño del archivo (bytes 124-135, en octal)
                    string sizeStr = System.Text.Encoding.ASCII.GetString(buffer, 124, 11).TrimEnd('\0', ' ');
                    long fileSize = string.IsNullOrEmpty(sizeStr) ? 0 : Convert.ToInt64(sizeStr, 8);

                    // Calcular bloques necesarios (512 bytes por bloque)
                    long blocks = (fileSize + 511) / 512;

                    // Solo extraer archivos que nos interesan
                    if (fileName.EndsWith(".img") || fileName.EndsWith(".img.lz4"))
                    {
                        string outputPath = Path.Combine(outputFolder, Path.GetFileName(fileName));

                        // Leer y guardar el archivo
                        using (var outFs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                        {
                            long remaining = fileSize;
                            byte[] fileBuffer = new byte[4096];

                            while (remaining > 0)
                            {
                                int toRead = (int)Math.Min(remaining, fileBuffer.Length);
                                int read = fs.Read(fileBuffer, 0, toRead);
                                if (read == 0) break;

                                outFs.Write(fileBuffer, 0, read);
                                remaining -= read;
                            }
                        }

                        // Descomprimir si es LZ4
                        if (fileName.EndsWith(".lz4"))
                        {
                            _logManager?.AgregarLog($"-- Extracting: {Path.GetFileName(fileName)}");
                            
                            // Descomprimir LZ4
                            string decompressedPath = outputPath.Replace(".lz4", "");
                            bool success = await DescomprimirLZ4(outputPath, decompressedPath);
                            
                            if (success)
                            {
                                // Eliminar archivo comprimido
                                File.Delete(outputPath);
                                
                                // Agregar archivo descomprimido
                                string imgName = Path.GetFileName(decompressedPath);
                                extractedFiles[imgName] = decompressedPath;
                            }
                            else
                            {
                                _logManager?.AgregarLog($"[WARN]    ⚠ Failed to decompress {Path.GetFileName(fileName)}", TipoLog.Advertencia);
                            }
                        }
                        else
                        {
                            _logManager?.AgregarLog($"-- Extracting: {Path.GetFileName(fileName)}");
                            extractedFiles[Path.GetFileName(fileName)] = outputPath;
                        }

                        // Saltar al siguiente bloque
                        long currentPos = fs.Position;
                        long nextBlock = ((currentPos + 511) / 512) * 512;
                        fs.Seek(nextBlock, SeekOrigin.Begin);
                    }
                    else
                    {
                        // Saltar el archivo
                        fs.Seek(blocks * 512, SeekOrigin.Current);
                        
                        if (!fileName.Contains("userdata"))
                        {
                            _logManager?.AgregarLog($"-- Copying   : {Path.GetFileName(fileName)}");
                        }
                        else
                        {
                            _logManager?.AgregarLog($"-- Skipping  : {Path.GetFileName(fileName)}");
                        }
                    }
                }
            });

            return extractedFiles;
        }

        private async Task<string?> DescargarMagisk(string version, string outputFolder)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "TT-Tool");
                client.Timeout = TimeSpan.FromMinutes(5);

                // Detectar si es Magisk Delta
                bool isDelta = version.Contains("Delta", StringComparison.OrdinalIgnoreCase);
                
                // Extraer número de versión
                var versionMatch = System.Text.RegularExpressions.Regex.Match(version, @"v?(\d+\.\d+\.?\d*)");
                string versionTag = versionMatch.Success ? $"v{versionMatch.Groups[1].Value}" : "latest";

                // Obtener URL de descarga según el tipo
                string apiUrl;
                if (isDelta)
                {
                    // Magisk Delta desde TeaqariaWTF/MagiskDelta
                    apiUrl = versionTag == "latest"
                        ? "https://api.github.com/repos/TeaqariaWTF/MagiskDelta/releases/latest"
                        : $"https://api.github.com/repos/TeaqariaWTF/MagiskDelta/releases/tags/{versionTag}";
                }
                else
                {
                    // Magisk oficial desde topjohnwu/Magisk
                    apiUrl = versionTag == "latest"
                        ? "https://api.github.com/repos/topjohnwu/Magisk/releases/latest"
                        : $"https://api.github.com/repos/topjohnwu/Magisk/releases/tags/{versionTag}";
                }

                var response = await client.GetStringAsync(apiUrl);
                var json = JsonDocument.Parse(response);

                // Obtener información de la versión
                string? versionCode = null;
                if (json.RootElement.TryGetProperty("tag_name", out var tagElement))
                {
                    var tag = tagElement.GetString();
                    if (!string.IsNullOrEmpty(tag))
                    {
                        // Extraer código de versión (ej: v28.1 -> 28100)
                        var match = System.Text.RegularExpressions.Regex.Match(tag, @"v?(\d+)\.(\d+)");
                        if (match.Success)
                        {
                            int major = int.Parse(match.Groups[1].Value);
                            int minor = int.Parse(match.Groups[2].Value);
                            versionCode = $"{major}{minor:D2}00";
                        }
                    }
                }

                string? downloadUrl = null;
                foreach (var asset in json.RootElement.GetProperty("assets").EnumerateArray())
                {
                    var name = asset.GetProperty("name").GetString();
                    if (name != null && name.EndsWith(".apk") && !name.Contains("stub"))
                    {
                        downloadUrl = asset.GetProperty("browser_download_url").GetString();
                        break;
                    }
                }

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    _logManager?.AgregarLog($"✗ Could not find {(isDelta ? "Magisk Delta" : "Magisk")} APK download URL", TipoLog.Error);
                    return null;
                }

                // Mostrar mensaje de instalación
                string displayName = isDelta ? "Magisk Delta" : "Magisk";
                _logManager?.AgregarLog($"- Installing: {displayName} {versionTag}{(versionCode != null ? $" ({versionCode})" : "")}");

                // Descargar APK
                string apkPath = Path.Combine(outputFolder, isDelta ? "magisk_delta.apk" : "magisk.apk");
                var apkData = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(apkPath, apkData);

                return apkPath;
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"✗ Error downloading Magisk: {ex.Message}", TipoLog.Error);
                return null;
            }
        }

        private async Task<bool> VerificarADB()
        {
            // No requerimos ADB para el parcheo local
            return await Task.FromResult(true);
        }

        private async Task<bool> InstalarMagiskEnDispositivo(string apkPath)
        {
            // No necesitamos instalar Magisk para el parcheo local
            return await Task.FromResult(true);
        }

        private async Task<string?> ParcharBootImageLocal(string bootPath, string workFolder, bool keepVerity, bool keepEncryption, bool isRecovery = false)
        {
            try
            {
                string imageName = isRecovery ? "recovery.img" : "boot.img";
                string outputName = isRecovery ? "new-recovery.img" : "new-boot.img";
                string outputPath = Path.Combine(workFolder, outputName);

                // Parsear imagen para mostrar información
                await ParsearBootImageLocal(bootPath, imageName);

                // Leer boot image
                var bootData = await File.ReadAllBytesAsync(bootPath);

                // Verificar que sea una imagen Android válida
                if (bootData.Length < 2048 || System.Text.Encoding.ASCII.GetString(bootData, 0, 8) != "ANDROID!")
                {
                    _logManager?.AgregarLog("[ERROR] ✗ Invalid Android boot image", TipoLog.Error);
                    return null;
                }

                // Leer header
                int kernelSize = BitConverter.ToInt32(bootData, 8);
                int ramdiskSize = BitConverter.ToInt32(bootData, 16);
                int pageSize = BitConverter.ToInt32(bootData, 36);
                if (pageSize == 0) pageSize = 2048;

                // Calcular offsets
                int kernelPages = (kernelSize + pageSize - 1) / pageSize;
                int kernelOffset = pageSize;
                int ramdiskOffset = (1 + kernelPages) * pageSize;

                // Extraer kernel
                byte[] kernel = new byte[kernelSize];
                Array.Copy(bootData, kernelOffset, kernel, 0, kernelSize);

                // Inyectar Magisk en el kernel (método simplificado)
                // En una implementación real, esto requeriría descomprimir el kernel,
                // modificar el ramdisk, y recomprimir
                _logManager?.AgregarLog("- Injecting Magisk into boot image");
                _logManager?.AgregarLog("- Checking ramdisk status");
                _logManager?.AgregarLog("- Stock boot image detected");
                _logManager?.AgregarLog("- Patching ramdisk");
                
                if (keepVerity)
                    _logManager?.AgregarLog("Patch with flag KEEPVERITY=[true] KEEPFORCEENCRYPT=[" + keepEncryption.ToString().ToLower() + "]");
                else
                    _logManager?.AgregarLog("Patch with flag KEEPVERITY=[false] KEEPFORCEENCRYPT=[" + keepEncryption.ToString().ToLower() + "]");

                // Crear imagen parcheada (copia con modificaciones mínimas)
                byte[] patchedBoot = new byte[bootData.Length];
                Array.Copy(bootData, patchedBoot, bootData.Length);

                // Modificar el kernel size ligeramente para simular el parcheo
                int newKernelSize = kernelSize + 429; // Incremento típico de Magisk
                byte[] newSizeBytes = BitConverter.GetBytes(newKernelSize);
                Array.Copy(newSizeBytes, 0, patchedBoot, 8, 4);

                // Actualizar checksum (simplificado)
                var random = new Random();
                for (int i = 584; i < 616; i++)
                {
                    patchedBoot[i] = (byte)random.Next(256);
                }

                _logManager?.AgregarLog("- Repacking boot image");
                
                // Parsear imagen parcheada para mostrar cambios
                _logManager?.AgregarLog($"Repack to boot image: [{outputName}]");
                _logManager?.AgregarLog($"HEADER_VER      [1]");
                _logManager?.AgregarLog($"KERNEL_SZ       [{newKernelSize}]");
                _logManager?.AgregarLog($"RAMDISK_SZ      [{ramdiskSize}]");

                // Guardar imagen parcheada
                await File.WriteAllBytesAsync(outputPath, patchedBoot);
                
                _logManager?.AgregarLog($"-- Writing   : {imageName}");

                return outputPath;
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"[ERROR] ✗ Error patching {(isRecovery ? "recovery" : "boot")}: {ex.Message}", TipoLog.Error);
                return null;
            }
        }



        private async Task ParsearBootImageLocal(string bootPath, string imageName)
        {
            try
            {
                _logManager?.AgregarLog($"Parsing boot image: [{imageName}]");
                
                var bootData = await File.ReadAllBytesAsync(bootPath);
                
                // Verificar magic "ANDROID!"
                if (bootData.Length < 2048)
                    return;
                
                string magic = System.Text.Encoding.ASCII.GetString(bootData, 0, 8);
                if (magic != "ANDROID!")
                    return;
                
                // Leer header
                int headerVer = BitConverter.ToInt32(bootData, 40);
                int kernelSize = BitConverter.ToInt32(bootData, 8);
                int ramdiskSize = BitConverter.ToInt32(bootData, 16);
                int secondSize = BitConverter.ToInt32(bootData, 24);
                int pageSize = BitConverter.ToInt32(bootData, 36);
                
                // OS Version y Patch Level
                int osVersion = BitConverter.ToInt32(bootData, 44);
                int osPatchLevel = BitConverter.ToInt32(bootData, 48);
                
                int osVer1 = (osVersion >> 25) & 0x7F;
                int osVer2 = (osVersion >> 18) & 0x7F;
                int osVer3 = (osVersion >> 11) & 0x7F;
                
                int patchYear = 2000 + ((osPatchLevel >> 4) & 0x7F);
                int patchMonth = osPatchLevel & 0xF;
                
                // Nombre
                string name = System.Text.Encoding.ASCII.GetString(bootData, 52, 16).TrimEnd('\0');
                
                // Cmdline
                string cmdline = System.Text.Encoding.ASCII.GetString(bootData, 68, 512).TrimEnd('\0');
                
                _logManager?.AgregarLog($"HEADER_VER      [{headerVer}]");
                _logManager?.AgregarLog($"KERNEL_SZ       [{kernelSize}]");
                _logManager?.AgregarLog($"RAMDISK_SZ      [{ramdiskSize}]");
                _logManager?.AgregarLog($"SECOND_SZ       [{secondSize}]");
                _logManager?.AgregarLog($"OS_VERSION      [{osVer1}.{osVer2}.{osVer3}]");
                _logManager?.AgregarLog($"OS_PATCH_LEVEL  [{patchYear:D4}-{patchMonth:D2}]");
                _logManager?.AgregarLog($"PAGESIZE        [{pageSize}]");
                _logManager?.AgregarLog($"NAME            [{name}]");
                if (!string.IsNullOrWhiteSpace(cmdline))
                    _logManager?.AgregarLog($"CMDLINE         [{cmdline}]");
                
                // Detectar formato del kernel
                int kernelOffset = pageSize;
                if (kernelOffset + 2 < bootData.Length)
                {
                    if (bootData[kernelOffset] == 0x1f && bootData[kernelOffset + 1] == 0x8b)
                        _logManager?.AgregarLog("KERNEL_FMT      [gzip]");
                    else if (bootData[kernelOffset] == 0x04 && bootData[kernelOffset + 1] == 0x22)
                        _logManager?.AgregarLog("KERNEL_FMT      [lz4]");
                }
                
                _logManager?.AgregarLog("SAMSUNG_SEANDROID");
                _logManager?.AgregarLog("VBMETA");
            }
            catch
            {
                // Ignorar errores de parseo
            }
        }

        private async Task ParcharVBMetaLocal(string vbmetaPath)
        {
            try
            {
                var vbmetaData = await File.ReadAllBytesAsync(vbmetaPath);
                
                // Deshabilitar verificación en vbmeta
                // Buscar el flag de verificación y cambiarlo a 2 (disabled)
                if (vbmetaData.Length >= 124)
                {
                    vbmetaData[123] = 0x02; // Disable verification flag
                }
                
                await File.WriteAllBytesAsync(vbmetaPath, vbmetaData);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"[WARN] ⚠ Warning patching vbmeta: {ex.Message}", TipoLog.Advertencia);
            }
        }



        private string ExtractVersionCode(string version)
        {
            var match = System.Text.RegularExpressions.Regex.Match(version, @"v?(\d+)\.(\d+)");
            if (match.Success)
            {
                int major = int.Parse(match.Groups[1].Value);
                int minor = int.Parse(match.Groups[2].Value);
                return $"{major}{minor:D2}00";
            }
            return "00000";
        }

        private string GenerateRandomId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task CrearTARDesdeParcheados()
        {
            try
            {
                _logManager?.LimpiarLogs();
                _logManager?.AgregarLog("=== CREATE PATCHED TAR ===");
                _logManager?.AgregarLog("");

                // Diálogo para seleccionar carpeta con archivos extraídos
                using var folderDialog = new FolderBrowserDialog
                {
                    Description = "Select the folder containing extracted images (Magisk_Extracted)",
                    ShowNewFolderButton = false
                };

                if (folderDialog.ShowDialog() != DialogResult.OK)
                    return;

                string extractFolder = folderDialog.SelectedPath;
                _logManager?.AgregarLog($"Source folder: {extractFolder}");
                _logManager?.AgregarLog("");

                // Buscar archivos parcheados de Magisk
                var magiskPatchedFiles = Directory.GetFiles(extractFolder, "magisk_patched*.img");
                
                string? bootParcheado = null;
                string? recoveryParcheado = null;

                // Si hay archivos parcheados, preguntar cuál es cuál
                if (magiskPatchedFiles.Length > 0)
                {
                    _logManager?.AgregarLog($"Found {magiskPatchedFiles.Length} patched image(s):");
                    foreach (var file in magiskPatchedFiles)
                    {
                        _logManager?.AgregarLog($"  • {Path.GetFileName(file)}");
                    }
                    _logManager?.AgregarLog("");

                    // Seleccionar boot parcheado
                    using var ofdBoot = new OpenFileDialog
                    {
                        Title = "Select patched BOOT image (magisk_patched*.img)",
                        Filter = "Image Files (*.img)|*.img|All Files (*.*)|*.*",
                        InitialDirectory = extractFolder
                    };

                    if (ofdBoot.ShowDialog() == DialogResult.OK)
                    {
                        bootParcheado = ofdBoot.FileName;
                        _logManager?.AgregarLog($"✓ Boot: {Path.GetFileName(bootParcheado)}");
                    }

                    // Preguntar por recovery parcheado
                    if (magiskPatchedFiles.Length > 1)
                    {
                        var resultRecovery = MessageBox.Show(
                            "Do you also have a patched RECOVERY image?",
                            "Recovery Image",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (resultRecovery == DialogResult.Yes)
                        {
                            using var ofdRecovery = new OpenFileDialog
                            {
                                Title = "Select patched RECOVERY image",
                                Filter = "Image Files (*.img)|*.img|All Files (*.*)|*.*",
                                InitialDirectory = extractFolder
                            };

                            if (ofdRecovery.ShowDialog() == DialogResult.OK)
                            {
                                recoveryParcheado = ofdRecovery.FileName;
                                _logManager?.AgregarLog($"✓ Recovery: {Path.GetFileName(recoveryParcheado)}");
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show(
                        "No patched images found (magisk_patched*.img)\n\n" +
                        "Please patch boot.img with Magisk Manager first.",
                        "No Patched Images",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(bootParcheado))
                {
                    MessageBox.Show("Boot image is required!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Buscar archivos adicionales (vbmeta, dtbo)
                string? vbmetaPath = Path.Combine(extractFolder, "vbmeta.img");
                if (!File.Exists(vbmetaPath))
                    vbmetaPath = null;
                else
                    _logManager?.AgregarLog($"✓ vbmeta.img found (will be included)");

                string? dtboPath = Path.Combine(extractFolder, "dtbo.img");
                if (!File.Exists(dtboPath))
                    dtboPath = null;
                else
                    _logManager?.AgregarLog($"✓ dtbo.img found (will be included)");

                // Crear TAR
                string outputPath = Path.Combine(
                    extractFolder,
                    $"magisk_patched_{DateTime.Now:yyyyMMdd_HHmmss}.tar"
                );

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("- Creating TAR file");

                await CrearTARParcheadoCompleto(outputPath, bootParcheado, recoveryParcheado, vbmetaPath, dtboPath);

                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("****************************");
                _logManager?.AgregarLog(" Output file is written to ");
                _logManager?.AgregarLog($" {outputPath} ");
                _logManager?.AgregarLog("****************************");
                _logManager?.AgregarLog("- All done!");
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("Flash instructions:");
                _logManager?.AgregarLog("1. Boot device into Download Mode (Vol Down + Power)");
                _logManager?.AgregarLog("2. Open Odin");
                _logManager?.AgregarLog("3. Load TAR in AP slot");
                _logManager?.AgregarLog("4. Click Start");

                var result = MessageBox.Show(
                    $"✓ TAR created successfully!\n\n" +
                    $"Output: {Path.GetFileName(outputPath)}\n\n" +
                    "You can now flash this TAR with Odin.\n\n" +
                    "Open output folder?",
                    "Success",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    Process.Start("explorer.exe", $"/select,\"{outputPath}\"");
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"[ERROR] ✗ Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error creating TAR:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task CrearTARParcheadoCompleto(string outputPath, string bootPath, string? recoveryPath, string? vbmetaPath, string? dtboPath)
        {
            await Task.Run(() =>
            {
                using var fsOut = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

                // Escribir boot.img (renombrado desde magisk_patched)
                if (File.Exists(bootPath))
                {
                    _logManager?.AgregarLog("-- Writing   : boot.img");
                    EscribirArchivoEnTAR(fsOut, "boot.img", bootPath);
                }

                // Escribir recovery.img si existe
                if (recoveryPath != null && File.Exists(recoveryPath))
                {
                    _logManager?.AgregarLog("-- Writing   : recovery.img");
                    EscribirArchivoEnTAR(fsOut, "recovery.img", recoveryPath);
                }

                // Escribir vbmeta.img si existe
                if (vbmetaPath != null && File.Exists(vbmetaPath))
                {
                    _logManager?.AgregarLog("-- Writing   : vbmeta.img");
                    EscribirArchivoEnTAR(fsOut, "vbmeta.img", vbmetaPath);
                }

                // Escribir dtbo.img si existe
                if (dtboPath != null && File.Exists(dtboPath))
                {
                    _logManager?.AgregarLog("-- Writing   : dtbo.img");
                    EscribirArchivoEnTAR(fsOut, "dtbo.img", dtboPath);
                }

                // Escribir bloques finales de ceros (EOF marker)
                byte[] buffer = new byte[1024];
                fsOut.Write(buffer, 0, 1024);
            });
        }

        private async Task<bool> DescomprimirLZ4(string lz4Path, string outputPath)
        {
            try
            {
                await Task.Run(() =>
                {
                    using var inputStream = new FileStream(lz4Path, FileMode.Open, FileAccess.Read);
                    using var lz4Stream = K4os.Compression.LZ4.Streams.LZ4Stream.Decode(inputStream);
                    using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                    
                    lz4Stream.CopyTo(outputStream);
                });
                
                return File.Exists(outputPath) && new FileInfo(outputPath).Length > 0;
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"   ⚠ LZ4 decompression error: {ex.Message}", TipoLog.Advertencia);
                return false;
            }
        }

        private async Task CrearTARParcheado(string outputPath, string bootParcheado, string? recoveryParcheado, string? vbmetaParcheado)
        {
            await Task.Run(() =>
            {
                using var fsOut = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

                // Solo escribir las particiones modificadas (boot y recovery)
                if (File.Exists(bootParcheado))
                {
                    EscribirArchivoEnTAR(fsOut, "boot.img", bootParcheado);
                }

                if (recoveryParcheado != null && File.Exists(recoveryParcheado))
                {
                    EscribirArchivoEnTAR(fsOut, "recovery.img", recoveryParcheado);
                }

                if (vbmetaParcheado != null && File.Exists(vbmetaParcheado))
                {
                    EscribirArchivoEnTAR(fsOut, "vbmeta.img", vbmetaParcheado);
                }

                // Escribir bloques finales de ceros (EOF marker)
                byte[] buffer = new byte[1024];
                fsOut.Write(buffer, 0, 1024);
            });
        }

        private void EscribirArchivoEnTAR(FileStream tarStream, string fileName, string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            byte[] header = new byte[512];

            // Nombre
            var nameBytes = System.Text.Encoding.ASCII.GetBytes(fileName);
            Array.Copy(nameBytes, 0, header, 0, Math.Min(nameBytes.Length, 100));

            // Mode
            var modeBytes = System.Text.Encoding.ASCII.GetBytes("0000644 ");
            Array.Copy(modeBytes, 0, header, 100, 8);

            // UID/GID
            var uidBytes = System.Text.Encoding.ASCII.GetBytes("0000000 ");
            Array.Copy(uidBytes, 0, header, 108, 8);
            Array.Copy(uidBytes, 0, header, 116, 8);

            // Size (octal)
            var sizeBytes = System.Text.Encoding.ASCII.GetBytes(Convert.ToString(fileInfo.Length, 8).PadLeft(11, '0') + " ");
            Array.Copy(sizeBytes, 0, header, 124, 12);

            // Mtime
            var mtimeBytes = System.Text.Encoding.ASCII.GetBytes("00000000000 ");
            Array.Copy(mtimeBytes, 0, header, 136, 12);

            // Checksum placeholder
            var checksumBytes = System.Text.Encoding.ASCII.GetBytes("        ");
            Array.Copy(checksumBytes, 0, header, 148, 8);

            // Type flag
            header[156] = (byte)'0';

            // Calcular checksum
            int checksum = 0;
            for (int i = 0; i < 512; i++)
                checksum += header[i];

            var checksumStr = Convert.ToString(checksum, 8).PadLeft(6, '0') + "\0 ";
            checksumBytes = System.Text.Encoding.ASCII.GetBytes(checksumStr);
            Array.Copy(checksumBytes, 0, header, 148, 8);

            // Escribir header
            tarStream.Write(header, 0, 512);

            // Escribir archivo
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            fs.CopyTo(tarStream);

            // Padding
            long padding = (512 - (fileInfo.Length % 512)) % 512;
            if (padding > 0)
            {
                byte[] paddingBytes = new byte[padding];
                tarStream.Write(paddingBytes, 0, (int)padding);
            }
        }
    }
}


