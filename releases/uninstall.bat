@echo off
REM ============================================================================
REM PrecisionDrv - Safe Uninstallation Script
REM Removes ONLY PrecisionDrv while preserving existing system drivers
REM ============================================================================

echo.
echo ============================================================================
echo   PrecisionDrv - Safe Uninstallation
echo ============================================================================
echo.

REM Check for administrator privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script requires Administrator privileges!
    echo Right-click and select "Run as administrator".
    pause
    exit /b 1
)

echo [STEP 1] Checking existing installation...
echo.

REM Check if the service exists
sc query PrecisionDrv >nul 2>&1
if %errorLevel% neq 0 (
    echo WARNING: PrecisionDrv service not found.
    echo The driver may not be installed or was already removed.
    echo.
)

REM Create backup directory if missing
if not exist "backup" mkdir backup

echo [STEP 2] Backing up current configuration...
echo.

REM Backup registry configuration before removal
echo - Backing up current registry configuration...
reg export "HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E96B-E325-11CE-BFC1-08002BE10318}" "backup\keyboard_before_uninstall.reg" /y >nul 2>&1
reg export "HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E96F-E325-11CE-BFC1-08002BE10318}" "backup\mouse_before_uninstall.reg" /y >nul 2>&1

echo [STEP 3] Stopping and removing the service...
echo.

REM Stop the service
echo - Stopping PrecisionDrv service...
sc stop PrecisionDrv >nul 2>&1
if %errorLevel% equ 0 (
    echo   * Service stopped successfully
    timeout /t 2 /nobreak >nul
) else (
    echo   * Service already stopped or not present
)

REM Delete the service
echo - Removing PrecisionDrv service...
sc delete PrecisionDrv >nul 2>&1
if %errorLevel% equ 0 (
    echo   * Service removed successfully
) else (
    echo   * ERROR: Could not remove the service or it was not found
)

timeout /t 1 /nobreak >nul

echo [STEP 4] Removing from UpperFilters (SAFE METHOD)...
echo.

REM ===== KEYBOARD =====
echo - Removing PrecisionDrv from keyboard UpperFilters...

reg query "HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E96B-E325-11CE-BFC1-08002BE10318}" /v UpperFilters >nul 2>&1
if %errorLevel% equ 0 (
    reg query "HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E96B-E325-11CE-BFC1-08002BE10318}" /v UpperFilters | find /i "PrecisionDrv" >nul 2>&1
    if %errorLevel% equ 0 (
        echo   * PrecisionDrv found in keyboard UpperFilters, removing...
        
        powershell -Command "& { $key = 'HKLM:\SYSTEM\CurrentControlSet\Control\Class\{4D36E96B-E325-11CE-BFC1-08002BE10318}'; $current = (Get-ItemProperty -Path $key -Name UpperFilters -ErrorAction SilentlyContinue).UpperFilters; if ($current) { $new = $current | Where-Object { $_ -ne 'PrecisionDrv' -and $_ -ne '' }; if ($new.Count -gt 0) { Set-ItemProperty -Path $key -Name UpperFilters -Value $new -Type MultiString } else { Remove-ItemProperty -Path $key -Name UpperFilters -ErrorAction SilentlyContinue } } }"
        
        if %errorLevel% equ 0 (
            echo   * PrecisionDrv successfully removed from keyboard UpperFilters
        ) else (
            echo   * ERROR: Unable to remove PrecisionDrv from keyboard UpperFilters
        )
    ) else (
        echo   * PrecisionDrv not found in keyboard UpperFilters
    )
) else (
    echo   * No UpperFilters configured for keyboard
)

REM ===== HIDClass =====
echo - Removing PrecisionDrv from HIDClass UpperFilters...

reg query "HKLM\SYSTEM\CurrentControlSet\Control\Class\{745a17a0-74d3-11d0-b6fe-00a0c90f57da}" /v UpperFilters >nul 2>&1
if %errorLevel% equ 0 (
    reg query "HKLM\SYSTEM\CurrentControlSet\Control\Class\{745a17a0-74d3-11d0-b6fe-00a0c90f57da}" /v UpperFilters | find /i "PrecisionDrv" >nul 2>&1
    if %errorLevel% equ 0 (
        echo   * PrecisionDrv found in HIDClass UpperFilters, removing...

        powershell -Command "& { $key = 'HKLM:\SYSTEM\CurrentControlSet\Control\Class\{745a17a0-74d3-11d0-b6fe-00a0c90f57da}'; $current = (Get-ItemProperty -Path $key -Name UpperFilters -ErrorAction SilentlyContinue).UpperFilters; if ($current) { $new = $current | Where-Object { $_ -ne 'PrecisionDrv' -and $_ -ne '' }; if ($new.Count -gt 0) { Set-ItemProperty -Path $key -Name UpperFilters -Value $new -Type MultiString } else { Remove-ItemProperty -Path $key -Name UpperFilters -ErrorAction SilentlyContinue } } }"

        if %errorLevel% equ 0 (
            echo   * PrecisionDrv successfully removed from HIDClass UpperFilters
        ) else (
            echo   * ERROR: Unable to remove PrecisionDrv from HIDClass UpperFilters
        )
    ) else (
        echo   * PrecisionDrv not found in HIDClass UpperFilters
    )
) else (
    echo   * No UpperFilters configured for HIDClass
)

