## Samsung CSC Changer

Herramienta para cambiar el código CSC (Consumer Software Customization) de dispositivos Samsung.

## ¿Qué es el CSC?

El CSC determina:
- Región del dispositivo
- Idiomas disponibles
- Apps preinstaladas
- Configuraciones de operador
- Actualizaciones OTA disponibles

## Requisitos

- Dispositivo Samsung conectado via USB
- Depuración USB activada
- Modo MTP (Transferencia de archivos) activado
- ADB instalado
- Python 3.x con pyserial (para versión Python)

## Instalación

### Opción 1: Python (Recomendado)

```bash
pip install pyserial
```

### Opción 2: C#

Compilar el archivo CSCChanger.cs

## Uso

### Versión Python:

```bash
python cambiar_csc.py
```

El script:
1. Detecta automáticamente el puerto COM de Samsung
2. Lee los CSC disponibles en tu dispositivo
3. Te permite seleccionar el nuevo CSC
4. Realiza el cambio y reinicia el dispositivo

### Versión Batch:

```cmd
cambiar_csc.bat
```

## Comandos AT Utilizados

```
AT+SWATD=0          # Cambiar a modo DDEXE
AT+ACTIVATE=0,0,0   # Activar modo
AT+SWATD=1          # Cambiar a modo ATD
AT+PRECONFG=2,XXX   # Configurar nuevo CSC
AT+PRECONFG=1,0     # Verificar CSC actual
AT+CFUN=1,1         # Reiniciar dispositivo
```

## CSC Comunes

### América Latina
- **CHO** - Chile (Claro, Entel, Movistar)
- **ZTO** - Brasil
- **PEO** - Perú
- **ARO** - Argentina
- **COO** - Colombia
- **MXO** - México
- **GTO** - Guatemala
- **EON** - Ecuador

### Otros
- **TTT** - Trinidad y Tobago
- **BVO** - Bolivia
- **UPO** - Paraguay
- **UYO** - Uruguay
- **TPA** - Panamá

## Proceso Técnico

1. **Lectura de CSC disponibles** (via ADB)
   ```bash
   adb shell cat product/omc/sales_code_list.dat
   ```

2. **Comunicación COM** (via puerto serial)
   - Abre puerto COM del modem Samsung
   - Envía comandos AT para cambiar configuración
   - Verifica el cambio
   - Reinicia el dispositivo

3. **Reinicio automático**
   - El dispositivo se reinicia para aplicar cambios
   - El nuevo CSC se activa después del reinicio

## Solución de Problemas

### No se encuentra el puerto COM
- Verifica que el dispositivo esté en modo MTP
- Instala los drivers de Samsung
- Revisa en Administrador de Dispositivos → Puertos COM

### Error "PROTECTED_NO_TOK"
- Este error es normal en algunos comandos
- No afecta el cambio de CSC
- El proceso continúa normalmente

### El CSC no cambia
- Verifica que el CSC esté en la lista de disponibles
- Algunos dispositivos tienen CSC bloqueados
- Intenta con otro CSC de la lista

### El dispositivo no reinicia
- Reinicia manualmente
- El CSC se aplicará en el próximo inicio

## Advertencias

⚠️ **IMPORTANTE:**
- Hacer backup antes de cambiar CSC
- Algunos CSC pueden no ser compatibles
- El cambio de CSC puede resetear algunas configuraciones
- No desconectes el dispositivo durante el proceso

## Reversión

Para volver al CSC original:
1. Ejecuta el script nuevamente
2. Selecciona el CSC original
3. El dispositivo se reiniciará con el CSC anterior

## Análisis del Protocolo

Basado en la captura con Frida, el proceso es:

```
[COM TX] AT+SWATD=0              → Modo DDEXE
[COM RX] [DR]: CHANGE TO DDEXE

[COM TX] AT+ACTIVATE=0,0,0       → Activar
[COM RX] +ACTIVATE:0,OK

[COM TX] AT+SWATD=1              → Modo ATD
[COM RX] [DR]: CHANGE TO ATD

[COM TX] AT+PRECONFG=2,ZTO       → Configurar CSC
[COM RX] +PRECONFG:2,OK

[COM TX] AT+PRECONFG=1,0         → Verificar
[COM RX] +PRECONFG:1,ZTO

[COM TX] AT+CFUN=1,1             → Reiniciar
```

## Créditos

- Protocolo extraído mediante ingeniería inversa con Frida
- Basado en SamFwTool
- Herramienta educativa para uso personal

## Licencia

Uso personal y educativo.
