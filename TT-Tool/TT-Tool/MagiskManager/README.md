# Magisk Manager DenyList - Herramienta Propia

Esta carpeta contiene todo lo necesario para instalar y gestionar el script de Magisk DenyList que oculta el root de más de 150 aplicaciones bancarias y de servicios.

## Archivos Incluidos

### 📄 manager.sh
El script que se ejecuta automáticamente en cada inicio del dispositivo. Configura la DenyList de Magisk para ocultar el root de apps bancarias, Uber, Netflix, etc.

### 🔧 Scripts de Instalación

#### instalar_magisk_manager.bat
Instala el script manager.sh en tu dispositivo Android.

**Requisitos:**
- Dispositivo conectado via USB
- Depuración USB activada
- Magisk instalado
- Acceso root funcionando

**Uso:**
1. Conecta tu dispositivo
2. Ejecuta `instalar_magisk_manager.bat`
3. Sigue las instrucciones
4. El dispositivo se reiniciará automáticamente

#### desinstalar_magisk_manager.bat
Elimina el script del dispositivo.

**Uso:**
1. Ejecuta `desinstalar_magisk_manager.bat`
2. El script será eliminado y el dispositivo se reiniciará

#### verificar_instalacion.bat
Verifica que todo esté instalado correctamente.

**Muestra:**
- Estado del dispositivo
- Versión de Magisk
- Estado del root
- Si el script está instalado
- Estado de la DenyList
- Apps protegidas

#### leer_info_dispositivo.bat
Lee toda la información del dispositivo (igual que la función "Read Info" de MultiUnlock).

**Muestra:**
- Modelo
- País
- Versión de Android
- Baseband
- Serial
- Y más...

## ¿Qué hace el script?

El script `manager.sh`:
1. Activa la DenyList de Magisk
2. Agrega más de 150 aplicaciones a la lista
3. Se ejecuta automáticamente en cada reinicio

**Apps protegidas incluyen:**
- 🏦 Bancos de Perú, Chile, Colombia, Argentina, México, Bolivia
- 💳 Billeteras digitales (Yape, Mercado Pago, etc.)
- 🚗 Apps de transporte (Uber, Didi, Yango)
- 🍔 Apps de delivery (Rappi, PedidosYa)
- 📺 Streaming (Netflix, Prime Video)
- 📱 Servicios de Google

## Comandos ADB Utilizados

```bash
# Verificar dispositivo
adb devices

# Verificar Magisk
adb shell pm list packages com.topjohnwu.magisk

# Verificar root
adb shell su -c "id"

# Subir script
adb push manager.sh /data/local/tmp/manager.sh

# Instalar script
adb shell su -c "mv /data/local/tmp/manager.sh /data/adb/service.d/manager.sh"
adb shell su -c "chown root:everybody /data/adb/service.d/manager.sh"
adb shell su -c "chmod 755 /data/adb/service.d/manager.sh"

# Reiniciar
adb reboot
```

## Seguridad

✅ **Este script es seguro:**
- No envía datos a servidores externos
- No instala software adicional
- Solo modifica la configuración de Magisk
- Es reversible en cualquier momento
- Es código abierto (puedes revisar manager.sh)

## Personalización

Puedes editar `manager.sh` para:
- Agregar más aplicaciones
- Remover aplicaciones que no uses
- Modificar el tiempo de espera inicial

**Formato para agregar apps:**
```bash
magisk --sqlite "REPLACE INTO denylist (package_name, process) VALUES('com.ejemplo.app','com.ejemplo.app');"
```

## Solución de Problemas

### El script no se instala
- Verifica que tengas root activo
- Asegúrate de que Magisk esté instalado
- Revisa que la depuración USB esté activada

### Las apps siguen detectando root
- Reinicia el dispositivo después de instalar
- Verifica que Magisk esté actualizado
- Algunas apps requieren configuración adicional en Magisk

### El dispositivo no reinicia
- Reinicia manualmente
- El script se activará en el próximo inicio

## Créditos

Script extraído y analizado de MultiUnlock v1.0.6
Herramienta creada para uso educativo y personal

## Licencia

Uso personal. El script original pertenece a sus respectivos autores.