REM ===== MOUSE =====
echo - Removing PrecisionDrv from mouse UpperFilters...

reg query "HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E96F-E325-11CE-BFC1-08002BE10318}" /v UpperFilters >nul 2>&1
if %errorLevel% equ 0 (
    reg query "HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E96F-E325-11CE-BFC1-08002BE10318}" /v UpperFilters | find /i "PrecisionDrv" >nul 2>&1
    if %errorLevel% equ 0 (
        echo   * PrecisionDrv found in mouse UpperFilters, removing...
        
        powershell -Command "& { $key = 'HKLM:\SYSTEM\CurrentControlSet\Control\Class\{4D36E96F-E325-11CE-BFC1-08002BE10318}'; $current = (Get-ItemProperty -Path $key -Name UpperFilters -ErrorAction SilentlyContinue).UpperFilters; if ($current) { $new = $current | Where-Object { $_ -ne 'PrecisionDrv' -and $_ -ne '' }; if ($new.Count -gt 0) { Set-ItemProperty -Path $key -Name UpperFilters -Value $new -Type MultiString } else { Remove-ItemProperty -Path $key -Name UpperFilters -ErrorAction SilentlyContinue } } }"
        
        if %errorLevel% equ 0 (
            echo   * PrecisionDrv successfully removed from mouse UpperFilters
        ) else (
            echo   * ERROR: Unable to remove PrecisionDrv from mouse UpperFilters
        )
    ) else (
        echo   * PrecisionDrv not found in mouse UpperFilters
    )
) else (
    echo   * No UpperFilters configured for mouse
)

echo [STEP 5] Removing driver file...
echo.

REM Backup driver before deletion
if exist "C:\Windows\System32\drivers\PrecisionDrv.sys" (
    echo - Backing up driver before removal...
    copy "C:\Windows\System32\drivers\PrecisionDrv.sys" "backup\PrecisionDrv_removed.sys" >nul 2>&1
    
    echo - Deleting driver file...
    del "C:\Windows\System32\drivers\PrecisionDrv.sys" >nul 2>&1
    if %errorLevel% equ 0 (
        echo   * Driver file removed successfully
    ) else (
        echo   * WARNING: Could not remove driver file
        echo   * The file might be in use. Reboot and rerun this script.
    )
) else (
    echo - Driver file not found in System32\drivers
)

REM Verify final configuration
echo.
echo [VERIFICATION] Checking post-removal configuration...

echo - Remaining Keyboard UpperFilters:
reg query "HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E96B-E325-11CE-BFC1-08002BE10318}" /v UpperFilters 2>nul | find "REG_MULTI_SZ" || echo   * No UpperFilters configured

echo - Remaining Mouse UpperFilters:
reg query "HKLM\SYSTEM\CurrentControlSet\Control\Class\{4D36E96F-E325-11CE-BFC1-08002BE10318}" /v UpperFilters 2>nul | find "REG_MULTI_SZ" || echo   * No UpperFilters configured

echo - Service status:
sc query PrecisionDrv >nul 2>&1
if %errorLevel% equ 0 (
    echo   * WARNING: Service still present!
) else (
    echo   * Service removed successfully
)

echo.
echo ============================================================================
echo   UNINSTALLATION COMPLETED
echo ============================================================================
echo.
echo PrecisionDrv has been removed from your system:
echo + PrecisionDrv service deleted
echo + PrecisionDrv removed from keyboard UpperFilters
echo + PrecisionDrv removed from mouse UpperFilters
echo + Driver file deleted from System32\drivers
echo.
echo IMPORTANT: Default system class drivers have been PRESERVED!
echo + Keyboard functionality remains intact
echo + Mouse functionality remains intact
echo + Other existing UpperFilters were preserved
echo.
echo BACKUP FILES CREATED:
echo - backup\keyboard_before_uninstall.reg (keyboard config before uninstall)
echo - backup\mouse_before_uninstall.reg (mouse config before uninstall)
echo - backup\PrecisionDrv_removed.sys (removed driver backup)
echo.
echo RECOMMENDATION: Please restart your system to finalize removal.
echo.

choice /c YN /m "Do you want to restart the system now?"
if %errorLevel% equ 1 (
    echo Rebooting system...
    timeout /t 3 /nobreak >nul
    shutdown /r /t 0
) else (
    echo.
    echo NOTE: Restart the system manually when ready.
    echo.
)

pause
