@echo off
setlocal

set "SERVER_DIR=%~dp0"
set "ROOT=%SERVER_DIR%.."
set "BIN_DIR=%ROOT%\Bin"
set "SLN=%SERVER_DIR%Server.sln"

if not exist "%BIN_DIR%\App.exe" if not exist "%BIN_DIR%\App.dll" (
    echo Building server solution...
    dotnet build "%SLN%"
    if errorlevel 1 (
        echo Server build failed.
        pause
        exit /b 1
    )
)

cd /d "%BIN_DIR%"

if exist "App.exe" (
    App.exe --AppType=Watcher --StartConfig=StartConfig/Localhost --Console=1
) else (
    dotnet App.dll --AppType=Watcher --StartConfig=StartConfig/Localhost --Console=1
)

pause
