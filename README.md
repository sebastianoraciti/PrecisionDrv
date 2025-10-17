# PrecisionDrv

**PrecisionDrv** is a custom Windows kernel driver project designed for safe input interception, blocking, and diagnostic experimentation.  
This repository contains the **open-source stub**, user-mode utilities, and installation scripts used for testing and certification purposes.

> ⚠️ The full production driver source code is **proprietary and not publicly distributed**.  
> The files provided here include only the **open and verifiable components** necessary to demonstrate functionality, transparency, and structure.

---

## 🧩 Project Overview

**PrecisionDrv** was created to explore advanced input management at the kernel level, including:

- Interception of mouse and keyboard events (for diagnostics and research)  
- Customizable blocking rules and safe filtering logic  
- Integration with user-mode control software via IOCTL interface  
- Logging and diagnostic capabilities for accessibility and automation contexts  

The repository provides a **safe, non-functional stub driver** (`precisiondrv_stub.c`) which exposes the same IOCTL interface as the production version but contains no interception logic.

---

## 📁 Repository Structure

```
PrecisionDrv/
├── src/
│   ├── driver_stub/          # Public stub driver (safe, minimal implementation)
│   │   ├── precisiondrv_stub.c
│   │   └── README_STUB.md
│   └── usermode/             # Example user-mode app (for IOCTL testing)
│
├── scripts/                  # Batch scripts for install, uninstall, and signature verification
│   ├── install_driver.bat
│   ├── uninstall_driver.bat
│   ├── verify_signature.bat
│   └── README_SCRIPTS.md
│
├── releases/                 # Ready-to-test binaries and documentation
│   ├── PrecisionDrv.sys
│   ├── driver.inf
│   ├── install.bat
│   ├── uninstall.bat
│   ├── SHA256SUMS.txt
│   └── README.txt
│
├── README.md                 # (This file)
├── USAGE.md                  # General usage notes and driver test guide
├── RECOVERY.md               # Safe recovery procedure (if system input becomes blocked)
├── CHANGELOG.md              # Version and development log
└── LICENSE                   # (Optional) to be added later
```

---

## ⚙️ Installation

> 🧠 **Note:** The provided binaries are the safe *stub* version for open distribution and certification testing.  
> They do not perform any real input interception or filtering.

1. Download the latest release from the [Releases](../../releases) section.  
2. Run `scripts/install_driver.bat` as **Administrator**.  
3. Reboot your system to complete the installation.  
4. To uninstall, run `scripts/uninstall_driver.bat`.

---

## 🔐 Verification

After signing with your certificate (e.g. Certum Open Source Code Signing), verify the signature with:

```bat
signtool verify /pa /v PrecisionDrv.sys
```

You can also check integrity using the provided SHA file:

```bash
sha256sum -c SHA256SUMS.txt
```

---

## 🧠 Development Notes

- The stub driver is built for **WDK x64** and compatible with Windows 10/11.  
- It provides identical **IOCTL codes** and device naming as the production driver.  
- The repository is organized to meet **Certum Open Source verification** standards.  

---

## ⚠️ Disclaimer

The full kernel driver source code is proprietary and privately maintained by **Seby Raciti**.  
Only safe, non-functional public components are released for transparency, certification, and educational research.

Unauthorized redistribution, reverse engineering, or modification of compiled driver binaries is strictly prohibited.

---

© 2025 **Sebastiano Raciti** — PrecisionDrv Project  
All rights reserved.
