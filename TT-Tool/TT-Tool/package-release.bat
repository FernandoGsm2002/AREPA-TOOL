@echo off
chcp 65001 >nul
title AREPA-TOOL - Package Release

echo.
echo ========================================
echo   AREPA-TOOL - Package Release
echo ========================================
echo.

powershell.exe -ExecutionPolicy Bypass -File "%~dp0package-release.ps1"

echo.
pause

