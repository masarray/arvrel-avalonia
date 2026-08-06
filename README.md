# ARVREL Avalonia

Cross-platform migration and engineering preview of the ARVREL Virtual Protection Relay Laboratory.

> **Status:** active preview, not the current public production release. The stable Windows release remains in `masarray/arvrel` and uses the WPF P6 real-device interface.

## Purpose

This repository isolates the Avalonia migration so cross-platform architecture, packaging, process-bus acquisition, waveform presentation, and relay-faceplate parity can evolve without reducing the quality of the stable Windows product.

## Current snapshot

- migration milestone: **P5.9**;
- source snapshot: `8ecec49933f7043c77a8f93b30d0cfe9803d5fff`;
- Avalonia application: `src/Arvrel.Desktop`;
- portable application layer: `src/Arvrel.Application`;
- capture abstraction: `src/Arvrel.Capture`;
- process-bus runtime: `src/Arvrel.ProcessBus`;
- protection core: `src/Arvrel.Protection`;
- scoped solution: `desktop/ARVREL.Desktop.sln`.

## Build and test

```powershell
.\scripts\build.ps1
```

Equivalent manual commands:

```powershell
cd desktop
dotnet restore ARVREL.Desktop.sln
dotnet build ARVREL.Desktop.sln -c Release --no-restore
dotnet test ARVREL.Desktop.sln -c Release --no-build
```

Run the application:

```powershell
.\scripts\run.ps1
```

## Migration boundary

The preview retains the shared protection, process-bus, capture, and application projects. The WPF application, Windows installer, WPF-only source-contract tests, and WPF release workflow are intentionally excluded.

The visual target is functional and visual parity with the P6 real-device interface. Cross-platform availability alone is not considered sufficient for replacing the stable product.

## Safety

ARVREL is virtual-output laboratory software. It is not a certified protection IED, calibrated relay test set, switching authority, IEC 61850 conformance result, IEC 60255 type-test platform, or hard-real-time trip system.

Use live capture only on isolated and authorized laboratory networks.

## License

GPL-3.0-or-later. Third-party components retain their own licenses.

