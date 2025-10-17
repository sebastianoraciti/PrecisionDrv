# Scripts Folder

This folder contains the official batch scripts for installing, uninstalling, and verifying the **PrecisionDrv** kernel driver.

Each script has been carefully designed to perform its task safely and transparently, without altering other system drivers or configurations.

---

## 📜 Script Overview

### 🧩 `install_driver.bat`
Installs the **PrecisionDrv** driver on the system.

- Copies `PrecisionDrv.sys` to `C:\Windows\System32\drivers\`
- Registers the kernel service using `sc create`
- Adds the driver as an **Upper Filter** for both **Keyboard** and **Mouse** classes
- Verifies installation and displays detailed status
- Requests a reboot to activate changes

> ⚠️ **Run this script as Administrator.**  
> The driver will not be installed if you lack administrative privileges.

---

### 🧹 `uninstall_driver.bat`
Safely removes the **PrecisionDrv** driver from the system.

- Stops and deletes the kernel service
- Removes `PrecisionDrv` from the **UpperFilters** entries (Keyboard and Mouse)
- Creates backups of current registry configurations in the `backup/` folder
- Preserves other existing drivers and filters
- Optionally reboots the system at the end of the process

> 🧠 **Note:**  
> The uninstall process never disables or removes other drivers.  
> Only `PrecisionDrv` is affected.

---

### 🔏 `verify_signature.bat`
Verifies the **digital signature** of the driver (`PrecisionDrv.sys`) after signing.

- Uses Microsoft’s `signtool` utility
- Displays the certificate chain and timestamp information
- Confirms if the driver is properly signed and trusted by the system

Example usage:

```bat
signtool verify /pa /v PrecisionDrv.sys
```

> 💡 This is useful after signing the driver with your **Certum Open Source Code Signing Certificate**.

---

## ⚙️ Usage Recommendations

- Always execute these scripts from an **elevated command prompt** (Run as Administrator).
- If Secure Boot is enabled, ensure the driver is **properly signed** before running the install script.
- For development or testing on unsigned builds, you may temporarily enable **Test Signing Mode** using:
  ```bat
  bcdedit /set testsigning on
  ```

---

## 🧩 Folder Summary

| Script | Purpose |
|--------|----------|
| `install_driver.bat` | Installs and registers the PrecisionDrv driver |
| `uninstall_driver.bat` | Safely removes the PrecisionDrv driver |
| `verify_signature.bat` | Checks digital signature validity |

---

© 2025 **Sebastiano Raciti** — PrecisionDrv Project  
All rights reserved.
