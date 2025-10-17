/*
 * PrecisionDrv - Stub version for public repository
 * This file is a safe, minimal stub derived from the original driver.
 * It exposes interfaces and basic structure but intentionally omits
 * the internal logic and implementation to protect proprietary code.
 *
 * Use this stub for documentation, build examples, and for Certum verification.
 * Do NOT expect full driver functionality from this file.
 *
 * Copyright (c) 2025 Seby Raciti (stub)
 */

#include <ntddk.h>
#include <wdm.h>

#define PRECISION_POOL_TAG 'cPrD'
#define PRECISION_DEVICE_NAME L"\\Device\\PrecisionDrvStub"
#define PRECISION_SYMBOLIC_LINK L"\\DosDevices\\PrecisionDrvStub"

/* IOCTL Codes (same IDs as real driver for compatibility testing) */
#define IOCTL_PRECISION_ENABLE_INTERCEPT    CTL_CODE(FILE_DEVICE_UNKNOWN, 0x800, METHOD_BUFFERED, FILE_ANY_ACCESS)
#define IOCTL_PRECISION_DISABLE_INTERCEPT   CTL_CODE(FILE_DEVICE_UNKNOWN, 0x801, METHOD_BUFFERED, FILE_ANY_ACCESS)
#define IOCTL_PRECISION_GET_INPUT_DATA      CTL_CODE(FILE_DEVICE_UNKNOWN, 0x802, METHOD_BUFFERED, FILE_ANY_ACCESS)
#define IOCTL_PRECISION_SET_BLOCK_POLICY    CTL_CODE(FILE_DEVICE_UNKNOWN, 0x803, METHOD_BUFFERED, FILE_ANY_ACCESS)
#define IOCTL_PRECISION_GET_STATISTICS      CTL_CODE(FILE_DEVICE_UNKNOWN, 0x804, METHOD_BUFFERED, FILE_ANY_ACCESS)
#define IOCTL_PRECISION_GET_DIAGNOSTICS     CTL_CODE(FILE_DEVICE_UNKNOWN, 0x805, METHOD_BUFFERED, FILE_ANY_ACCESS)

typedef struct _DRIVER_STATISTICS {
    LONG TotalInputs;
    LONG BlockedInputs;
    BOOLEAN InterceptEnabled;
} DRIVER_STATISTICS, *PDRIVER_STATISTICS;

/* Forward declarations */
DRIVER_INITIALIZE DriverEntry;
DRIVER_UNLOAD PrecisionUnload;
_Dispatch_type_(IRP_MJ_CREATE) DRIVER_DISPATCH PrecisionDispatchCreate;
_Dispatch_type_(IRP_MJ_CLOSE) DRIVER_DISPATCH PrecisionDispatchClose;
_Dispatch_type_(IRP_MJ_DEVICE_CONTROL) DRIVER_DISPATCH PrecisionDispatchDeviceControl;

/* Global control device */
PDEVICE_OBJECT g_ControlDevice = NULL;

/* Minimal DriverEntry */
NTSTATUS
DriverEntry(
    _In_ PDRIVER_OBJECT DriverObject,
    _In_ PUNICODE_STRING RegistryPath
)
{
    UNREFERENCED_PARAMETER(RegistryPath);

    /* Set up dispatch routines */
    DriverObject->MajorFunction[IRP_MJ_CREATE] = PrecisionDispatchCreate;
    DriverObject->MajorFunction[IRP_MJ_CLOSE] = PrecisionDispatchClose;
    DriverObject->MajorFunction[IRP_MJ_DEVICE_CONTROL] = PrecisionDispatchDeviceControl;
    DriverObject->DriverUnload = PrecisionUnload;

    /* Create a minimal control device (not a functional filter) */
    UNICODE_STRING devName = RTL_CONSTANT_STRING(PRECISION_DEVICE_NAME);
    NTSTATUS status = IoCreateDevice(
        DriverObject,
        0,
        &devName,
        FILE_DEVICE_UNKNOWN,
        0,
        FALSE,
        &g_ControlDevice
    );

    if (!NT_SUCCESS(status)) {
        return status;
    }

    UNICODE_STRING symLink = RTL_CONSTANT_STRING(PRECISION_SYMBOLIC_LINK);
    status = IoCreateSymbolicLink(&symLink, &devName);
    if (!NT_SUCCESS(status)) {
        IoDeleteDevice(g_ControlDevice);
        g_ControlDevice = NULL;
        return status;
    }

    g_ControlDevice->Flags |= DO_BUFFERED_IO;
    g_ControlDevice->Flags &= ~DO_DEVICE_INITIALIZING;

    KdPrint(("PrecisionDrvStub: DriverEntry completed (stub)\n"));
    return STATUS_SUCCESS;
}

