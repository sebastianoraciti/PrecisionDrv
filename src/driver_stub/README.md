# PrecisionDrv Stub — README

This directory contains a **stub implementation** of the PrecisionDrv kernel driver (`precisiondrv_stub.c`) intended for public distribution alongside the project's documentation and user-mode test utilities.

**IMPORTANT:** This stub intentionally omits the real kernel interception logic and sensitive implementation details. It is provided only to demonstrate the driver’s public interface (device names, IOCTLs, basic behavior) and to allow reviewers and testers to compile a non-invasive driver for verification and development.

## Purpose
- Provide a minimal, safe kernel driver that exposes the same user-mode **IOCTL** interface used by the real driver.
- Allow developers, auditors, and Certum reviewers to validate the presence of a kernel component without revealing proprietary code.
- Serve as a build example and a compatibility target for the user-mode test application.

## What is included
- `precisiondrv_stub.c` — minimal kernel-mode driver source (stub).
  - Creates a control device and symbolic link:
    - Device: `\Device\PrecisionDrvStub`
    - Symbolic link: `\DosDevices\PrecisionDrvStub`
  - Implements basic IRP handlers: Create, Close, DeviceControl.
  - Exposes the same IOCTL definitions as the production driver (see below).
  - Returns safe, non-functional responses for IOCTLs (no interception performed).

## IOCTL interface (stub)
The stub defines the following IOCTL codes (for compatibility with the user-mode test app):

- `IOCTL_PRECISION_ENABLE_INTERCEPT` — Accepts request, does not enable interception (stub).
- `IOCTL_PRECISION_DISABLE_INTERCEPT` — Accepts request, does not disable interception (stub).
- `IOCTL_PRECISION_GET_INPUT_DATA` — Returns zero bytes (no real input data).
- `IOCTL_PRECISION_SET_BLOCK_POLICY` — Accepts policy structure but ignores contents.
- `IOCTL_PRECISION_GET_STATISTICS` — Returns a `DRIVER_STATISTICS` structure with zeroed counters.
- `IOCTL_PRECISION_GET_DIAGNOSTICS` — Returns `STATUS_NOT_IMPLEMENTED` in stub.

`DRIVER_STATISTICS` (stub):
```c
typedef struct _DRIVER_STATISTICS {
    LONG TotalInputs;
    LONG BlockedInputs;
    BOOLEAN InterceptEnabled;
} DRIVER_STATISTICS;
```

## Build notes
- The stub is a minimal WDK-compatible driver source file. It is **not** a fully functional filter driver.
- To compile:
  1. Create a WDK driver project in Visual Studio (KMDF/WDK).
  2. Add `precisiondrv_stub.c` to the project.
  3. Set the target architecture to **x64** and build.
- The produced `.sys` can be used for testing install/uninstall scripts and the user-mode test application without risk of input interception.

## How to use
- Use the stub `.sys` for:
  - Verifying installation scripts (`install.bat`, `uninstall.bat`).
  - Testing the user-mode test application and IOCTL interactions.
  - Demonstrating to reviewers (Certum) that a kernel component exists and exposes documented interfaces.
- **Do not** rely on the stub for real interception or production use.

## Security & licensing
- The stub is intentionally limited: it does not perform any input interception or logging.
- The real driver source remains proprietary and is not included in this repository.
- License for the stub follows the project license (see `LICENSE` in root). The stub is provided "AS IS" without warranty.
- Redistribution and reverse engineering of the compiled production driver may be restricted by project policy (see `USAGE.md`).

## Notes for reviewers
- The stub mirrors the public IOCTL contract expected by the user-mode application. If you need to verify deeper behavior, contact the maintainer for privileged access under NDA or for a code-review appointment.
- Maintainer: **Sebastiano Raciti** — GitHub: @SebastianoRaciti

