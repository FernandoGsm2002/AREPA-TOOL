INSTRUCCIONES PARA AGREGAR ADB Y FASTBOOT:

1. Descargar Android Platform Tools desde:
   https://developer.android.com/studio/releases/platform-tools

2. Extraer el ZIP y copiar los siguientes archivos a esta carpeta:
   - adb.exe
   - AdbWinApi.dll
   - AdbWinUsbApi.dll
   - fastboot.exe

3. Los archivos deben quedar así:
   Resources/Tools/
   ├── adb.exe
   ├── AdbWinApi.dll
   ├── AdbWinUsbApi.dll
   ├── fastboot.exe
   └── README.txt (este archivo)

4. La aplicación usará estos ejecutables automáticamente.

NOTA: Si no copias estos archivos, la aplicación intentará usar ADB/Fastboot 
del PATH del sistema (si están instalados).

