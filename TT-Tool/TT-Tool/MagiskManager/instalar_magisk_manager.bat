@echo off
title Instalador de Magisk Manager DenyList
color 0A
echo.
echo ========================================
echo   INSTALADOR DE MAGISK MANAGER
echo ========================================
echo.
echo Este script instalara el manager.sh en tu dispositivo
echo que oculta Magisk de apps bancarias y servicios.
echo.
echo Asegurate de tener:
echo  - Dispositivo conectado via USB
echo  - Depuracion USB activada
echo  - Magisk instalado
echo  - Acceso root funcionando
echo.
pause
echo.

REM Ruta al ADB (en la misma carpeta)
set ADB=%~dp0adb.exe

echo [1/8] Verificando dispositivo...
%ADB% devices
if errorlevel 1 (
    echo ERROR: No se pudo conectar con ADB
    pause
    exit /b 1
)
echo OK!
echo.

echo [2/8] Verificando Magisk...
%ADB% shell pm list packages com.topjohnwu.magisk | find "magisk" >nul
if errorlevel 1 (
    echo ADVERTENCIA: Magisk no detectado
    echo El script se instalara pero no funcionara sin Magisk
    pause
)
echo OK!
echo.

echo [3/8] Verificando acceso root...
%ADB% shell su -c "id" | find "uid=0" >nul
if errorlevel 1 (
    echo ERROR: No hay acceso root
    echo Asegurate de que Magisk este instalado y funcionando
    pause
    exit /b 1
)
echo OK!
echo.

echo [4/8] Subiendo script al dispositivo...
%ADB% push manager.sh /data/local/tmp/manager.sh
if errorlevel 1 (
    echo ERROR: No se pudo subir el archivo
    pause
    exit /b 1
)
echo OK!
echo.

echo [5/8] Moviendo a carpeta de servicios...
%ADB% shell su -c "mv /data/local/tmp/manager.sh /data/adb/service.d/manager.sh"
if errorlevel 1 (
    echo ERROR: No se pudo mover el archivo
    pause
    exit /b 1
)
echo OK!
echo.

echo [6/8] Configurando permisos...
%ADB% shell su -c "chown root:everybody /data/adb/service.d/manager.sh"
%ADB% shell su -c "chmod 755 /data/adb/service.d/manager.sh"
echo OK!
echo.

echo [7/8] Verificando instalacion...
%ADB% shell su -c "ls -la /data/adb/service.d/manager.sh"
echo.

echo [8/8] Reiniciando dispositivo...
echo El dispositivo se reiniciara en 5 segundos...
timeout /t 5
%ADB% reboot

echo.
echo ========================================
echo   INSTALACION COMPLETADA
echo ========================================
echo.
echo El script se ejecutara automaticamente
echo en cada reinicio del dispositivo.
echo.
echo Apps protegidas: 150+ (bancos, Uber, Netflix, etc.)
echo.
pause
