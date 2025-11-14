# 📤 INSTRUCCIONES PARA SUBIR A GITHUB

## ✅ PREPARACIÓN COMPLETADA

El código ya está listo para subir a GitHub con:
- ✅ README.md profesional
- ✅ LICENSE propietaria
- ✅ .gitignore configurado (excluye APKs grandes)
- ✅ Documentación completa

---

## 🚀 SUBIR EL CÓDIGO A GITHUB

### Repositorio Principal: https://github.com/FernandoGsm2002/AREPA-TOOL

### Paso 1: Inicializar Git en el proyecto

Abre PowerShell en la carpeta: `C:\Users\Fernando\Desktop\TT-Tool\`

```powershell
# Ir a la carpeta del proyecto
cd C:\Users\Fernando\Desktop\TT-Tool

# Inicializar repositorio Git
git init

# Agregar el repositorio remoto
git remote add origin https://github.com/FernandoGsm2002/AREPA-TOOL.git

# Configurar rama principal
git branch -M main
```

### Paso 2: Agregar archivos al repositorio

```powershell
# Agregar todos los archivos (respetando .gitignore)
git add .

# Verificar qué archivos se agregarán
git status
```

**IMPORTANTE:** Los siguientes archivos **NO SE SUBIRÁN** (están en .gitignore):
- ❌ APKs de bancos (demasiado grandes)
- ❌ scrcpy-win64-v3.3.3.zip
- ❌ Archivos compilados (bin/, obj/)
- ❌ Archivos temporales

### Paso 3: Hacer el primer commit

```powershell
# Crear el commit inicial
git commit -m "Initial commit: AREPA-TOOL v1.0"
```

### Paso 4: Subir a GitHub

```powershell
# Subir al repositorio remoto
git push -u origin main
```

**Si pide autenticación:**
- Usuario: `FernandoGsm2002`
- Contraseña: Usar **Personal Access Token** (no la contraseña normal)

---

## 🔐 CREAR PERSONAL ACCESS TOKEN (GitHub)

GitHub ya no acepta contraseñas normales para Git. Necesitas un token:

### Paso 1: Ir a GitHub Settings
1. Ve a: https://github.com/settings/tokens
2. Click en **"Generate new token (classic)"**

### Paso 2: Configurar el token
- **Note**: `AREPA-TOOL Git Access`
- **Expiration**: 90 days (o sin expiración)
- **Select scopes**:
  - ✅ `repo` (acceso completo al repositorio)
  - ✅ `workflow` (opcional)

### Paso 3: Generar y copiar
1. Click **"Generate token"**
2. **COPIA EL TOKEN** (solo se muestra una vez)
3. Úsalo como contraseña al hacer `git push`

---

## 🔒 SISTEMA DE CONTROL REMOTO (IMPORTANTE)

Para que el sistema de control remoto funcione, necesitas **OTRO repositorio**:

### Crear repositorio de configuración

#### 1. Crear nuevo repositorio
- Nombre: `arepa-tool-config`
- Visibilidad: **PÚBLICO** ⚠️
- URL: `https://github.com/FernandoGsm2002/arepa-tool-config`

#### 2. Crear archivo `license.json`

En el nuevo repositorio, crear archivo con este contenido:

```json
{
  "enabled": true,
  "message": "",
  "welcomeMessage": "¡Bienvenido a AREPA-TOOL v1.0!",
  "expirationDate": null,
  "minimumVersion": "1.0.0",
  "updateUrl": ""
}
```

#### 3. Obtener URL Raw

1. Abre `license.json` en GitHub
2. Click en botón **"Raw"**
3. Copia la URL (será algo como):
```
https://raw.githubusercontent.com/FernandoGsm2002/arepa-tool-config/main/license.json
```

#### 4. Actualizar LicenseManager.cs

Edita el archivo: `TT-Tool/Managers/LicenseManager.cs`

