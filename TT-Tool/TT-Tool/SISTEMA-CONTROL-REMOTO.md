# 🔒 SISTEMA DE CONTROL REMOTO - AREPA-TOOL

## 📋 ¿QUÉ ES ESTO?

Este sistema te permite **activar o desactivar** todas las copias de AREPA-TOOL remotamente, **sin que los usuarios tengan que hacer nada**.

### ✅ Ventajas:
- ✅ Desactiva todas las copias con un solo cambio
- ✅ Controla el período gratuito
- ✅ Puedes mostrar mensajes personalizados
- ✅ No requiere actualización de la app
- ✅ Funciona automáticamente al iniciar

---

## 🚀 CONFIGURACIÓN INICIAL (Solo una vez)

### Paso 1: Crear un repositorio en GitHub

1. Ve a: https://github.com/new
2. Nombre del repositorio: `arepa-tool-config` (o el que quieras)
3. Visibilidad: **Público** (importante)
4. Crea el repositorio

### Paso 2: Crear el archivo de control

1. En tu repositorio, crea un archivo llamado: `license.json`
2. Copia este contenido inicial (app ACTIVADA):

```json
{
  "enabled": true,
  "message": "",
  "welcomeMessage": "¡Bienvenido a AREPA-TOOL!\nPeríodo gratuito activo.",
  "expirationDate": null,
  "minimumVersion": "1.0.0",
  "updateUrl": ""
}
```

3. Guarda el archivo (Commit)

### Paso 3: Obtener la URL Raw

1. En GitHub, abre el archivo `license.json`
2. Click en botón **"Raw"**
3. Copia la URL (será algo como):
   ```
   https://raw.githubusercontent.com/TU-USUARIO/arepa-tool-config/main/license.json
   ```

### Paso 4: Configurar en tu código

Edita el archivo: `Managers/LicenseManager.cs`

Busca esta línea:
```csharp
private const string LICENSE_URL = "https://raw.githubusercontent.com/TU-USUARIO/arepa-tool-config/main/license.json";
```

Reemplaza con TU URL del paso 3.

### Paso 5: Compilar

Una vez configurado, compila tu aplicación:
```
dotnet build -c Release
```

---

## 🎮 CÓMO USAR EL SISTEMA

### ✅ MANTENER LA APP ACTIVA (Modo por defecto)

Tu archivo `license.json` debe verse así:

```json
{
  "enabled": true,
  "message": "",
  "welcomeMessage": "¡Bienvenido a AREPA-TOOL!",
  "expirationDate": null,
  "minimumVersion": "1.0.0",
  "updateUrl": ""
}
```

**Resultado:** Todas las apps funcionan normalmente.

---

### ❌ DESACTIVAR TODAS LAS APPS

Cuando quieras cerrar el acceso, cambia `license.json` a:

```json
{
  "enabled": false,
  "message": "El período gratuito ha finalizado.\n\nPara continuar usando AREPA-TOOL, visita:\nhttps://leopegsm.com",
  "welcomeMessage": "",
  "expirationDate": null,
  "minimumVersion": "1.0.0",
  "updateUrl": "https://leopegsm.com/downloads"
}
```

**Resultado:** 
- ❌ La app se cierra automáticamente
- 📨 Muestra tu mensaje personalizado
- 🔗 Puede incluir enlace a nueva versión

---

### 📅 CONFIGURAR FECHA DE EXPIRACIÓN

Para que expire automáticamente en una fecha:

```json
{
  "enabled": true,
  "message": "El período de prueba ha finalizado.\n\nVisita: https://leopegsm.com",
  "welcomeMessage": "¡Bienvenido! Período de prueba activo hasta 31/01/2025",
  "expirationDate": "2025-01-31T23:59:59",
  "minimumVersion": "1.0.0",
  "updateUrl": "https://leopegsm.com"
}
```

**Resultado:**
- ✅ Funciona hasta la fecha especificada
- ❌ Después de esa fecha, se bloquea automáticamente
- 📨 Muestra tu mensaje personalizado

---

## 🔄 EJEMPLO DE USO REAL

### Escenario 1: Lanzamiento Gratuito por 1 Mes

```json
{
  "enabled": true,
  "message": "",
  "welcomeMessage": "¡Bienvenido a AREPA-TOOL!\nPeríodo gratuito hasta el 31 de Diciembre 2025",
  "expirationDate": "2025-12-31T23:59:59",
  "minimumVersion": "1.0.0",
  "updateUrl": ""
}
```

### Escenario 2: Cerrar Acceso Inmediatamente

```json
{
  "enabled": false,
  "message": "Esta versión gratuita ha sido descontinuada.\n\nContacta en:\nhttps://t.me/LeoPEGSM\n\nO visita:\nhttps://leopegsm.com",
  "welcomeMessage": "",
  "expirationDate": null,
  "minimumVersion": "1.0.0",
  "updateUrl": "https://leopegsm.com/downloads"
}
```

### Escenario 3: Forzar Actualización

