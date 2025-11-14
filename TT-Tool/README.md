# 🔧 AREPA-TOOL

> **Professional Tool for Samsung, Motorola, Android & More Devices**  
> By LeoPE-GSM.COM

[![License](https://img.shields.io/badge/License-Proprietary-red.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)](https://www.microsoft.com/windows)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Status](https://img.shields.io/badge/Status-Active-success.svg)]()

---

## 📱 Overview

AREPA-TOOL is a comprehensive professional tool designed for device management and operations across multiple brands including Samsung, Motorola, Android, and Honor devices.

### ✨ Key Features

#### Samsung Operations
- 🔥 **Odin Mode Flashing** - Flash firmware (BL, AP, CP, CSC)
- 🛠️ **Fix Apps Bancarias 2025** - Banking apps fix solution
- 📲 **Magisk Manager Installer** - Easy Magisk installation
- 🏦 **Install Apps Bancarias Peru** - Banking apps installer
- 🗑️ **Remove Samsung Account** - Account removal tool
- 📱 **eSIM to SIM Convert** - Convert eSIM to physical SIM
- 🌍 **CSC Change** - Change device CSC code
- 📊 **Device Info Reader** - Read complete device information

#### Android Operations
- 📱 **ADB Read Info** - Read device information via ADB
- 🖥️ **Live Screen (scrcpy)** - Screen mirroring
- 🔄 **Reboot Modes** - Multiple reboot options
- 🔓 **FRP Remove** - Factory Reset Protection removal
- 📦 **Package Manager** - App management tools

#### Motorola Operations
- 🔓 **OEM Unlock** - Bootloader unlock
- 🛠️ **MTK Operations** - MediaTek specific operations

---

## 🚀 Installation

### Requirements

- **OS**: Windows 10/11 (64-bit)
- **.NET Runtime**: 8.0 (included)
- **USB Drivers**: For your specific device
- **ADB Debugging**: Enabled on device

### Quick Start

1. **Download** the latest release
2. **Extract** all files to a folder
3. **Run** `AREPA-TOOL.exe`
4. **No installation required!**

⚠️ **IMPORTANT**: Keep all files together in the same folder, including:
- Resources folder
- Runtimes folder
- All DLL files

---

## 🛠️ Building from Source

### Prerequisites

```bash
- .NET 8.0 SDK
- Windows 10/11
- Visual Studio 2022 or VS Code
```

### Clone Repository

```bash
git clone https://github.com/FernandoGsm2002/AREPA-TOOL.git
cd AREPA-TOOL/TT-Tool/TT-Tool
```

### Build Release

```bash
dotnet build -c Release
```

### Package Distribution

```bash
# Run the packaging script
.\package-release.bat
```

This will create a ready-to-distribute folder on your desktop with all necessary files.

---

## 📁 Project Structure

```
AREPA-TOOL/
├── Brands/              # Brand-specific operations
│   ├── AndroidOperations.cs
│   ├── SamsungOperations.cs
│   ├── MotorolaOperations.cs
│   └── HonorOperations.cs
├── Managers/            # Core managers
│   ├── ADBManager.cs
│   ├── LogManager.cs
│   ├── LicenseManager.cs
│   └── DeviceManager.cs
├── Controls/            # Custom UI controls
├── Resources/           # Images, icons, tools
│   ├── AppsBancarias/   # Banking apps APKs
│   ├── Tools/           # ADB, Fastboot, scrcpy
│   └── *.png            # UI images
└── Forms/               # Additional forms

```

---

## 🔒 License & Remote Control

This tool implements a remote license verification system for controlled distribution.

### For Developers

The tool checks a remote JSON configuration file at startup. See [`SISTEMA-CONTROL-REMOTO.md`](TT-Tool/SISTEMA-CONTROL-REMOTO.md) for complete documentation.

**Quick Setup:**
1. Create public GitHub repository: `arepa-tool-config`
2. Add `license.json` file
3. Configure URL in `LicenseManager.cs`

### License Configuration Example

```json
{
  "enabled": true,
  "message": "",
  "welcomeMessage": "Welcome to AREPA-TOOL!",
  "expirationDate": null
}
```

---

## 📖 Documentation

- 📘 [**Compilation Instructions**](TT-Tool/INSTRUCCIONES-COMPILACION.md)
- 🔐 [**Remote Control System**](TT-Tool/SISTEMA-CONTROL-REMOTO.md)
- 📝 [**Final Summary**](TT-Tool/RESUMEN-FINAL.md)

---

## ⚠️ Disclaimer

**USE AT YOUR OWN RISK**

- This tool is provided "as is" without warranty of any kind
- The author is not responsible for any damage to your device
- Make sure to create backups before performing any operation
- Only use on devices you own or have permission to modify
- Some operations may void your device warranty

---

## 🤝 Contributing

This is a proprietary project. Contributions are not currently accepted.

For bug reports or feature requests, please open an issue.

---

## 📞 Contact & Support

- **Website**: [LeoPE-GSM.COM](https://leopegsm.com)
- **Repository**: [GitHub - AREPA-TOOL](https://github.com/FernandoGsm2002/AREPA-TOOL)
- **Version**: 1.0.0
- **Author**: LeoPE-GSM.COM

---

## 🙏 Acknowledgments

Special thanks to the following projects and libraries:

- [SharpOdinClient](https://github.com/username/SharpOdinClient) - Odin protocol implementation
- [LibUsbDotNet](https://github.com/LibUsbDotNet/LibUsbDotNet) - USB device communication
- [scrcpy](https://github.com/Genymobile/scrcpy) - Screen mirroring
- [Magisk](https://github.com/topjohnwu/Magisk) - Root solution
- [K4os.Compression.LZ4](https://github.com/MiloszKrajewski/K4os.Compression.LZ4) - LZ4 compression

---

## 📊 Statistics

![GitHub repo size](https://img.shields.io/github/repo-size/FernandoGsm2002/AREPA-TOOL)
![GitHub language count](https://img.shields.io/github/languages/count/FernandoGsm2002/AREPA-TOOL)
![GitHub top language](https://img.shields.io/github/languages/top/FernandoGsm2002/AREPA-TOOL)

---

## 📜 Copyright

Copyright © 2025 LeoPE-GSM.COM  
All rights reserved.

This software is proprietary and confidential. Unauthorized copying, distribution, or modification of this software, via any medium, is strictly prohibited.

---

**Made with ❤️ by LeoPE-GSM.COM**