Línea 11, cambia:
```csharp
private const string LICENSE_URL = "https://raw.githubusercontent.com/FernandoGsm2002/arepa-tool-config/main/license.json";
```

#### 5. Commit y push del cambio

```powershell
git add Managers/LicenseManager.cs
git commit -m "Configure license URL"
git push
```

---

## 📁 QUÉ SE SUBE A GITHUB

### ✅ Archivos que SÍ se suben:
- ✅ Todo el código fuente (.cs)
- ✅ Archivos de proyecto (.csproj, .sln)
- ✅ Documentación (.md)
- ✅ Imágenes de UI (.png de marcas)
- ✅ Ícono (.ico)
- ✅ Scripts (.sh, .bat, .ps1)
- ✅ Configuración (.gitignore, LICENSE)

### ❌ Archivos que NO se suben:
- ❌ APKs de bancos (demasiado grandes para GitHub)
- ❌ Archivos compilados (bin/, obj/, .exe, .dll)
- ❌ scrcpy-win64-v3.3.3.zip (muy grande)
- ❌ Archivos temporales

---

## 📝 NOTA IMPORTANTE SOBRE APKs

Los APKs de bancos **NO ESTARÁN EN GITHUB** porque:
1. Son muy grandes (superan límites de GitHub)
2. Son específicos para tu distribución
3. Los usuarios los obtendrán del paquete compilado

**Solución:**
- GitHub: Solo código fuente
- Distribución: Paquete completo con APKs

---

## 🔄 ACTUALIZACIONES FUTURAS

Para subir cambios al código:

```powershell
# Ver archivos modificados
git status

# Agregar cambios
git add .

# Crear commit
git commit -m "Descripción de los cambios"

# Subir a GitHub
git push
```

---

## 📊 ESTRUCTURA EN GITHUB

```
AREPA-TOOL/                     (Repositorio principal)
├── README.md                   ✅
├── LICENSE                     ✅
├── .gitignore                  ✅
├── TT-Tool.sln                 ✅
└── TT-Tool/
    ├── TT-Tool/
    │   ├── Brands/             ✅
    │   ├── Managers/           ✅
    │   ├── Controls/           ✅
    │   ├── Resources/          ✅ (sin APKs grandes)
    │   ├── Forms/              ✅
    │   ├── *.cs                ✅
    │   └── TT-Tool.csproj      ✅
    ├── INSTRUCCIONES-*.md      ✅
    ├── SISTEMA-CONTROL-*.md    ✅
    └── package-release.*       ✅

arepa-tool-config/              (Repositorio de control)
└── license.json                ✅
```

---

## 🎯 VERIFICACIÓN FINAL

Antes de distribuir, verifica:

1. ✅ Código subido a GitHub
2. ✅ Repositorio de configuración creado
3. ✅ URL de licencia actualizada en código
4. ✅ Compilación exitosa
5. ✅ Control remoto probado

---

## ⚠️ RECORDATORIOS DE SEGURIDAD

- 🔒 **NO subas contraseñas** o API keys al código
- 🔒 **NO subas archivos personales** (.user, .suo)
- 🔒 **Repositorio de config debe ser PÚBLICO** para que funcione
- 🔒 **Guarda tu Personal Access Token** en un lugar seguro

---

## 🆘 SOLUCIÓN DE PROBLEMAS

### Error: "Permission denied"
**Solución:** Usa Personal Access Token en lugar de contraseña

### Error: "Remote origin already exists"
```powershell
git remote remove origin
git remote add origin https://github.com/FernandoGsm2002/AREPA-TOOL.git
```

### Error: "Files too large"
**Solución:** Los APKs grandes ya están excluidos en .gitignore

### ¿Cómo ver qué archivos se subirán?
```powershell
git status
git diff --staged
```

---

## 📞 CONTACTO

Si tienes problemas con GitHub:
- GitHub Docs: https://docs.github.com/
- Git Tutorial: https://git-scm.com/book/es/v2

---

¡LISTO PARA SUBIR A GITHUB! 🚀

