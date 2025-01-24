@echo off
setlocal

:: Check if a file was dragged onto the script
if "%~1"=="" (
    echo No file was dragged onto the script.
    echo Please drag and drop Pulsar's executable onto this script.
    pause
    exit /b
)

:: Set appPath to the dragged file's path
set appPath=%~1

:: Register the "pulsar" scheme
reg add "HKCU\Software\Classes\quasar" /ve /d "URL:Pulsar Protocol" /f
reg add "HKCU\Software\Classes\quasar\shell\open\command" /ve /d "\"%appPath%\" \"%%1\"" /f

echo Registered 'quasar:' URL scheme.

:: Confirm success
echo.
echo Custom URL scheme 'quasar:' have been registered successfully for the application:
echo %appPath%
pause
