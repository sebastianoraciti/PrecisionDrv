@echo off
setlocal
echo === Verifying driver signature ===
set DRIVER=PrecisionDrv.sys

if not exist "%DRIVER%" (
    echo ERROR: %DRIVER% not found.
    exit /b 1
)

signtool verify /pa /v "%DRIVER%"
pause
endlocal
