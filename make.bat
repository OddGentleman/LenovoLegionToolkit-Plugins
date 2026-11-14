@echo off
set "PLUGIN_NAME=CustomFanCurve"
set "DLL_NAME=CustomFanCurve"

echo Building %PLUGIN_NAME% in Release configuration...
dotnet publish %PLUGIN_NAME%\%DLL_NAME%.csproj -c Release -o %PLUGIN_NAME%\bin\Publish > %PLUGIN_NAME%\build.log 2>&1 || (type %PLUGIN_NAME%\build.log && exit /b)

echo.
echo Copying output DLLs to LocalAppData (replacing original CustomFanCurve)...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$running = Get-Process -Name LenovoLegionToolkit -ErrorAction SilentlyContinue; if ($running) { $running | Stop-Process -Force; $running | Wait-Process -Timeout 10 -ErrorAction SilentlyContinue }"

set "PLUGIN_DEST=%LOCALAPPDATA%\LenovoLegionToolkit\Plugins\%PLUGIN_NAME%"
if not exist "%PLUGIN_DEST%" mkdir "%PLUGIN_DEST%"
set "PLUGIN_SOURCE=%PLUGIN_NAME%\bin\Publish\%DLL_NAME%.dll"
set "PLUGIN_TARGET=%PLUGIN_DEST%\%DLL_NAME%.dll"
powershell -NoProfile -ExecutionPolicy Bypass -Command "$source = $env:PLUGIN_SOURCE; $destination = $env:PLUGIN_TARGET; for ($attempt = 1; $attempt -le 10; $attempt++) { try { Copy-Item -LiteralPath $source -Destination $destination -Force; exit 0 } catch { if ($attempt -eq 10) { throw }; Start-Sleep -Seconds 1 } }"
if errorlevel 1 (
    echo Failed to replace CustomFanCurve.dll. Close Lenovo Legion Toolkit and run make.bat again.
    exit /b 1
)
if exist "%LOCALAPPDATA%\LenovoLegionToolkit\Plugins\%PLUGIN_NAME%\UniversalFanControl.Lib.dll" (
    del /q "%LOCALAPPDATA%\LenovoLegionToolkit\Plugins\%PLUGIN_NAME%\UniversalFanControl.Lib.dll"
)

rmdir /s /q "..\BuildLLT\obj" 2>nul
rmdir /s /q "..\BuildLLT\bin" 2>nul
rmdir /s /q "%PLUGIN_NAME%\bin" 2>nul
rmdir /s /q "%PLUGIN_NAME%\obj" 2>nul

echo.
echo Build and deployment completed successfully!
