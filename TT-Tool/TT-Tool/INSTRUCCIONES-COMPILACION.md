# 📦 AREPA-TOOL - Instrucciones de Compilación y Distribución

## ✅ CONFIGURACIÓN COMPLETADA

El proyecto ya está completamente configurado con:

- ✅ Ícono personalizado: `Resources/arepatool.ico`
- ✅ Nombre de ejecutable: `AREPA-TOOL.exe`
- ✅ Información del producto configurada
- ✅ Scripts de empaquetado automático
- ✅ Estructura de carpetas organizada

---

## 🚀 MÉTODO 1: Empaquetado Automático (RECOMENDADO)

### Opción A: Doble clic en el archivo BAT

```
1. Ve a la carpeta: TT-Tool/TT-Tool/
2. Haz doble clic en: package-release.bat
3. Espera a que termine
4. ¡Listo! Carpeta creada en tu escritorio
```

### Opción B: Ejecutar PowerShell

```powershell
# Abre PowerShell en: TT-Tool/TT-Tool/
.\package-release.ps1
```

**Resultado:**
- 📁 Carpeta creada en el escritorio: `AREPA-TOOL-v1.0/`
- ✅ Ejecutable con ícono personalizado
- ✅ Todas las DLLs necesarias
- ✅ Carpeta Resources completa
- ✅ README.txt con instrucciones

---

## 🛠️ MÉTODO 2: Compilación Manual

### Paso 1: Compilar en modo Release

```powershell
cd TT-Tool
dotnet build -c Release
```

### Paso 2: Ubicar los archivos

Los archivos compilados están en:
```
TT-Tool/bin/Release/net8.0-windows/
```

### Paso 3: Copiar archivos necesarios

Crea una carpeta y copia:

**✅ Archivos ejecutables:**
- `AREPA-TOOL.exe` ← Ejecutable principal (con ícono)
- `AREPA-TOOL.dll`
- `AREPA-TOOL.runtimeconfig.json`

**✅ DLLs necesarias:**
- `K4os.Compression.LZ4.dll`
- `K4os.Compression.LZ4.Legacy.dll`
- `K4os.Compression.LZ4.Streams.dll`
- `K4os.Hash.xxHash.dll`
- `LibUsbDotNet.LibUsbDotNet.dll`
- `SharpOdinClient.dll`
- `System.CodeDom.dll`
- `System.IO.Pipelines.dll`
- `System.IO.Ports.dll`
- `System.Management.dll`

**✅ Carpetas completas:**
- `Resources/` ← Imágenes, APKs, herramientas
- `runtimes/` ← Dependencias de .NET

---

## 📁 ESTRUCTURA FINAL DEL PAQUETE

```
AREPA-TOOL-v1.0/
├── AREPA-TOOL.exe          ← Ejecutable con ícono
├── AREPA-TOOL.dll
├── AREPA-TOOL.runtimeconfig.json
├── *.dll                   ← Todas las DLLs
├── Resources/
│   ├── arepatool.ico
│   ├── TT-TOOLNEWLOGO.png
│   ├── samsung.png
│   ├── motorola.png
│   ├── androidd.png
│   ├── hhonor.png
│   ├── qualcomm.png
│   ├── magisk_hide_script.sh
│   ├── AppsBancarias/
│   │   ├── BCP.apks
│   │   ├── BBVA.apks
│   │   ├── Interbank.apks
│   │   ├── Yape.apks
│   │   ├── Scotiabank.apk
│   │   ├── Banco de la Nación.apk
│   │   └── izipayYA.apks
│   └── Tools/
│       ├── adb.exe
│       ├── fastboot.exe
│       ├── kitsune.apk
│       ├── ACBridge.apk
│       └── scrcpy-win64-v3.3.3/
└── runtimes/
    ├── win/
    ├── linux-x64/
    └── osx-x64/
```

---

## 🎯 CREAR ZIP PARA DISTRIBUCIÓN

### Método 1: Desde el explorador de Windows

```
1. Click derecho en la carpeta AREPA-TOOL-v1.0
2. Enviar a → Carpeta comprimida
3. Renombrar a: AREPA-TOOL-v1.0.zip
```

### Método 2: Desde PowerShell

```powershell
Compress-Archive -Path "$env:USERPROFILE\Desktop\AREPA-TOOL-v1.0" -DestinationPath "$env:USERPROFILE\Desktop\AREPA-TOOL-v1.0.zip"
```

---

## 📊 TAMAÑO ESTIMADO

- **Carpeta completa:** ~120-150 MB
- **ZIP comprimido:** ~60-80 MB

(El tamaño varía según las APKs incluidas en Resources/AppsBancarias/)

---

## 🎨 VERIFICAR EL ÍCONO

El ejecutable `AREPA-TOOL.exe` ya tiene el ícono `arepatool.ico` integrado.

Para verificar:
1. Abre la carpeta del ejecutable
2. Verás el ícono personalizado en `AREPA-TOOL.exe`
3. Al crear acceso directo en el escritorio, también usará este ícono

---

## 📝 INSTRUCCIONES PARA EL USUARIO FINAL

El archivo `README.txt` que se crea automáticamente incluye:

✅ Requisitos del sistema
✅ Instrucciones de instalación (solo extraer)
✅ Lista completa de características
✅ Información de soporte
✅ Disclaimer

---

## ⚠️ IMPORTANTE

### NO BORRAR ESTOS ARCHIVOS/CARPETAS:

- ❌ NO borrar `Resources/` → Contiene imágenes y herramientas
- ❌ NO borrar `runtimes/` → Necesarias para .NET
- ❌ NO borrar ninguna DLL → Todas son necesarias
- ❌ NO separar archivos → Deben estar juntos

### ARCHIVOS OPCIONALES (puedes borrar para reducir tamaño):

- ✓ `scrcpy-win64-v3.3.3.zip` en Resources/Tools/
- ✓ Archivos `.pdb` (solo para debugging)

---

## 🔄 ACTUALIZAR VERSIÓN

Para actualizar la versión del programa:

1. Editar `TT-Tool.csproj`:
   ```xml
   <Version>1.0.1</Version>
   ```

2. Editar `package-release.ps1`:
   ```powershell
   $outputFolder = "AREPA-TOOL-v1.0.1"
   ```

3. Recompilar con el script de empaquetado

---

## 🎉 ¡LISTO PARA DISTRIBUIR!

El paquete es **100% portable**:
- ✅ No requiere instalación
- ✅ No modifica el registro
- ✅ Se puede ejecutar desde cualquier carpeta
- ✅ Se puede ejecutar desde USB
- ✅ No deja rastros en el sistema

---

## 📞 SOPORTE

**Website:** LeoPE-GSM.COM  
**Copyright:** © 2025 LeoPE-GSM.COM  
**Versión:** 1.0.0

---

## 🛡️ NOTAS DE SEGURIDAD

- Windows Defender puede detectar como "desconocido" al principio
- Esto es normal para aplicaciones sin firma digital
- Para firmar digitalmente se necesita un certificado (opcional)

---

*Documento creado automáticamente por el sistema de empaquetado*

