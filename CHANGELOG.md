# Changelog — PrecisionDrv

All notable changes to this project will be documented in this file.  
This project adheres to the principles of **transparency**, **safety**, and **open development**.

---

## [v1.0.0] — 2025-10-17
### Initial public release

#### Added
- First public open-source release of **PrecisionDrv**, a kernel-level filter driver for keyboard and mouse input interception.
- Included all essential documentation files:
  - `README.md` — full project overview, usage, and purpose.
  - `LICENSE` — MIT License.
  - `USAGE.md` — ethical and legal usage policy.
  - `RECOVERY.md` — recovery instructions in case of input loss.
  - `CHANGELOG.md` — version history file.

#### Technical features
- Kernel-level interception of keyboard and mouse input events.
- Support for controlled blocking and filtering of selected inputs.
- Debug-safe architecture with diagnostic counters and callbacks.
- Compatible with both PS/2 and HID-class devices.
- Compiled for Windows 10 / Windows 11 x64 (WDK-based).

#### Safety and transparency improvements
- Clear usage limitations and warnings against misuse.
- Step-by-step Safe Mode recovery guide.
- Code structure adapted for open-source verification (Certum SimplySign).

---

## Planned for [v1.1.0]
### Upcoming improvements
- Digital signature support (Certum SimplySign Open Source certificate).
- Enhanced INF file for Plug & Play installation.
- Additional configuration options in user-space application.
- Input filtering whitelist/blacklist via configuration file.
- Extended diagnostic logging via IOCTL commands.

---

### Maintainer
**Sebastiano Raciti**  
---

> © 2025 — PrecisionDrv Project. Released under the MIT License.
