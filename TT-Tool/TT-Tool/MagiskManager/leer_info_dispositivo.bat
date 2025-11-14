@echo off
title Lector de Informacion del Dispositivo
color 0E
echo.
echo ========================================
echo   INFORMACION DEL DISPOSITIVO
echo ========================================
echo.

REM Ruta al ADB (en la misma carpeta)
set ADB=%~dp0adb.exe

echo [*] Verificando dispositivo...
%ADB% devices
echo.

echo Modelo:
%ADB% shell getprop ro.product.model
echo.

echo Pais:
%ADB% shell getprop ro.csc.country_code
echo.

echo Zona Horaria:
%ADB% shell getprop persist.sys.timezone
echo.

echo Version Android:
%ADB% shell getprop ro.build.version.release
echo.

echo SDK:
%ADB% shell getprop ro.build.version.sdk
echo.

echo Fecha de Build:
%ADB% shell getprop ro.build.date
echo.

echo Version Incremental:
%ADB% shell getprop ro.build.version.incremental
echo.

echo Baseband:
%ADB% shell getprop gsm.version.baseband
echo.

echo Plataforma:
%ADB% shell getprop ro.board.platform
echo.

echo Serial:
%ADB% shell getprop ro.serialno
echo.

echo Tipo de Red:
%ADB% shell getprop gsm.network.type
echo.

echo Estado SIM:
%ADB% shell getprop gsm.sim.state
echo.

echo Parche de Seguridad:
%ADB% shell getprop ro.build.version.security_patch
echo.

echo EM DID:
%ADB% shell getprop ro.boot.em.did
echo.

echo ========================================
pause
