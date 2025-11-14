using TT_Tool.Controls;
using TT_Tool.Managers;
using System.Diagnostics;

namespace TT_Tool.Brands
{
    /// <summary>
    /// Operaciones para dispositivos Honor/Huawei
    /// </summary>
    public class HonorOperations : IBrandOperations
    {
        private HonorOEMManager? _oemManager;
        private LogManager? _logManager;
        private ADBManager? _adbManager;

        public string BrandName => "Honor";

        public void Initialize()
        {
            _adbManager = new ADBManager();
        }

        public void SetLogManager(LogManager logManager)
        {
            _logManager = logManager;
            _oemManager = new HonorOEMManager();
            _oemManager.SetLogManager(_logManager);
            // No suscribirse al evento OnLogMessage porque el manager ya usa el LogManager directamente
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

            // Pestaña 1: Main Services
            var tabMainService = new TabPage("Main Services");
            tabMainService.BackColor = Color.FromArgb(240, 242, 245);
            tabMainService.Controls.Add(CrearPanelMainServices());
            tabControl.TabPages.Add(tabMainService);

            // Pestaña 2: Fastboot Services
            var tabFastboot = new TabPage("Fastboot Services");

            tabFastboot.BackColor = Color.FromArgb(240, 242, 245);
            tabFastboot.Controls.Add(CrearPanelFastbootServices());
            tabControl.TabPages.Add(tabFastboot);

            panel.Controls.Add(tabControl);
            return panel;
        }

