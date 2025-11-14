# ========================================
# AREPA-TOOL - Release Package Script
# By LeoPE-GSM.COM
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AREPA-TOOL - Package Release" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Variables
$projectPath = "TT-Tool"
$releasePath = "bin\Release\net8.0-windows"
$outputFolder = "AREPA-TOOL-v1.0"
$desktopPath = [Environment]::GetFolderPath("Desktop")
$packagePath = Join-Path $desktopPath $outputFolder

Write-Host "[1/6] Compilando en modo Release..." -ForegroundColor Yellow
dotnet build $projectPath -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Error en la compilación" -ForegroundColor Red
    exit 1
}

Write-Host "      ✓ Compilación exitosa" -ForegroundColor Green
Write-Host ""

Write-Host "[2/6] Limpiando carpeta de destino..." -ForegroundColor Yellow

if (Test-Path $packagePath) {
    Remove-Item -Path $packagePath -Recurse -Force
}

New-Item -ItemType Directory -Path $packagePath | Out-Null
Write-Host "      ✓ Carpeta creada: $packagePath" -ForegroundColor Green
Write-Host ""

Write-Host "[3/6] Copiando archivos ejecutables..." -ForegroundColor Yellow

$sourcePath = Join-Path $projectPath $releasePath

# Copiar ejecutable principal y DLLs
Copy-Item -Path "$sourcePath\AREPA-TOOL.exe" -Destination $packagePath
Copy-Item -Path "$sourcePath\AREPA-TOOL.dll" -Destination $packagePath
Copy-Item -Path "$sourcePath\AREPA-TOOL.runtimeconfig.json" -Destination $packagePath

# Copiar todas las DLLs necesarias
Copy-Item -Path "$sourcePath\*.dll" -Destination $packagePath -Exclude "AREPA-TOOL.dll"

Write-Host "      ✓ Ejecutable y DLLs copiados" -ForegroundColor Green
Write-Host ""

Write-Host "[4/6] Copiando carpeta Resources..." -ForegroundColor Yellow

# Copiar carpeta Resources completa
Copy-Item -Path "$sourcePath\Resources" -Destination $packagePath -Recurse

Write-Host "      ✓ Resources copiados" -ForegroundColor Green
Write-Host ""

Write-Host "[5/6] Copiando carpeta runtimes..." -ForegroundColor Yellow

# Copiar carpeta runtimes (necesaria para .NET)
Copy-Item -Path "$sourcePath\runtimes" -Destination $packagePath -Recurse

Write-Host "      ✓ Runtimes copiados" -ForegroundColor Green
Write-Host ""

Write-Host "[6/6] Creando README.txt..." -ForegroundColor Yellow

$readmeContent = @"
===============================================================
           AREPA-TOOL By LeoPE-GSM.COM
               Version 1.0.0 - 2025
===============================================================

PROFESSIONAL TOOL FOR:
   - Samsung devices (Odin Mode, ADB, Root)
   - Motorola devices (MTK, OEM Unlock)
   - Android devices (ADB operations)
   - Honor/Huawei devices

===============================================================
INSTALLATION INSTRUCTIONS:
===============================================================

1. Extract all files to a folder
2. Run "AREPA-TOOL.exe"
3. No installation required!

IMPORTANT:
- Keep ALL files in the same folder
- Don't delete the "Resources" folder
- Don't delete the "runtimes" folder
- Don't delete any DLL files

===============================================================
REQUIREMENTS:
===============================================================

- Windows 10/11 (64-bit)
- .NET 8.0 Runtime (included)
- USB Drivers for your device
- ADB Debugging enabled on device

===============================================================
FEATURES:
===============================================================

SAMSUNG OPERATIONS:
  - Odin Mode Flashing (BL, AP, CP, CSC)
  - Fix Apps Bancarias 2025
  - Magisk Manager Installer
  - Install Apps Bancarias Peru
  - Remove Samsung Account
  - eSIM to SIM Convert
  - CSC Change
  - Read Device Info

ANDROID OPERATIONS:
  - ADB Read Info
  - Live Screen (scrcpy)
  - Reboot modes
  - FRP Remove
  - Package Manager

MOTOROLA OPERATIONS:
  - OEM Unlock
  - MTK operations

===============================================================
SUPPORT:
===============================================================

Website: LeoPE-GSM.COM
Copyright (C) 2025 LeoPE-GSM.COM
All rights reserved.

===============================================================
DISCLAIMER:
===============================================================

Use this tool at your own risk.
The author is not responsible for any damage to your device.
Make sure you have backups before using any operation.

===============================================================
"@

$readmeContent | Out-File -FilePath (Join-Path $packagePath "README.txt") -Encoding UTF8

Write-Host "      ✓ README.txt creado" -ForegroundColor Green
Write-Host ""

# Calcular tamaño del paquete
$totalSize = (Get-ChildItem -Path $packagePath -Recurse | Measure-Object -Property Length -Sum).Sum
$sizeMB = [math]::Round($totalSize / 1MB, 2)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ✓✓✓ PACKAGE COMPLETADO ✓✓✓" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📦 Ubicación: $packagePath" -ForegroundColor Yellow
Write-Host "📊 Tamaño total: $sizeMB MB" -ForegroundColor Yellow
Write-Host ""
Write-Host "Archivos incluidos:" -ForegroundColor White
Write-Host "  ✓ AREPA-TOOL.exe (con ícono personalizado)" -ForegroundColor Green
Write-Host "  ✓ DLLs necesarias" -ForegroundColor Green
Write-Host "  ✓ Resources/ (imágenes, APKs, herramientas)" -ForegroundColor Green
Write-Host "  ✓ Runtimes/ (.NET dependencies)" -ForegroundColor Green
Write-Host "  ✓ README.txt (instrucciones)" -ForegroundColor Green
Write-Host ""
Write-Host "🎉 Listo para distribuir!" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para crear un ZIP:" -ForegroundColor Yellow
Write-Host "  Compress-Archive -Path '$packagePath' -DestinationPath '$desktopPath\AREPA-TOOL-v1.0.zip'" -ForegroundColor Gray
Write-Host ""