/* Minimal unload */
VOID
PrecisionUnload(
    _In_ PDRIVER_OBJECT DriverObject
)
{
    UNREFERENCED_PARAMETER(DriverObject);
    if (g_ControlDevice) {
        UNICODE_STRING symLink = RTL_CONSTANT_STRING(PRECISION_SYMBOLIC_LINK);
        IoDeleteSymbolicLink(&symLink);
        IoDeleteDevice(g_ControlDevice);
        g_ControlDevice = NULL;
    }
    KdPrint(("PrecisionDrvStub: Unloaded\n"));
}

/* Create/Close handlers simply succeed */
NTSTATUS
PrecisionDispatchCreate(
    _In_ PDEVICE_OBJECT DeviceObject,
    _In_ PIRP Irp
)
{
    UNREFERENCED_PARAMETER(DeviceObject);
    Irp->IoStatus.Status = STATUS_SUCCESS;
    Irp->IoStatus.Information = 0;
    IoCompleteRequest(Irp, IO_NO_INCREMENT);
    return STATUS_SUCCESS;
}

NTSTATUS
PrecisionDispatchClose(
    _In_ PDEVICE_OBJECT DeviceObject,
    _In_ PIRP Irp
)
{
    UNREFERENCED_PARAMETER(DeviceObject);
    Irp->IoStatus.Status = STATUS_SUCCESS;
    Irp->IoStatus.Information = 0;
    IoCompleteRequest(Irp, IO_NO_INCREMENT);
    return STATUS_SUCCESS;
}

/* DeviceControl: provide safe, non-functional responses for IOCTLs */
NTSTATUS
PrecisionDispatchDeviceControl(
    _In_ PDEVICE_OBJECT DeviceObject,
    _In_ PIRP Irp
)
{
    UNREFERENCED_PARAMETER(DeviceObject);

    PIO_STACK_LOCATION irpSp = IoGetCurrentIrpStackLocation(Irp);
    ULONG code = irpSp->Parameters.DeviceIoControl.IoControlCode;

    NTSTATUS status = STATUS_SUCCESS;
    ULONG_PTR info = 0;

    switch (code) {
        case IOCTL_PRECISION_ENABLE_INTERCEPT:
            /* Stub: accept but do not enable any kernel interception */
            KdPrint(("PrecisionDrvStub: IOCTL_ENABLE_INTERCEPT (stub)\n"));
            status = STATUS_SUCCESS;
            break;

        case IOCTL_PRECISION_DISABLE_INTERCEPT:
            KdPrint(("PrecisionDrvStub: IOCTL_DISABLE_INTERCEPT (stub)\n"));
            status = STATUS_SUCCESS;
            break;

        case IOCTL_PRECISION_SET_BLOCK_POLICY:
            /* Expect input buffer containing policy structure - stub ignores it */
            KdPrint(("PrecisionDrvStub: IOCTL_SET_BLOCK_POLICY (stub)\n"));
            status = STATUS_SUCCESS;
            break;

        case IOCTL_PRECISION_GET_INPUT_DATA:
            /* No real input data available in stub; return zero bytes */
            status = STATUS_SUCCESS;
            info = 0;
            break;

        case IOCTL_PRECISION_GET_STATISTICS:
            if (Irp->AssociatedIrp.SystemBuffer && irpSp->Parameters.DeviceIoControl.OutputBufferLength >= sizeof(DRIVER_STATISTICS)) {
                DRIVER_STATISTICS stats = { 0 };
                stats.TotalInputs = 0;
                stats.BlockedInputs = 0;
                stats.InterceptEnabled = FALSE;
                RtlCopyMemory(Irp->AssociatedIrp.SystemBuffer, &stats, sizeof(stats));
                info = sizeof(stats);
                status = STATUS_SUCCESS;
            } else {
                status = STATUS_BUFFER_TOO_SMALL;
            }
            break;

        case IOCTL_PRECISION_GET_DIAGNOSTICS:
            /* Diagnostics not available in stub */
            status = STATUS_NOT_IMPLEMENTED;
            break;

        default:
            status = STATUS_INVALID_DEVICE_REQUEST;
            break;
    }

    Irp->IoStatus.Status = status;
    Irp->IoStatus.Information = info;
    IoCompleteRequest(Irp, IO_NO_INCREMENT);
    return status;
}

/* End of stub file */
