using TT_Tool.Controls;
using TT_Tool.Managers;
using System.Diagnostics;

namespace TT_Tool.Brands
{
    /// <summary>
    /// Operaciones para dispositivos Motorola (MTK)
    /// </summary>
    public class MotorolaOperations : IBrandOperations
    {
        private MotorolaMTKManager? _mtkManager;
        private LogManager? _logManager;
        private ADBManager? _adbManager;

        // Configuración MTK
        private string? _preloaderPath;
        private string? _loaderPath;
        private string _selectedDevice = "penangf"; // Default: Moto G24

        public string BrandName => "Motorola";

        public void Initialize()
        {
            _adbManager = new ADBManager();
        }

        public void SetLogManager(LogManager logManager)
        {
            _logManager = logManager;
            _mtkManager = new MotorolaMTKManager();
            _mtkManager.SetLogManager(_logManager);
            _mtkManager.OnLogMessage += (s, msg) => _logManager?.AgregarLog(msg);
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

            // Pestaña 1: MTK Operations
            var tabMTK = new TabPage("MTK Operations");
            tabMTK.BackColor = Color.FromArgb(240, 242, 245);
            tabMTK.Controls.Add(CrearPanelMTKOperations());
            tabControl.TabPages.Add(tabMTK);

            // Pestaña 2: OEM Unlock
            var tabOEM = new TabPage("OEM Unlock");
            tabOEM.BackColor = Color.FromArgb(240, 242, 245);
            tabOEM.Controls.Add(CrearPanelOEMUnlock());
            tabControl.TabPages.Add(tabOEM);

            // Pestaña 3: Fastboot Tools
            var tabFastboot = new TabPage("Fastboot Tools");
            tabFastboot.BackColor = Color.FromArgb(240, 242, 245);
            tabFastboot.Controls.Add(CrearPanelFastbootTools());
            tabControl.TabPages.Add(tabFastboot);

            panel.Controls.Add(tabControl);
            return panel;
        }

        private Panel CrearPanelMTKOperations()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(235, 238, 242)
            };

            int yPos = 10;

