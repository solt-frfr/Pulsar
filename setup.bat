@echo off
setlocal

:: Check if a file was dragged onto the script
if "%~1"=="" (
    echo No file was dragged onto the script.
    echo Please drag and drop your application's executable onto this script.
    pause
    exit /b
)

:: Set appPath to the dragged file's path
set appPath=%~1

:: Register the "quasar" scheme
reg add "HKCU\Software\Classes\quasar" /ve /d "URL:Quasar Protocol" /f
reg add "HKCU\Software\Classes\quasar" /v "URL Protocol" /f
reg add "HKCU\Software\Classes\quasar\shell\open\command" /ve /d "\"%appPath%\" \"%%1\"" /f

echo "Registered 'quasar:' URL scheme."

:: Register the "pulsar" scheme
reg add "HKCU\Software\Classes\pulsar" /ve /d "URL:Pulsar Protocol" /f
reg add "HKCU\Software\Classes\pulsar\shell\open\command" /ve /d "\"%appPath%\" \"%%1\"" /f

echo Registered 'pulsar:' URL scheme.

:: Confirm success
echo.
echo Custom URL schemes 'quasar:' and 'pulsar:' have been registered successfully for the application:
echo %appPath%
pause