        private Panel CrearPanelMainServices()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20, 20, 20, 20),
                BackColor = Color.FromArgb(235, 238, 242)
            };

            // Botón: Patch OEM
            var btnPatchOEM = new ModernButton
            {
                Text = "🔧 (Auto) PATCH OEM - REMOVE PAYJOY, MDM",
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0),
                Margin = new Padding(0, 0, 0, 8)
            };
            btnPatchOEM.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnPatchOEM.FlatAppearance.BorderSize = 1;
            btnPatchOEM.Click += async (s, e) => await PatchOEMInfo();
            panel.Controls.Add(btnPatchOEM);

            return panel;
        }

        private Panel CrearPanelFastbootServices()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20, 20, 20, 20),
                BackColor = Color.FromArgb(235, 238, 242)
            };

            // Botón: Read SN
            var btnReadSN = new ModernButton
            {
                Text = "📱 [ FASTBOOT ] READ SN",
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0),
                Margin = new Padding(0, 0, 0, 8)
            };
            btnReadSN.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnReadSN.FlatAppearance.BorderSize = 1;
            btnReadSN.Click += async (s, e) => await ReadSerialNumber();
            panel.Controls.Add(btnReadSN);

            // Botón: Write FRP Key
            var btnWriteFRPKey = new ModernButton
            {
                Text = "🔓 [ FASTBOOT ] WRITE FRP KEY",
                Dock = DockStyle.Top,
                Height = 42,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 75, 82),
                BorderRadius = 8,
                HoverColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0),
                Margin = new Padding(0, 0, 0, 8)
            };
            btnWriteFRPKey.FlatAppearance.BorderColor = Color.FromArgb(215, 220, 225);
            btnWriteFRPKey.FlatAppearance.BorderSize = 1;
            btnWriteFRPKey.Click += async (s, e) => await WriteFRPKey();
            panel.Controls.Add(btnWriteFRPKey);

            return panel;
        }

        // ==================== MÉTODOS DE OPERACIONES ====================

        private async Task PatchOEMInfo()
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "All Files (*.*)|*.*|IMG Files (*.img)|*.img|BIN Files (*.bin)|*.bin";
                    ofd.Title = "Select OEM Info file";
                    ofd.FilterIndex = 1;

                    if (ofd.ShowDialog() != DialogResult.OK)
                        return;

                    _logManager?.LimpiarLogs();

                    // Inicializar el manager si no existe
                    if (_oemManager == null)
                    {
                        _oemManager = new HonorOEMManager();
                        if (_logManager != null)
                        {
                            _oemManager.SetLogManager(_logManager);
                        }
                    }

                    bool resultado = await _oemManager.AnalizarYModificarOEMInfo(ofd.FileName);

                    // Solo mostrar alerta si hubo éxito y preguntar si quiere abrir la carpeta
                    if (resultado)
                    {
                        string? directorio = Path.GetDirectoryName(ofd.FileName);
                        if (!string.IsNullOrEmpty(directorio))
                        {
                            var confirmResult = MessageBox.Show(
                                "Open output folder?",
                                "Completed",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question);

                            if (confirmResult == DialogResult.Yes)
                            {
                                Process.Start("explorer.exe", directorio);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error processing file:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ReadSerialNumber()
        {
            try
            {
                _logManager?.LimpiarLogs();
                _logManager?.AgregarLog("[ FASTBOOT ] READ SN");
                _logManager?.AgregarLog("Detecting device in fastboot mode...");

                string fastbootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Tools", "fastboot.exe");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fastbootPath,
                        Arguments = "devices",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (string.IsNullOrWhiteSpace(output))
                {
                    _logManager?.AgregarLog("No device detected in fastboot mode", TipoLog.Error);
                    _logManager?.AgregarLog("Please connect device in fastboot mode", TipoLog.Advertencia);
                    return;
                }

                // Extraer el serial number (primera columna)
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains("fastboot"))
                    {
                        var parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                        {
                            string serialNumber = parts[0];
                            _logManager?.AgregarLog("Device detected... OK", TipoLog.Exito);
                            _logManager?.AgregarLog("");
                            _logManager?.AgregarLog($"Serial Number: {serialNumber}", TipoLog.Exito);
                            _logManager?.AgregarLog("");
                            _logManager?.AgregarLog("Operation completed successfully", TipoLog.Exito);
                            return;
                        }
                    }
                }

                _logManager?.AgregarLog("Could not read serial number", TipoLog.Error);
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
            }
        }

        private async Task WriteFRPKey()
        {
            try
            {
                // Crear diálogo para ingresar el FRP Key
                var inputDialog = new Form
                {
                    Text = "Enter FRP Key",
                    Size = new Size(450, 180),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    BackColor = Color.White
                };

                var lblInfo = new Label
                {
                    Text = "Enter the FRP unlock key:",
                    Location = new Point(20, 20),
                    Size = new Size(400, 20),
                    Font = new Font("Segoe UI", 10F)
                };
                inputDialog.Controls.Add(lblInfo);

                var txtKey = new TextBox
                {
                    Location = new Point(20, 50),
                    Width = 400,
                    Font = new Font("Segoe UI", 10F),
                    PlaceholderText = "Enter FRP key here..."
                };
                inputDialog.Controls.Add(txtKey);

                var btnOK = new Button
                {
                    Text = "Unlock",
                    Location = new Point(230, 90),
                    Size = new Size(90, 35),
                    DialogResult = DialogResult.OK,
                    BackColor = Color.FromArgb(66, 133, 244),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                btnOK.FlatAppearance.BorderSize = 0;
                inputDialog.Controls.Add(btnOK);

                var btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(330, 90),
                    Size = new Size(90, 35),
                    DialogResult = DialogResult.Cancel,
                    BackColor = Color.FromArgb(200, 200, 200),
                    ForeColor = Color.FromArgb(50, 50, 50),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F)
                };
                btnCancel.FlatAppearance.BorderSize = 0;
                inputDialog.Controls.Add(btnCancel);

                inputDialog.AcceptButton = btnOK;
                inputDialog.CancelButton = btnCancel;

                if (inputDialog.ShowDialog() != DialogResult.OK)
                    return;

                string frpKey = txtKey.Text.Trim();

                if (string.IsNullOrWhiteSpace(frpKey))
                {
                    MessageBox.Show("Please enter a valid FRP key", "Invalid Key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _logManager?.LimpiarLogs();
                _logManager?.AgregarLog("[ FASTBOOT ] WRITE FRP KEY");
                _logManager?.AgregarLog($"FRP Key entered: {frpKey}");
                _logManager?.AgregarLog("");
                _logManager?.AgregarLog("Sending unlock command...");

                string fastbootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Tools", "fastboot.exe");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = fastbootPath,
                        Arguments = $"oem frp-unlock {frpKey}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                // Combinar output y error ya que fastboot suele usar stderr
                string fullOutput = output + error;

                _logManager?.AgregarLog("");

                // Verificar si fue exitoso
                if (fullOutput.Contains("OKAY", StringComparison.OrdinalIgnoreCase) ||
                    fullOutput.Contains("success", StringComparison.OrdinalIgnoreCase) ||
                    fullOutput.Contains("unlocked", StringComparison.OrdinalIgnoreCase))
                {
                    _logManager?.AgregarLog("✓ FRP unlock successful!", TipoLog.Exito);
                    _logManager?.AgregarLog("Device FRP has been unlocked", TipoLog.Exito);
                    MessageBox.Show("FRP unlock successful!\n\nThe device FRP has been unlocked.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (fullOutput.Contains("incorrect", StringComparison.OrdinalIgnoreCase) ||
                         fullOutput.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                         fullOutput.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                         fullOutput.Contains("FAIL", StringComparison.OrdinalIgnoreCase))
                {
                    _logManager?.AgregarLog("✗ Incorrect FRP key", TipoLog.Error);
                    _logManager?.AgregarLog("The key provided is not valid for this device", TipoLog.Error);
                    MessageBox.Show("Incorrect FRP key!\n\nThe key provided is not valid for this device.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    _logManager?.AgregarLog("Command executed", TipoLog.Advertencia);
                    _logManager?.AgregarLog("Response from device:");
                    _logManager?.AgregarLog(fullOutput);
                }
            }
            catch (Exception ex)
            {
                _logManager?.AgregarLog($"Error: {ex.Message}", TipoLog.Error);
                MessageBox.Show($"Error executing command:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
