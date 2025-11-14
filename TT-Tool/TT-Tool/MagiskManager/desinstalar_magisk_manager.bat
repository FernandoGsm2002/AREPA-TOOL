@echo off
title Desinstalador de Magisk Manager DenyList
color 0C
echo.
echo ========================================
echo   DESINSTALADOR DE MAGISK MANAGER
echo ========================================
echo.
echo Este script eliminara el manager.sh de tu dispositivo
echo.
pause
echo.

REM Ruta al ADB (en la misma carpeta)
set ADB=%~dp0adb.exe

echo [1/3] Verificando dispositivo...
%ADB% devices
echo.

echo [2/3] Eliminando script...
%ADB% shell su -c "rm /data/adb/service.d/manager.sh"
if errorlevel 1 (
    echo ERROR: No se pudo eliminar el archivo
    echo Puede que ya este eliminado o no tengas root
    pause
    exit /b 1
)
echo OK!
echo.

echo [3/3] Reiniciando dispositivo...
%ADB% reboot

echo.
echo ========================================
echo   DESINSTALACION COMPLETADA
echo ========================================
echo.
echo El script ha sido eliminado.
echo Puedes gestionar la DenyList manualmente desde Magisk Manager.
echo.
pause
