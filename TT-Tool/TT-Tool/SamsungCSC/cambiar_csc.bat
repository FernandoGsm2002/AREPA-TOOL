@echo off
title Samsung CSC Changer
color 0B
echo.
echo ========================================
echo   SAMSUNG CSC CHANGER
echo ========================================
echo.

REM Ruta al ADB
set ADB=..\MultiUnlock\Binaries\adb.exe

echo [1/5] Verificando dispositivo...
%ADB% devices
if errorlevel 1 (
    echo ERROR: No se pudo conectar con ADB
    pause
    exit /b 1
)
echo OK!
echo.

echo [2/5] Leyendo lista de CSC disponibles...
%ADB% shell "cat product/omc/sales_code_list.dat" > csc_list.txt
type csc_list.txt
echo.

echo [3/5] CSC disponibles:
for /f "tokens=*" %%a in (csc_list.txt) do (
    echo   - %%a
)
echo.

set /p nuevo_csc="Ingresa el CSC que deseas (ej: ZTO, CHO, PEO): "
if "%nuevo_csc%"=="" (
    echo ERROR: No ingresaste un CSC
    pause
    exit /b 1
)

echo.
echo [4/5] Cambiando CSC a %nuevo_csc%...
echo.
echo IMPORTANTE: El dispositivo debe estar en modo MTP
echo y con depuracion USB activada.
echo.
pause

REM Buscar puerto COM
echo Buscando puerto COM de Samsung...
for /f "tokens=1" %%p in ('mode ^| findstr /C:"COM"') do (
    echo Probando %%p...
    set PUERTO=%%p
)

if "%PUERTO%"=="" (
    echo ERROR: No se encontro puerto COM de Samsung
    echo Asegurate de que el dispositivo este conectado en modo MTP
    pause
    exit /b 1
)

echo Puerto encontrado: %PUERTO%
echo.

echo Ejecutando cambio de CSC...
echo Este proceso puede tardar unos segundos...
echo.

REM Llamar al programa C# que hara el cambio
csc_changer.exe %PUERTO% %nuevo_csc%

if errorlevel 1 (
    echo.
    echo ERROR: No se pudo cambiar el CSC
    echo Verifica que el dispositivo este en modo correcto
    pause
    exit /b 1
)

echo.
echo [5/5] CSC cambiado exitosamente!
echo El dispositivo se reiniciara automaticamente.
echo.
pause
