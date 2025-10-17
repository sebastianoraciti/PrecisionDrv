# Recovery Guide — PrecisionDrv

If your **keyboard or mouse stops responding** after installing or testing the driver, follow this recovery guide carefully.  
These steps will help you **safely remove PrecisionDrv** and restore normal system input.

---

## 🧰 Step 1 — Boot into Safe Mode

### Option A — From Windows
1. Hold **Shift** and click **Restart** from the Start Menu.  
2. Choose **Troubleshoot → Advanced options → Startup Settings → Restart**.  
3. When prompted, press:
   - **4** for **Safe Mode**, or  
   - **5** for **Safe Mode with Networking**.

### Option B — From Command Prompt
If you still have access to a console or remote shell, run:
```
bcdedit /set {default} safeboot minimal
shutdown /r /t 0
```
After the reboot, Windows will automatically start in Safe Mode.

---

## 🧹 Step 2 — Uninstall the Driver

Once inside Safe Mode, you can remove PrecisionDrv using **Device Manager** or **Command Prompt**.

### A) Using Device Manager
1. Press **Win + X → Device Manager**.  
2. Expand either:
   - **Keyboards**, or  
   - **Mice and other pointing devices**.  
3. Right-click any device entry related to **PrecisionDrv** and select **Uninstall device**.  
4. Check **Delete the driver software for this device** if available.  
5. Confirm, then **restart the PC**.

---

### B) Using Command Prompt (Recommended)
1. Open **Command Prompt (Admin)**.  
2. List all installed drivers:
```
pnputil /enum-drivers
```
3. Find the driver related to PrecisionDrv — it will look similar to:
```
Published Name : oem42.inf
Original Name  : driver.inf
Provider Name  : Sebastiano Raciti
Class Name     : Keyboard
```
4. Uninstall it:
```
pnputil /delete-driver oem42.inf /uninstall /force
```
5. Remove leftover files (if installed manually):
```
del /f "C:\Program Files\PrecisionDrv\PrecisionDrv.sys"
del /f "C:\Program Files\PrecisionDrv\driver.inf"
rmdir "C:\Program Files\PrecisionDrv" /s /q
```

---

## ⚙️ Step 3 — Clean the Service (if registered manually)
If you previously used `sc create` to register the driver service, remove it manually:
```
sc stop PrecisionDrv
sc delete PrecisionDrv
```

---

## 🔁 Step 4 — Exit Safe Mode and Reboot
If you booted into Safe Mode using `bcdedit`, disable it before restarting:
```
bcdedit /deletevalue {default} safeboot
```
Then reboot normally:
```
shutdown /r /t 0
```

Your keyboard and mouse should now function correctly again.

---

## 🩺 Step 5 — If Input Devices Still Do Not Respond
If the issue persists, try one or more of the following:
- Connect devices to **different USB ports**.  
- Access the machine through **Remote Desktop (RDP)** if available.  
- Perform a **System Restore** from a previous restore point.  
- Boot from a **Windows installation media**, select **Repair your computer → Command Prompt**, and repeat the uninstall commands above.

---

## 💡 Additional Recommendations
- Always test kernel-mode drivers in a **virtual machine** before installing on real hardware.  
- Keep a **system restore point** or **registry backup** before experimenting with input filter drivers.  
- Avoid editing registry filters manually unless absolutely necessary.

---

**PrecisionDrv Recovery Guide**  
Last updated: **October 2025**  
Maintainer: **Sebastiano Raciti**
