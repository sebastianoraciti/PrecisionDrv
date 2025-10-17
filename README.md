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

## 💡 Potential Use Cases

The PrecisionDrv framework can serve as a foundation for legitimate and socially useful applications in multiple technical and research domains, including:

### 🧠 1. Accessibility & Assistive Input Systems
Used to create accessibility tools for people with physical disabilities — e.g. custom key mapping, prevention of accidental inputs, or “sticky keys” logic for improved usability.

### 🧪 2. Keyboard & Mouse Diagnostics
Enable low-level input event logging to study latency, hardware ghosting, and key press timing, allowing for accurate performance measurements and debugging of hardware or driver-level input issues.

### 🖥️ 3. Human–Machine Interaction Research
Provide a framework for controlled input capture in usability or cognitive studies, helping researchers analyze human response times, ergonomics, and real-world interaction behaviors.

### ⚙️ 4. Automation & Training Systems
Integrate within industrial or educational training simulators where multiple input devices must be coordinated or filtered safely (e.g., SCADA systems, aviation training panels, or industrial control units).

### 🔐 5. Security & Behavioral Analytics
Used to study typing behavior and develop non-invasive keystroke dynamics recognition methods for authentication and behavioral security systems (without storing sensitive data).


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