            // Selector de dispositivo
            var lblDevice = new Label
            {
                Text = "Device Model:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(20, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblDevice);

            var cmbDevice = new ModernComboBox
            {
                Location = new Point(150, yPos - 3),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbDevice.Items.AddRange(new object[]
            {
                "Moto G24 (penangf)",
                "Moto G24 Power (fogorow)",
                "Moto G04 (lamu)"
            });
            cmbDevice.SelectedIndex = 0;
            cmbDevice.SelectedIndexChanged += (s, e) =>
            {
                _selectedDevice = cmbDevice.SelectedIndex switch
                {
                    0 => "penangf",
                    1 => "fogorow",
                    2 => "lamu",
                    _ => "penangf"
                };
            };
            panel.Controls.Add(cmbDevice);

            yPos += 50;

            // Selector de Preloader
            var lblPreloader = new Label
            {
                Text = "Preloader:",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(70, 70, 70),
                Location = new Point(20, yPos + 5),
                AutoSize = true
            };
            panel.Controls.Add(lblPreloader);

            var txtPreloader = new TextBox
            {
                Location = new Point(120, yPos),
                Width = 330,
                Font = new Font("Segoe UI", 9F),
                ReadOnly = true,
                BackColor = Color.White
            };
            panel.Controls.Add(txtPreloader);

            var btnPreloader = new ModernButton
            {
                Text = "📁",
                Location = new Point(460, yPos),
                Size = new Size(40, 26),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                BorderRadius = 5
            };
            btnPreloader.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "BIN Files (*.bin)|*.bin|All Files (*.*)|*.*";
                    ofd.Title = "Select Preloader";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        txtPreloader.Text = ofd.FileName;
                        _preloaderPath = ofd.FileName;
                    }
                }
            };
            panel.Controls.Add(btnPreloader);

            yPos += 40;

            // Selector de Loader
            var lblLoader = new Label
            {
                Text = "Loader (DA):",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(70, 70, 70),
                Location = new Point(20, yPos + 5),
                AutoSize = true
            };
            panel.Controls.Add(lblLoader);

            var txtLoader = new TextBox
            {
                Location = new Point(120, yPos),
                Width = 330,
                Font = new Font("Segoe UI", 9F),
                ReadOnly = true,
                BackColor = Color.White
            };
            panel.Controls.Add(txtLoader);

            var btnLoader = new ModernButton
            {
                Text = "📁",
                Location = new Point(460, yPos),
                Size = new Size(40, 26),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                BorderRadius = 5
            };
            btnLoader.Click += (s, e) =>
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "BIN Files (*.bin)|*.bin|All Files (*.*)|*.*";
                    ofd.Title = "Select Loader (DA)";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        txtLoader.Text = ofd.FileName;
                        _loaderPath = ofd.FileName;
                    }
                }
            };
            panel.Controls.Add(btnLoader);

            yPos += 60;

            // Separador
            var separator = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(480, 2),
                BackColor = Color.FromArgb(200, 200, 200)
            };
            panel.Controls.Add(separator);

            yPos += 20;

            // Botones de operaciones
            var btnDetectDevice = new ModernButton
            {
                Text = "🔍 Detect MTK Device",
                Location = new Point(20, yPos),
                Size = new Size(480, 42),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnDetectDevice.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnDetectDevice.FlatAppearance.BorderSize = 1;
            btnDetectDevice.Click += async (s, e) => await DetectarDispositivoMTK();
            panel.Controls.Add(btnDetectDevice);

            yPos += 50;

            var btnBackupCritical = new ModernButton
            {
                Text = "💾 Backup Critical Partitions",
                Location = new Point(20, yPos),
                Size = new Size(480, 42),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnBackupCritical.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnBackupCritical.FlatAppearance.BorderSize = 1;
            btnBackupCritical.Click += async (s, e) => await BackupParticionesCriticas();
            panel.Controls.Add(btnBackupCritical);

            yPos += 50;

            var btnReadPartition = new ModernButton
            {
                Text = "📖 Read Partition",
                Location = new Point(20, yPos),
                Size = new Size(230, 42),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnReadPartition.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnReadPartition.FlatAppearance.BorderSize = 1;
            btnReadPartition.Click += async (s, e) => await LeerParticion();
            panel.Controls.Add(btnReadPartition);

            var btnWritePartition = new ModernButton
            {
                Text = "✍️ Write Partition",
                Location = new Point(270, yPos),
                Size = new Size(230, 42),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnWritePartition.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnWritePartition.FlatAppearance.BorderSize = 1;
            btnWritePartition.Click += async (s, e) => await EscribirParticion();
            panel.Controls.Add(btnWritePartition);

            yPos += 50;

            var btnRemoveCarrier = new ModernButton
            {
                Text = "🔓 Remove Carrier Block",
                Location = new Point(20, yPos),
                Size = new Size(480, 42),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                BorderRadius = 8,
                HoverColor = Color.FromArgb(245, 124, 0),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnRemoveCarrier.Click += async (s, e) => await RemoverBloqueoCarrier();
            panel.Controls.Add(btnRemoveCarrier);

            return panel;
        }

        private Panel CrearPanelOEMUnlock()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(235, 238, 242)
            };

            int yPos = 20;

            // Título
            var lblTitulo = new Label
            {
                Text = "Motorola Bootloader Unlock Key Generator",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(20, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblTitulo);

            yPos += 40;

            // Instrucciones
            var lblInstrucciones = new Label
            {
                Text = "1. Boot device into fastboot mode\n" +
                       "2. Connect device via USB\n" +
                       "3. Click 'Get Device Key' or enter manually\n" +
                       "4. Click 'Generate Unlock Key'",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(20, yPos),
                Size = new Size(500, 80)
            };
            panel.Controls.Add(lblInstrucciones);

            yPos += 90;

            // Device Key Input
            var lblDeviceKey = new Label
            {
                Text = "Device Key (32 hex chars):",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(20, yPos + 5),
                AutoSize = true
            };
            panel.Controls.Add(lblDeviceKey);

            var txtDeviceKey = new TextBox
            {
                Location = new Point(20, yPos + 30),
                Width = 400,
                Font = new Font("Consolas", 10F),
                MaxLength = 32,
                CharacterCasing = CharacterCasing.Upper
            };
            panel.Controls.Add(txtDeviceKey);

            var btnGetKey = new ModernButton
            {
                Text = "🔍 Get Device Key",
                Location = new Point(430, yPos + 28),
                Size = new Size(150, 28),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                BorderRadius = 5,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnGetKey.Click += async (s, e) =>
            {
                var key = await MotorolaOEMUnlocker.ObtenerClaveDispositivo();
                if (!string.IsNullOrEmpty(key))
                {
                    txtDeviceKey.Text = key;
                    _logManager?.AgregarLog($"Device key obtained: {key}", TipoLog.Exito);
                }
                else
                {
                    MessageBox.Show("Could not get device key. Make sure device is in fastboot mode.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            panel.Controls.Add(btnGetKey);

            yPos += 80;

            // Unlock Key Output
            var lblUnlockKey = new Label
            {
                Text = "Unlock Key:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location = new Point(20, yPos + 5),
                AutoSize = true
            };
            panel.Controls.Add(lblUnlockKey);

            var txtUnlockKey = new TextBox
            {
                Location = new Point(20, yPos + 30),
                Width = 400,
                Font = new Font("Consolas", 10F),
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245)
            };
            panel.Controls.Add(txtUnlockKey);

            var btnCopyKey = new ModernButton
            {
                Text = "📋 Copy",
                Location = new Point(430, yPos + 28),
                Size = new Size(70, 28),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                BorderRadius = 5,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnCopyKey.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtUnlockKey.Text))
                {
                    Clipboard.SetText(txtUnlockKey.Text);
                    MessageBox.Show("Unlock key copied to clipboard!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            panel.Controls.Add(btnCopyKey);

            var btnSaveKey = new ModernButton
            {
                Text = "💾 Save",
                Location = new Point(510, yPos + 28),
                Size = new Size(70, 28),
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                BorderRadius = 5,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnSaveKey.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtUnlockKey.Text))
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Filter = "Text Files (*.txt)|*.txt";
                        sfd.FileName = "motorola_unlock_key.txt";
                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllText(sfd.FileName, 
                                $"Device Key: {txtDeviceKey.Text}\n" +
                                $"Unlock Key: {txtUnlockKey.Text}\n" +
                                $"Generated: {DateTime.Now}");
                            MessageBox.Show("Key saved successfully!", "Success", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            };
            panel.Controls.Add(btnSaveKey);

            yPos += 80;

            // Botón Generate
            var btnGenerate = new ModernButton
            {
                Text = "🔑 Generate Unlock Key",
                Location = new Point(20, yPos),
                Size = new Size(560, 45),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                BorderRadius = 8,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            btnGenerate.Click += (s, e) =>
            {
                try
                {
                    string deviceKey = txtDeviceKey.Text.Trim();
                    
                    if (!MotorolaOEMUnlocker.ValidarFormatoClave(deviceKey))
                    {
                        MessageBox.Show("Invalid device key format. Must be 32 hexadecimal characters.", 
                            "Invalid Key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string unlockKey = MotorolaOEMUnlocker.GenerarClaveDesbloqueo(deviceKey);
                    txtUnlockKey.Text = unlockKey;

                    _logManager?.AgregarLog("=== OEM UNLOCK KEY GENERATED ===", TipoLog.Exito);
                    _logManager?.AgregarLog($"Device Key: {deviceKey}");
                    _logManager?.AgregarLog($"Unlock Key: {unlockKey}", TipoLog.Exito);
                    _logManager?.AgregarLog("");
                    _logManager?.AgregarLog("Use this command to unlock:");
                    _logManager?.AgregarLog($"fastboot oem unlock {unlockKey}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating key: {ex.Message}", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            panel.Controls.Add(btnGenerate);

            yPos += 70;

            // Nota importante
            var lblNota = new Label
            {
                Text = "⚠️ WARNING: Unlocking bootloader will erase all data and void warranty!",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(244, 67, 54),
                Location = new Point(20, yPos),
                Size = new Size(560, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(255, 235, 238),
                Padding = new Padding(10)
            };
            panel.Controls.Add(lblNota);

            return panel;
        }

        private Panel CrearPanelFastbootTools()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(235, 238, 242)
            };

            int yPos = 10;

            // Botón: Flash Stock Firmware
            var btnFlashStock = new ModernButton
            {
                Text = "⚡ Flash Stock Firmware",
                Location = new Point(20, yPos),
                Size = new Size(480, 42),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnFlashStock.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnFlashStock.FlatAppearance.BorderSize = 1;
            btnFlashStock.Click += async (s, e) => await FlashStockFirmware();
            panel.Controls.Add(btnFlashStock);

            yPos += 50;

            var btnFixFastbootd = new ModernButton
            {
                Text = "🔧 Fix Fastbootd Softbrick",
                Location = new Point(20, yPos),
                Size = new Size(480, 42),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnFixFastbootd.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnFixFastbootd.FlatAppearance.BorderSize = 1;
            btnFixFastbootd.Click += async (s, e) => await FixFastbootdSoftbrick();
            panel.Controls.Add(btnFixFastbootd);

            yPos += 50;

            var btnRebootFastboot = new ModernButton
            {
                Text = "🔄 Reboot to Fastboot",
                Location = new Point(20, yPos),
                Size = new Size(230, 42),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnRebootFastboot.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnRebootFastboot.FlatAppearance.BorderSize = 1;
            btnRebootFastboot.Click += async (s, e) => await RebootToFastboot();
            panel.Controls.Add(btnRebootFastboot);

            var btnRebootSystem = new ModernButton
            {
                Text = "🔄 Reboot to System",
                Location = new Point(270, yPos),
                Size = new Size(230, 42),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnRebootSystem.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnRebootSystem.FlatAppearance.BorderSize = 1;
            btnRebootSystem.Click += async (s, e) => await RebootToSystem();
            panel.Controls.Add(btnRebootSystem);

            yPos += 50;

            var btnEraseUserdata = new ModernButton
            {
                Text = "🗑️ Erase Userdata",
                Location = new Point(20, yPos),
                Size = new Size(230, 42),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                BorderRadius = 8,
                HoverColor = Color.FromArgb(211, 47, 47),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnEraseUserdata.Click += async (s, e) => await EraseUserdata();
            panel.Controls.Add(btnEraseUserdata);

            var btnEraseMetadata = new ModernButton
            {
                Text = "🗑️ Erase Metadata",
                Location = new Point(270, yPos),
                Size = new Size(230, 42),
                BackColor = Color.FromArgb(244, 67, 54),
                ForeColor = Color.White,
                BorderRadius = 8,
                HoverColor = Color.FromArgb(211, 47, 47),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            btnEraseMetadata.Click += async (s, e) => await EraseMetadata();
            panel.Controls.Add(btnEraseMetadata);

            return panel;
        }

        // ==================== MÉTODOS DE OPERACIONES ====================

        private async Task DetectarDispositivoMTK()
        {
            try
            {
                _logManager?.LimpiarLogs();
                _logManager?.AgregarLog("=== DETECTING MTK DEVICE ===");

                if (_mtkManager == null)
                {
                    MessageBox.Show("MTK Manager not initialized", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var dispositivos = await _mtkManager.DetectarDispositivosMTK();

                if (dispositivos.Count > 0)
                {
                    _logManager?.AgregarLog($"✓ Found {dispositivos.Count} MTK device(s):", TipoLog.Exito);
                    foreach (var dispositivo in dispositivos)
                    {
                        _logManager?.AgregarLog($"  - {dispositivo}");
                    }
                }
                else
                {
                    _logManager?.AgregarLog("✗ No MTK devices found", TipoLog.Advertencia);
                    _logManager?.AgregarLog("Make sure device is in BROM mode (powered off, hold Vol+ while connecting USB)");
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private async Task BackupParticionesCriticas()
        {
            try
            {
                if (!ValidarConfiguracionMTK())
                    return;

                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Select backup destination folder";
                    if (fbd.ShowDialog() != DialogResult.OK)
                        return;

                    string carpetaBackup = Path.Combine(fbd.SelectedPath, 
                        $"motorola_backup_{DateTime.Now:yyyyMMdd_HHmmss}");

                    _logManager?.LimpiarLogs();
                    await _mtkManager!.BackupParticionesCriticas(carpetaBackup, _preloaderPath!, _loaderPath!);

                    MessageBox.Show($"Backup completed!\n\nLocation: {carpetaBackup}", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error during backup: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LeerParticion()
        {
            try
            {
                if (!ValidarConfiguracionMTK())
                    return;

                var inputDialog = new Form
                {
                    Text = "Read Partition",
                    Size = new Size(400, 150),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var lblPartition = new Label
                {
                    Text = "Partition name:",
                    Location = new Point(20, 20),
                    AutoSize = true
                };
                inputDialog.Controls.Add(lblPartition);

                var txtPartition = new TextBox
                {
                    Location = new Point(20, 45),
                    Width = 340
                };
                inputDialog.Controls.Add(txtPartition);

                var btnOK = new Button
                {
                    Text = "OK",
                    Location = new Point(200, 80),
                    DialogResult = DialogResult.OK
                };
                inputDialog.Controls.Add(btnOK);

                var btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(285, 80),
                    DialogResult = DialogResult.Cancel
                };
                inputDialog.Controls.Add(btnCancel);

                inputDialog.AcceptButton = btnOK;
                inputDialog.CancelButton = btnCancel;

                if (inputDialog.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(txtPartition.Text))
                    return;

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "IMG Files (*.img)|*.img|BIN Files (*.bin)|*.bin|All Files (*.*)|*.*";
                    sfd.FileName = $"{txtPartition.Text}.img";
                    
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        _logManager?.LimpiarLogs();
                        await _mtkManager!.LeerParticion(txtPartition.Text, sfd.FileName, 
                            _preloaderPath!, _loaderPath!);
                    }
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private async Task EscribirParticion()
        {
            try
            {
                if (!ValidarConfiguracionMTK())
                    return;

                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "IMG Files (*.img)|*.img|BIN Files (*.bin)|*.bin|All Files (*.*)|*.*";
                    ofd.Title = "Select partition image";
                    
                    if (ofd.ShowDialog() != DialogResult.OK)
                        return;

                    var inputDialog = new Form
                    {
                        Text = "Write Partition",
                        Size = new Size(400, 150),
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.FixedDialog,
                        MaximizeBox = false,
                        MinimizeBox = false
                    };

                    var lblPartition = new Label
                    {
                        Text = "Partition name:",
                        Location = new Point(20, 20),
                        AutoSize = true
                    };
                    inputDialog.Controls.Add(lblPartition);

                    var txtPartition = new TextBox
                    {
                        Location = new Point(20, 45),
                        Width = 340
                    };
                    inputDialog.Controls.Add(txtPartition);

                    var btnOK = new Button
                    {
                        Text = "Write",
                        Location = new Point(200, 80),
                        DialogResult = DialogResult.OK
                    };
                    inputDialog.Controls.Add(btnOK);

                    var btnCancel = new Button
                    {
                        Text = "Cancel",
                        Location = new Point(285, 80),
                        DialogResult = DialogResult.Cancel
                    };
                    inputDialog.Controls.Add(btnCancel);

                    inputDialog.AcceptButton = btnOK;
                    inputDialog.CancelButton = btnCancel;

                    if (inputDialog.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(txtPartition.Text))
                        return;

                    var confirmResult = MessageBox.Show(
                        $"Are you sure you want to write to partition '{txtPartition.Text}'?\n\n" +
                        "This operation can brick your device if done incorrectly!",
                        "Confirm Write",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult == DialogResult.Yes)
                    {
                        _logManager?.LimpiarLogs();
                        await _mtkManager!.EscribirParticion(txtPartition.Text, ofd.FileName, 
                            _preloaderPath!, _loaderPath!);
                    }
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private Task RemoverBloqueoCarrier()
        {
            try
            {
                var confirmResult = MessageBox.Show(
                    "This will remove carrier lock by writing to 'elable' partition.\n\n" +
                    "⚠️ WARNING: This operation is device-specific and may not work on all models.\n\n" +
                    "Continue?",
                    "Remove Carrier Block",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmResult != DialogResult.Yes)
                    return Task.CompletedTask;

                MessageBox.Show(
                    "This feature requires a modified 'elable' partition image.\n\n" +
                    "Please prepare the unlock_elable.img file for your device model.",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // TODO: Implementar lógica de desbloqueo de carrier
                _logManager?.AgregarLog("Carrier unlock feature - Coming soon", TipoLog.Advertencia);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
            
            return Task.CompletedTask;
        }

        private async Task FlashStockFirmware()
        {
            try
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Select firmware folder containing .img files";
                    if (fbd.ShowDialog() != DialogResult.OK)
                        return;

                    var confirmResult = MessageBox.Show(
                        "This will flash stock firmware to your device.\n\n" +
                        "⚠️ WARNING: This will erase all data!\n\n" +
                        "Make sure device is in fastboot mode.\n\n" +
                        "Continue?",
                        "Flash Stock Firmware",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult != DialogResult.Yes)
                        return;

                    _logManager?.LimpiarLogs();
                    _logManager?.AgregarLog("=== FLASHING STOCK FIRMWARE ===");

                    // Flash vbmeta partitions
                    _logManager?.AgregarLog("Flashing vbmeta partitions...");
                    await FlashPartitionIfExists(fbd.SelectedPath, "vbmeta", "--disable-verity --disable-verification");
                    await FlashPartitionIfExists(fbd.SelectedPath, "vbmeta_system", "--disable-verity --disable-verification");
                    await FlashPartitionIfExists(fbd.SelectedPath, "vbmeta_vendor", "--disable-verity --disable-verification");

                    // Flash bootloader
                    _logManager?.AgregarLog("Flashing bootloader...");
                    await FlashPartitionIfExists(fbd.SelectedPath, "lk");
                    await FlashPartitionIfExists(fbd.SelectedPath, "dtbo");

                    _logManager?.AgregarLog("Rebooting to bootloader...");
                    await EjecutarFastboot("reboot bootloader");
                    await Task.Delay(5000);

                    // Flash boot
                    _logManager?.AgregarLog("Flashing boot...");
                    await FlashPartitionIfExists(fbd.SelectedPath, "boot");
                    await FlashPartitionIfExists(fbd.SelectedPath, "vendor_boot");

                    // Flash other partitions
                    _logManager?.AgregarLog("Flashing system partitions...");
                    await FlashPartitionIfExists(fbd.SelectedPath, "md1img");
                    await FlashPartitionIfExists(fbd.SelectedPath, "scp");
                    await FlashPartitionIfExists(fbd.SelectedPath, "spmfw");
                    await FlashPartitionIfExists(fbd.SelectedPath, "sspm");
                    await FlashPartitionIfExists(fbd.SelectedPath, "gz");
                    await FlashPartitionIfExists(fbd.SelectedPath, "tee");
                    await FlashPartitionIfExists(fbd.SelectedPath, "super");

                    // Erase userdata
                    _logManager?.AgregarLog("Erasing userdata...");
                    await EjecutarFastboot("erase userdata");
                    await EjecutarFastboot("erase metadata");
                    await EjecutarFastboot("erase md_udc");

                    _logManager?.AgregarLog("✓ Firmware flashed successfully!", TipoLog.Exito);
                    _logManager?.AgregarLog("Rebooting device...");
                    await EjecutarFastboot("reboot");

                    MessageBox.Show("Firmware flashed successfully!\n\nDevice is rebooting.", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error flashing firmware: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task FlashPartitionIfExists(string firmwareFolder, string partitionName, string extraArgs = "")
        {
            string imgPath = Path.Combine(firmwareFolder, $"{partitionName}.img");
            if (File.Exists(imgPath))
            {
                _logManager?.AgregarLog($"Flashing {partitionName}...");
                string args = string.IsNullOrEmpty(extraArgs) 
                    ? $"flash {partitionName} \"{imgPath}\""
                    : $"flash {extraArgs} {partitionName} \"{imgPath}\"";
                await EjecutarFastboot(args);
            }
            else
            {
                _logManager?.AgregarLog($"Skipping {partitionName} (not found)", TipoLog.Advertencia);
            }
        }

        private async Task FixFastbootdSoftbrick()
        {
            try
            {
                MessageBox.Show(
                    "This will attempt to fix fastbootd softbrick by flashing boot partitions.\n\n" +
                    "Make sure you have the stock firmware folder ready.",
                    "Fix Fastbootd Softbrick",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Select firmware folder";
                    if (fbd.ShowDialog() != DialogResult.OK)
                        return;

                    _logManager?.LimpiarLogs();
                    _logManager?.AgregarLog("=== FIXING FASTBOOTD SOFTBRICK ===");

                    await FlashPartitionIfExists(fbd.SelectedPath, "boot");
                    await FlashPartitionIfExists(fbd.SelectedPath, "vendor_boot");

                    _logManager?.AgregarLog("Rebooting...");
                    await EjecutarFastboot("reboot");

                    MessageBox.Show("Fix applied! Device is rebooting.", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private async Task RebootToFastboot()
        {
            try
            {
                _logManager?.LimpiarLogs();
                _logManager?.AgregarLog("Rebooting to fastboot mode...");
                await _adbManager!.ExecuteAdbCommandAsync("reboot bootloader");
                _logManager?.AgregarLog("✓ Command sent", TipoLog.Exito);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private async Task RebootToSystem()
        {
            try
            {
                _logManager?.LimpiarLogs();
                _logManager?.AgregarLog("Rebooting to system...");
                await EjecutarFastboot("reboot");
                _logManager?.AgregarLog("✓ Command sent", TipoLog.Exito);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private async Task EraseUserdata()
        {
            try
            {
                var confirmResult = MessageBox.Show(
                    "⚠️ WARNING: This will erase ALL user data!\n\n" +
                    "This action cannot be undone.\n\n" +
                    "Continue?",
                    "Erase Userdata",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmResult != DialogResult.Yes)
                    return;

                _logManager?.LimpiarLogs();
                _logManager?.AgregarLog("Erasing userdata...");
                await EjecutarFastboot("erase userdata");
                _logManager?.AgregarLog("✓ Userdata erased", TipoLog.Exito);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private async Task EraseMetadata()
        {
            try
            {
                _logManager?.LimpiarLogs();
                _logManager?.AgregarLog("Erasing metadata...");
                await EjecutarFastboot("erase metadata");
                _logManager?.AgregarLog("✓ Metadata erased", TipoLog.Exito);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private bool ValidarConfiguracionMTK()
        {
            if (string.IsNullOrEmpty(_preloaderPath) || string.IsNullOrEmpty(_loaderPath))
            {
                MessageBox.Show(
                    "Please select Preloader and Loader files first.",
                    "Configuration Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            if (!File.Exists(_preloaderPath))
            {
                MessageBox.Show(
                    "Preloader file not found!",
                    "File Not Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            if (!File.Exists(_loaderPath))
            {
                MessageBox.Show(
                    "Loader file not found!",
                    "File Not Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Helper para ejecutar comandos fastboot
        /// </summary>
        private async Task<string> EjecutarFastboot(string argumentos)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "fastboot",
                        Arguments = argumentos,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                var output = new System.Text.StringBuilder();
                process.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.AppendLine(e.Data);
                        _logManager?.AgregarLog(e.Data);
                    }
                };

                process.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output.AppendLine(e.Data);
                        _logManager?.AgregarLog(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                return output.ToString();
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                return string.Empty;
            }
        }
    }
}
