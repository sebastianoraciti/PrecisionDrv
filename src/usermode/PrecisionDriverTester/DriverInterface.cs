using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PrecisionDriverTester
{
    // Enum per i tipi di input
    public enum InputType : uint
    {
        InputKeyboard = 1,
        InputMouseButton = 2,
        InputMouseMove = 3,
        InputMouseWheel = 4
    }

    // Enum per le azioni
    public enum InputAction : uint
    {
        ActionAllow = 0,
        ActionBlock = 1
    }

    // *** STRUTTURE CORRETTE CON PACK=1 PER MATCHARE IL DRIVER C ***

    // Struttura per i dati keyboard (8 bytes totali)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KeyboardData
    {
        public ushort ScanCode;   // 2 bytes
        public ushort Flags;      // 2 bytes
        public byte KeyDown;      // 1 byte
        public byte Reserved1;    // 1 byte (padding)
        public byte Reserved2;    // 1 byte (padding)
        public byte Reserved3;    // 1 byte (padding)
        // TOTALE: 8 bytes
    }

    // Struttura per i dati mouse (16 bytes totali)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MouseData
    {
        public int X;                // 4 bytes (LONG in C)
        public int Y;                // 4 bytes (LONG in C)
        public ushort ButtonFlags;   // 2 bytes
        public ushort ButtonData;    // 2 bytes
        public byte ButtonPressed;   // 1 byte
        public byte Reserved1;       // 1 byte (padding)
        public byte Reserved2;       // 1 byte (padding)
        public byte Reserved3;       // 1 byte (padding)
        // TOTALE: 16 bytes
    }

    // Union per i dati input - la più grande è MouseData (16 bytes)
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct InputData
    {
        [FieldOffset(0)]
        public KeyboardData Keyboard;  // 8 bytes

        [FieldOffset(0)]
        public MouseData Mouse;        // 16 bytes (la più grande)
    }

    // *** STRUTTURA CRITICA: DEVE ESSERE ESATTAMENTE 26 BYTES ***
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InterceptedInput
    {
        // Header (13 bytes)
        public uint Type;              // 4 bytes (INPUT_TYPE in C)
        public long Timestamp;         // 8 bytes (LARGE_INTEGER in C)
        public byte Blocked;           // 1 byte (BOOLEAN in C)

        // Union Data - INLINE senza nested struct (13 bytes - la più grande)
        // Questi campi sono condivisi tra Keyboard e Mouse
        public int Data1;              // 4 bytes - Keyboard: ScanCode+Flags | Mouse: X
        public int Data2;              // 4 bytes - Keyboard: KeyDown+padding | Mouse: Y
        public ushort Data3;           // 2 bytes - Mouse: ButtonFlags
        public ushort Data4;           // 2 bytes - Mouse: ButtonData
        public byte Data5;             // 1 byte - Mouse: ButtonPressed

        // TOTALE: 4 + 8 + 1 + 4 + 4 + 2 + 2 + 1 = 26 bytes ✓

        // ========================================================================
        // HELPER PROPERTIES PER ACCESSO TIPIZZATO
        // ========================================================================

        // Per Keyboard
        public ushort KeyboardScanCode
        {
            get { return (ushort)(Data1 & 0xFFFF); }
        }

        public ushort KeyboardFlags
        {
            get { return (ushort)((Data1 >> 16) & 0xFFFF); }
        }

        public bool KeyboardKeyDown
        {
            get { return (Data2 & 0xFF) != 0; }
        }

        // Per Mouse
        public int MouseX
        {
            get { return Data1; }
        }

        public int MouseY
        {
            get { return Data2; }
        }

        public ushort MouseButtonFlags
        {
            get { return Data3; }
        }

        public ushort MouseButtonData
        {
            get { return Data4; }
        }

        public bool MouseButtonPressed
        {
            get { return Data5 != 0; }
        }

        // Property helper per il tipo
        public InputType InputType
        {
            get { return (InputType)Type; }
        }

        public bool IsBlocked
        {
            get { return Blocked != 0; }
        }
    }


    // *** ALTERNATIVE: Prova questa versione FLAT senza nested structures ***
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InterceptedInputFlat
    {
        public uint Type;              // 4 bytes
        public long Timestamp;         // 8 bytes
        public byte Blocked;           // 1 byte

        // Union inlined - PRIMO MEMBRO (Keyboard)
        public ushort ScanCode;        // 2 bytes
        public ushort KeyboardFlags;   // 2 bytes
        public byte KeyDown;           // 1 byte
        public byte Kbd_Reserved1;     // 1 byte
        public byte Kbd_Reserved2;     // 1 byte
        public byte Kbd_Reserved3;     // 1 byte

        // Padding per raggiungere la dimensione della union (16 bytes totali)
        public long UnionPadding;      // 8 bytes extra per completare i 16

        // TOTALE: 4 + 8 + 1 + 16 = 29 bytes

        // Helper per accedere ai dati mouse (reinterpretando i byte)
        public int MouseX
        {
            get { return (int)((uint)ScanCode | ((uint)KeyboardFlags << 16)); }
        }
        public int MouseY
        {
            get
            {
                return (int)((uint)KeyDown | ((uint)Kbd_Reserved1 << 8) |
                              ((uint)Kbd_Reserved2 << 16) | ((uint)Kbd_Reserved3 << 24));
            }
        }
    }

    // *** VERSIONE SEMPLIFICATA: Proviamo con byte array raw ***
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct InterceptedInputRaw
    {
        public uint Type;              // 4 bytes
        public long Timestamp;         // 8 bytes
        public byte Blocked;           // 1 byte

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] DataRaw;         // 16 bytes raw data

        // TOTALE: 4 + 8 + 1 + 16 = 29 bytes

        // Helper per decodificare keyboard
        public KeyboardData GetKeyboardData()
        {
            var kbd = new KeyboardData();
            kbd.ScanCode = BitConverter.ToUInt16(DataRaw, 0);
            kbd.Flags = BitConverter.ToUInt16(DataRaw, 2);
            kbd.KeyDown = DataRaw[4];
            return kbd;
        }

        // Helper per decodificare mouse
        public MouseData GetMouseData()
        {
            var mouse = new MouseData();
            mouse.X = BitConverter.ToInt32(DataRaw, 0);
            mouse.Y = BitConverter.ToInt32(DataRaw, 4);
            mouse.ButtonFlags = BitConverter.ToUInt16(DataRaw, 8);
            mouse.ButtonData = BitConverter.ToUInt16(DataRaw, 10);
            mouse.ButtonPressed = DataRaw[12];
            return mouse;
        }
    }

    // *** INIZIALIZZA L'ARRAY NEL COSTRUTTORE ***
    public static class InterceptedInputHelper
    {
        public static InterceptedInputRaw CreateEmpty()
        {
            return new InterceptedInputRaw { DataRaw = new byte[16] };
        }
    }

    // Struttura per le policy di blocco
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BlockPolicy
    {
        public byte BlockKeyboard;
        public byte BlockMouse;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] BlockSpecificKeys;
        public byte BlockMouseButtons;
        public byte BlockMouseMovement;

        public BlockPolicy(bool initialize)
        {
            BlockKeyboard = 0;
            BlockMouse = 0;
            BlockMouseButtons = 0;
            BlockMouseMovement = 0;
            BlockSpecificKeys = new byte[256];
        }
    }

    // Struttura per le statistiche
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DriverStatistics
    {
        public int TotalInputs;
        public int BlockedInputs;
        public int KeyboardInputs;
        public int MouseInputs;
        public byte InterceptEnabled;
    }

    // Classe per l'interfaccia con il driver
    public static class DriverInterface
    {
        // Costanti IOCTL (CORRETTE)
        private const uint FILE_DEVICE_UNKNOWN = 0x00000022;
        private const uint METHOD_BUFFERED = 0;
        private const uint FILE_ANY_ACCESS = 0;

        private static uint CTL_CODE(uint deviceType, uint function, uint method, uint access)
        {
            return ((deviceType) << 16) | ((access) << 14) | ((function) << 2) | (method);
        }

        public static readonly uint IOCTL_PRECISION_ENABLE_INTERCEPT = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x800, METHOD_BUFFERED, FILE_ANY_ACCESS); // 0x222000
        public static readonly uint IOCTL_PRECISION_DISABLE_INTERCEPT = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x801, METHOD_BUFFERED, FILE_ANY_ACCESS); // 0x222004
        public static readonly uint IOCTL_PRECISION_GET_INPUT_DATA = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x802, METHOD_BUFFERED, FILE_ANY_ACCESS); // 0x222008
        public static readonly uint IOCTL_PRECISION_SET_BLOCK_POLICY = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x803, METHOD_BUFFERED, FILE_ANY_ACCESS); // 0x22200C
        public static readonly uint IOCTL_PRECISION_GET_STATISTICS = CTL_CODE(FILE_DEVICE_UNKNOWN, 0x804, METHOD_BUFFERED, FILE_ANY_ACCESS); // 0x222010

        // Path del device
        public const string DEVICE_PATH = "\\\\.\\PrecisionDrv";

        // Imports Win32 API
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        // Costanti Win32
        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    }

    // Classe wrapper per semplificare l'uso del driver
    public class PrecisionDriver : IDisposable
    {
        private SafeFileHandle _deviceHandle;
        private bool _disposed = false;

        public bool IsConnected => _deviceHandle != null && !_deviceHandle.IsInvalid;

        public bool Connect()
        {
            try
            {
                _deviceHandle = DriverInterface.CreateFile(
                    DriverInterface.DEVICE_PATH,
                    DriverInterface.GENERIC_READ | DriverInterface.GENERIC_WRITE,
                    DriverInterface.FILE_SHARE_READ | DriverInterface.FILE_SHARE_WRITE,
                    IntPtr.Zero,
                    DriverInterface.OPEN_EXISTING,
                    DriverInterface.FILE_ATTRIBUTE_NORMAL,
                    IntPtr.Zero);

                return IsConnected;
            }
            catch
            {
                return false;
            }
        }

        public void Disconnect()
        {
            _deviceHandle?.Dispose();
            _deviceHandle = null;
        }

        public bool EnableIntercept()
        {
            if (!IsConnected) return false;

            return DriverInterface.DeviceIoControl(
                _deviceHandle,
                DriverInterface.IOCTL_PRECISION_ENABLE_INTERCEPT,
                IntPtr.Zero, 0,
                IntPtr.Zero, 0,
                out _,
                IntPtr.Zero);
        }

        public bool DisableIntercept()
        {
            if (!IsConnected) return false;

            return DriverInterface.DeviceIoControl(
                _deviceHandle,
                DriverInterface.IOCTL_PRECISION_DISABLE_INTERCEPT,
                IntPtr.Zero, 0,
                IntPtr.Zero, 0,
                out _,
                IntPtr.Zero);
        }

        public bool SetBlockPolicy(BlockPolicy policy)
        {
            if (!IsConnected) return false;

            IntPtr policyPtr = Marshal.AllocHGlobal(Marshal.SizeOf<BlockPolicy>());
            try
            {
                Marshal.StructureToPtr(policy, policyPtr, false);

                return DriverInterface.DeviceIoControl(
                    _deviceHandle,
                    DriverInterface.IOCTL_PRECISION_SET_BLOCK_POLICY,
                    policyPtr, (uint)Marshal.SizeOf<BlockPolicy>(),
                    IntPtr.Zero, 0,
                    out _,
                    IntPtr.Zero);
            }
            finally
            {
                Marshal.FreeHGlobal(policyPtr);
            }
        }

        public DriverStatistics? GetStatistics()
        {
            if (!IsConnected) return null;

            IntPtr statsPtr = Marshal.AllocHGlobal(Marshal.SizeOf<DriverStatistics>());
            try
            {
                bool success = DriverInterface.DeviceIoControl(
                    _deviceHandle,
                    DriverInterface.IOCTL_PRECISION_GET_STATISTICS,
                    IntPtr.Zero, 0,
                    statsPtr, (uint)Marshal.SizeOf<DriverStatistics>(),
                    out _,
                    IntPtr.Zero);

                if (success)
                {
                    return Marshal.PtrToStructure<DriverStatistics>(statsPtr);
                }

                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(statsPtr);
            }
        }

        public InterceptedInput[] GetInputData(int maxInputs = 100)
        {
            Console.WriteLine($"[GetInputData] ═══════════════════════════════════");
            Console.WriteLine($"[GetInputData] Chiamato con maxInputs={maxInputs}");

            if (!IsConnected)
            {
                Console.WriteLine($"[GetInputData] ✗ NON CONNESSO!");
                return new InterceptedInput[0];
            }

            int structSize = Marshal.SizeOf<InterceptedInput>();
            int bufferSize = structSize * maxInputs;

            Console.WriteLine($"[GetInputData] Sizeof(InterceptedInput)={structSize} bytes");
            Console.WriteLine($"[GetInputData] BufferSize={bufferSize} bytes ({maxInputs} eventi max)");

            IntPtr inputPtr = Marshal.AllocHGlobal(bufferSize);
            Console.WriteLine($"[GetInputData] Buffer allocato: 0x{inputPtr.ToInt64():X}");

            try
            {
                Console.WriteLine($"[GetInputData] Chiamando DeviceIoControl...");
                Console.WriteLine($"[GetInputData] IOCTL=0x{DriverInterface.IOCTL_PRECISION_GET_INPUT_DATA:X8}");

                bool success = DriverInterface.DeviceIoControl(
                    _deviceHandle,
                    DriverInterface.IOCTL_PRECISION_GET_INPUT_DATA,
                    IntPtr.Zero, 0,
                    inputPtr, (uint)bufferSize,
                    out uint bytesReturned,
                    IntPtr.Zero);

                Console.WriteLine($"[GetInputData] DeviceIoControl result={success}");
                Console.WriteLine($"[GetInputData] BytesReturned={bytesReturned}");

                if (!success)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    Console.WriteLine($"[GetInputData] ✗ ERRORE Win32: {errorCode} (0x{errorCode:X})");
                    return new InterceptedInput[0];
                }

                if (bytesReturned > 0)
                {
                    int inputCount = (int)(bytesReturned / structSize);
                    Console.WriteLine($"[GetInputData] ✓ Calcolati {inputCount} eventi ({bytesReturned}/{structSize})");

                    if (bytesReturned % structSize != 0)
                    {
                        Console.WriteLine($"[GetInputData] ⚠ WARNING: bytesReturned non è multiplo esatto!");
                    }

                    InterceptedInput[] inputs = new InterceptedInput[inputCount];

                    for (int i = 0; i < inputCount; i++)
                    {
                        IntPtr currentPtr = IntPtr.Add(inputPtr, i * structSize);
                        inputs[i] = Marshal.PtrToStructure<InterceptedInput>(currentPtr);

                        // Debug primi 3 eventi
                        if (i < 3)
                        {
                            Console.WriteLine($"[GetInputData]   Evento[{i}]: Type={inputs[i].Type}, Blocked={inputs[i].Blocked}");
                        }
                    }

                    Console.WriteLine($"[GetInputData] ✓ Restituiti {inputCount} eventi");
                    return inputs;
                }
                else
                {
                    Console.WriteLine($"[GetInputData] ○ Nessun evento disponibile (bytesReturned=0)");
                    return new InterceptedInput[0];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetInputData] ✗✗✗ ECCEZIONE: {ex.Message}");
                Console.WriteLine($"[GetInputData] StackTrace: {ex.StackTrace}");
                return new InterceptedInput[0];
            }
            finally
            {
                Marshal.FreeHGlobal(inputPtr);
                Console.WriteLine($"[GetInputData] Buffer liberato");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _deviceHandle?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}