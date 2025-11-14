@echo off
title Magisk Manager DenyList - Menu Principal
color 0B

REM ========================================
REM Sistema de Proteccion con Contraseña
REM ========================================
set "PASSWORD=dixongay"
set "input_pass="

echo Ingresa la contraseña para acceder:
echo.

REM Capturar contraseña sin mostrarla
setlocal enabledelayedexpansion
set "psCommand=powershell -Command "$password = Read-Host -AsSecureString; $BSTR=[System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password); [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)""
for /f "usebackq delims=" %%p in (`%psCommand%`) do set "input_pass=%%p"

if not "!input_pass!"=="%PASSWORD%" (
    color 0C
    echo.
    echo ========================================
    echo   ACCESO DENEGADO - Contraseña Incorrecta
    echo ========================================
    echo.
    timeout /t 3
    exit
)
endlocal & set "input_pass=%input_pass%"

color 0B
cls
echo.
echo ========================================
echo   ACCESO CONCEDIDO
echo ========================================
timeout /t 1
cls

:menu
cls
echo.
echo ========================================
echo   MAGISK MANAGER DENYLIST TOOL
echo ========================================
echo.
echo   1. Instalar Magisk Manager
echo   2. Desinstalar Magisk Manager
echo   3. Verificar Instalacion
echo   4. Leer Info del Dispositivo
echo   5. Ver README
echo   6. Salir
echo.
echo ========================================
echo.
set /p opcion="Selecciona una opcion (1-6): "

if "%opcion%"=="1" goto instalar
if "%opcion%"=="2" goto desinstalar
if "%opcion%"=="3" goto verificar
if "%opcion%"=="4" goto info
if "%opcion%"=="5" goto readme
if "%opcion%"=="6" goto salir
goto menu

:instalar
cls
call instalar_magisk_manager.bat
pause
goto menu

:desinstalar
cls
call desinstalar_magisk_manager.bat
pause
goto menu

:verificar
cls
call verificar_instalacion.bat
pause
goto menu

:info
cls
call leer_info_dispositivo.bat
pause
goto menu

:readme
cls
type README.md
echo.
pause
goto menu

:salir
echo.
echo Gracias por usar Magisk Manager DenyList Tool!
echo.
timeout /t 2
exit