```json
{
  "enabled": false,
  "message": "Nueva versión disponible!\n\nEsta versión ya no es compatible.\n\nDescarga la versión 2.0 desde:\nhttps://leopegsm.com/downloads",
  "welcomeMessage": "",
  "expirationDate": null,
  "minimumVersion": "2.0.0",
  "updateUrl": "https://leopegsm.com/downloads"
}
```

---

## 🛠️ MODIFICAR EL CONTROL

### Cambiar en GitHub (Recomendado)

1. Ve a tu repositorio en GitHub
2. Abre `license.json`
3. Click en el ícono de lápiz (Edit)
4. Modifica el JSON
5. **Commit changes**

**Los cambios son INMEDIATOS:**
- ⚡ Sin recompilar
- ⚡ Sin redistribuir
- ⚡ Afecta a TODAS las copias al iniciar

---

## 📊 PARÁMETROS DEL JSON

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `enabled` | boolean | `true` = app funciona, `false` = app bloqueada |
| `message` | string | Mensaje que se muestra si está desactivada |
| `welcomeMessage` | string | Mensaje de bienvenida al iniciar (opcional) |
| `expirationDate` | datetime | Fecha de expiración automática (opcional) |
| `minimumVersion` | string | Versión mínima requerida (futuro) |
| `updateUrl` | string | URL de descarga de nueva versión (futuro) |

---

## 🔒 SEGURIDAD Y PRIVACIDAD

### ✅ Lo que hace el sistema:
- ✅ Consulta un archivo JSON público (5 segundos timeout)
- ✅ Lee configuración (enabled/disabled)
- ✅ NO envía información del usuario
- ✅ NO recopila datos
- ✅ NO rastrea dispositivos

### ⚡ Fail-Safe (Modo seguro):
Si hay error de conexión, la app **sigue funcionando**:
- ❌ No hay internet → App funciona
- ❌ GitHub caído → App funciona
- ❌ Timeout → App funciona

Esto evita bloqueos accidentales por problemas de red.

---

## 🎯 CASOS DE USO

### 1. **Lanzamiento Beta/Prueba**
```json
{
  "enabled": true,
  "expirationDate": "2025-02-01T00:00:00",
  "welcomeMessage": "Versión Beta - Expira: 01/02/2025"
}
```

### 2. **Versión Crackeada/Pirata**
Si detectas que alguien redistribuye tu app modificada:
```json
{
  "enabled": false,
  "message": "Esta versión ha sido detectada como copia no autorizada.\n\nDescarga la versión oficial desde:\nhttps://leopegsm.com"
}
```

### 3. **Mantenimiento Programado**
```json
{
  "enabled": false,
  "message": "Mantenimiento en progreso.\n\nLa herramienta estará disponible en unas horas.\n\nGracias por tu paciencia."
}
```

### 4. **Nuevo Modelo de Negocio**
```json
{
  "enabled": false,
  "message": "¡AREPA-TOOL ahora es premium!\n\nCaracterísticas mejoradas disponibles.\n\nMás información:\nhttps://leopegsm.com/premium"
}
```

---

## 💡 TIPS Y RECOMENDACIONES

### ✅ BUENAS PRÁCTICAS:

1. **Siempre prueba antes:** Haz cambios en una versión de prueba primero
2. **Mensajes claros:** Explica por qué y dónde ir
3. **Backup del JSON:** Guarda copias de tu configuración
4. **Monitoreo:** Revisa que GitHub esté activo
5. **Comunicación:** Avisa en redes sociales antes de desactivar

### ⚠️ PRECAUCIONES:

- ❌ NO desactives sin avisar si tienes muchos usuarios
- ❌ NO uses mensajes agresivos
- ❌ NO cambies constantemente (confunde usuarios)
- ❌ NO olvides la URL correcta en tu código

---

## 🆘 SOLUCIÓN DE PROBLEMAS

### Problema: Los cambios no se aplican

**Posibles causas:**
1. URL incorrecta en `LicenseManager.cs`
2. Repositorio privado (debe ser público)
3. Cache de GitHub (espera 1-2 minutos)
4. Sintaxis incorrecta en JSON

**Solución:**
- Verifica la URL Raw en un navegador
- Asegúrate que el repositorio sea público
- Usa JSONLint para validar tu JSON

### Problema: App no se conecta

**Esto es normal y esperado:**
- La app funciona sin conexión (fail-safe)
- Solo verifica al iniciar
- Si no hay internet, no bloquea

---

## 📚 RECURSOS ADICIONALES

### Validar tu JSON:
- https://jsonlint.com/

### Probar fechas:
- Usa formato: `"2025-12-31T23:59:59"`
- Zona horaria: UTC

### Hosting alternativo (sin GitHub):
- Pastebin Raw
- Tu propio servidor web
- Google Drive (público)
- Cualquier URL que devuelva JSON

---

## 📞 CONTACTO

**Desarrollador:** LeoPE-GSM.COM  
**Versión del sistema:** 1.0  
**Última actualización:** 2025

---

## 🎉 ¡LISTO PARA USAR!

Una vez configurado:
1. ✅ Compila tu app
2. ✅ Distribuye
3. ✅ Controla remotamente cuando quieras
4. ✅ Sin recompilar
5. ✅ Sin redistribuir

**¡Simple, efectivo y profesional!** 🚀

