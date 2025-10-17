@echo off
setlocal EnableDelayedExpansion

:: ======================================================================
:: PrecisionDrv Filter Driver Installation Script
:: Version 2.7 - FIX: HIDClass removed to avoid double attach
:: ======================================================================

cd /d "%~dp0"
echo Working directory: %cd%

set "DRIVER_NAME=PrecisionDrv"
set "SYS_FILE=%DRIVER_NAME%.sys"
set "DRIVER_SOURCE=%~dp0%SYS_FILE%"
set "DRIVER_PATH_DEST=%SystemRoot%\System32\drivers\%SYS_FILE%"
set "KBD_CLASS_GUID={4d36e96b-e325-11ce-bfc1-08002be10318}"
set "MOU_CLASS_GUID={4d36e96f-e325-11ce-bfc1-08002be10318}"

:: -- *** HIDClass removed! ***
:: set "HID_CLASS_GUID={745a17a0-74d3-11d0-b6fe-00a0c90f57da}"

:: -- Privilege check --
echo Checking administrator privileges...
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo.
    echo ERROR: This script must be run as Administrator.
    pause
    goto :eof
)
echo Privileges OK.

if not exist "%DRIVER_SOURCE%" (
    echo.
    echo ERROR: File "%DRIVER_SOURCE%" not found.
    pause
    goto :eof
)

echo.
echo === STARTING DRIVER INSTALLATION: %DRIVER_NAME% ===
echo.

:: -- Copy driver --
echo [1/3] Copying %SYS_FILE% to %SystemRoot%\System32\drivers...
copy /Y "%DRIVER_SOURCE%" "%DRIVER_PATH_DEST%" >nul
if !errorlevel! neq 0 (
    echo ERROR: Failed to copy the driver file.
    pause
    goto :eof
)
echo Copy completed.
echo.

:: -- Create system service --
echo [2/3] Creating system service '%DRIVER_NAME%'...
sc create %DRIVER_NAME% binPath= "%DRIVER_PATH_DEST%" type= kernel start= system error= normal group= "Extended Base" DisplayName= "PrecisionDrv Upper Filter" >nul
if !errorlevel! neq 0 (
    echo WARNING: The service may already exist.
) else (
    echo Service created successfully.
)
echo.

:: -- Registry update: KEYBOARD --
echo [3/3] Updating registry for Keyboard class...
set "REG_KEY=HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\!KBD_CLASS_GUID!"
set "EXISTING_FILTERS="
for /f "tokens=2,*" %%A in ('reg query "!REG_KEY!" /v UpperFilters 2^>nul') do (
    if "%%A"=="REG_MULTI_SZ" set "EXISTING_FILTERS=%%B"
)

set "FOUND=0"
if defined EXISTING_FILTERS (
    for %%F in (!EXISTING_FILTERS:\0= !) do (
        if /i "%%~F"=="!DRIVER_NAME!" set "FOUND=1"
    )
)

if !FOUND! equ 1 (
    echo The driver is already present in the Keyboard filter list.
) else (
    if defined EXISTING_FILTERS (
        set "NEW_FILTERS=!DRIVER_NAME!\0!EXISTING_FILTERS!"
    ) else (
        set "NEW_FILTERS=!DRIVER_NAME!"
    )
    reg add "!REG_KEY!" /v UpperFilters /t REG_MULTI_SZ /d "!NEW_FILTERS!" /f >nul
    if !errorlevel! equ 0 (
        echo Registry for Keyboard class updated.
    ) else (
        echo ERROR: Unable to update registry for Keyboard class.
    )
)
echo.

:: -- Registry update: MOUSE --
echo [3/3] Updating registry for Mouse class...
set "REG_KEY=HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\!MOU_CLASS_GUID!"
set "EXISTING_FILTERS="
for /f "tokens=2,*" %%A in ('reg query "!REG_KEY!" /v UpperFilters 2^>nul') do (
    if "%%A"=="REG_MULTI_SZ" set "EXISTING_FILTERS=%%B"
)

set "FOUND=0"
if defined EXISTING_FILTERS (
    for %%F in (!EXISTING_FILTERS:\0= !) do (
        if /i "%%~F"=="!DRIVER_NAME!" set "FOUND=1"
    )
)

if !FOUND! equ 1 (
    echo The driver is already present in the Mouse filter list.
) else (
    if defined EXISTING_FILTERS (
        set "NEW_FILTERS=!DRIVER_NAME!\0!EXISTING_FILTERS!"
    ) else (
        set "NEW_FILTERS=!DRIVER_NAME!"
    )
    reg add "!REG_KEY!" /v UpperFilters /t REG_MULTI_SZ /d "!NEW_FILTERS!" /f >nul
    if !errorlevel! equ 0 (
        echo Registry for Mouse class updated.
    ) else (
        echo ERROR: Unable to update registry for Mouse class.
    )
)
echo.

:: -- Verification --
echo Verifying installation...
echo.
sc query !DRIVER_NAME! >nul 2>&1
if !errorlevel! equ 0 (
    echo   Service: OK
) else (
    echo   Service: NOT FOUND
)

reg query "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\!KBD_CLASS_GUID!" /v UpperFilters 2>nul | find "!DRIVER_NAME!" >nul
if !errorlevel! equ 0 (
    echo   Keyboard: OK
) else (
    echo   Keyboard: NOT FOUND
)

reg query "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\!MOU_CLASS_GUID!" /v UpperFilters 2>nul | find "!DRIVER_NAME!" >nul
if !errorlevel! equ 0 (
    echo   Mouse: OK
) else (
    echo   Mouse: NOT FOUND
)

echo.
echo =======================================================
echo.
echo           INSTALLATION COMPLETED!
echo.
echo   The driver has been installed for:
echo   - Keyboard class
echo   - Mouse class
echo.
echo   *** HIDClass REMOVED to avoid duplication issues ***
echo.
echo   PLEASE RESTART YOUR COMPUTER to activate changes.
echo.
echo =======================================================
echo.
pause
endlocal
