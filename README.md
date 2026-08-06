# ARVREL Avalonia

[![Avalonia cross-platform CI](https://github.com/masarray/arvrel-avalonia/actions/workflows/ci.yml/badge.svg)](https://github.com/masarray/arvrel-avalonia/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/license-GPL--3.0--or--later-0b7285)](LICENSE)
[![Status](https://img.shields.io/badge/status-engineering%20preview-b45309)](MIGRATION_STATUS.md)

Cross-platform migration and engineering preview of the ARVREL Virtual Protection Relay Laboratory.

> **Repository boundary:** this repository owns Avalonia and cross-platform migration work. The released Windows WPF P6 product remains in [`masarray/arvrel`](https://github.com/masarray/arvrel).

## Status

This is an active preview, not the current public production release. Cross-platform availability alone is not sufficient for promotion: functional parity, visual parity, packaging, clean-machine validation, and protection/trust regression gates must all pass.

See [`MIGRATION_STATUS.md`](MIGRATION_STATUS.md) for the current boundary and promotion criteria.

## Current snapshot

- migration milestone: **P5.9**;
- imported source snapshot: `8ecec49933f7043c77a8f93b30d0cfe9803d5fff`;
- current repository baseline: `main`;
- Avalonia application: `src/Arvrel.Desktop`;
- portable application layer: `src/Arvrel.Application`;
- capture abstraction: `src/Arvrel.Capture`;
- process-bus runtime: `src/Arvrel.ProcessBus`;
- protection core: `src/Arvrel.Protection`;
- scoped solution: `desktop/ARVREL.Desktop.sln`.

## Source layout

Full decoder and process-bus validation expects the public ARIEC61850 repository beside this repository:

```text
Git/
├── ARIEC61850/
└── arvrel-avalonia/
```

GitHub Actions checks out the pinned decoder automatically.

## Build and test

```powershell
.\scripts\build.ps1
```

Equivalent commands:

```powershell
cd desktop
dotnet restore ARVREL.Desktop.sln
dotnet build ARVREL.Desktop.sln -c Release --no-restore
dotnet test ARVREL.Desktop.sln -c Release --no-build
```

Run the preview:

```powershell
.\scripts\run.ps1
```

## Migration boundary

The repository retains the shared protection, process-bus, capture, and application projects required by the Avalonia client. It intentionally excludes:

- the WPF product project `src/Arvrel.App`;
- WPF P6 implementation and WPF-only visual tests;
- the stable Windows installer and release publication workflow;
- stable-product release versioning and public website.

Visual target: functional and visual parity with the approved WPF P6 real-device interface without duplicating protection, trust, capture, injection, or evidence authorities.

## Issue routing

- Avalonia, Linux, macOS, cross-platform UI, and migration issues: use this repository.
- Released Windows WPF P6 issues: use [`masarray/arvrel`](https://github.com/masarray/arvrel/issues).

## Safety

ARVREL is virtual-output laboratory software. It is not a certified protection IED, calibrated relay test set, switching authority, IEC 61850 conformance result, IEC 60255 type-test platform, or hard-real-time trip system.

Use live capture only on isolated and authorized laboratory networks.

## License

GPL-3.0-or-later. Third-party components retain their own licenses.
