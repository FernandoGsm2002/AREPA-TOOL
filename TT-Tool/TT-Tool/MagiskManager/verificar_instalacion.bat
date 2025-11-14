@echo off
title Verificador de Magisk Manager
color 0B
echo.
echo ========================================
echo   VERIFICADOR DE INSTALACION
echo ========================================
echo.

REM Ruta al ADB (en la misma carpeta)
set ADB=%~dp0adb.exe

echo [*] Verificando dispositivo...
%ADB% devices
echo.

echo [*] Verificando Magisk...
%ADB% shell pm list packages com.topjohnwu.magisk
echo.

echo [*] Version de Magisk:
%ADB% shell dumpsys package com.topjohnwu.magisk | find "versionName"
echo.

echo [*] Verificando root...
%ADB% shell su -c "id"
echo.

echo [*] Verificando script instalado...
%ADB% shell su -c "ls -la /data/adb/service.d/manager.sh"
echo.

echo [*] Primeras lineas del script:
%ADB% shell su -c "head -n 5 /data/adb/service.d/manager.sh"
echo.

echo [*] Estado de DenyList en Magisk:
%ADB% shell su -c "magisk --sqlite 'SELECT * FROM settings WHERE key=\"denylist\";'"
echo.

echo [*] Apps en DenyList (primeras 10):
%ADB% shell su -c "magisk --sqlite 'SELECT * FROM denylist LIMIT 10;'"
echo.

echo ========================================
echo   VERIFICACION COMPLETADA
echo ========================================
echo.
pause
