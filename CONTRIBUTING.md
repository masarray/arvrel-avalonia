# Contributing to ARVREL Avalonia

Thank you for helping improve the cross-platform ARVREL engineering preview. Contributions should preserve protection correctness, evidence integrity, deterministic behavior, and the virtual-output safety boundary.

## Repository scope

This repository owns the Avalonia desktop migration and its cross-platform presentation, packaging research, and compatibility work.

Use [`masarray/arvrel`](https://github.com/masarray/arvrel) for issues and changes that apply only to the stable Windows WPF P6 product.

## Before starting

For non-trivial work, open an issue and describe:

- the engineering or migration problem;
- affected operating systems and runtime identifiers;
- the expected operator behavior;
- protection, process-bus, or IEC 61850 impact;
- intended tests and evidence;
- compatibility and safety implications.

Small documentation corrections may be submitted directly.

## Development baseline

Recommended source layout:

```text
Git/
├── ARIEC61850/
└── arvrel-avalonia/
```

Build and test:

```powershell
cd C:\Git\arvrel-avalonia
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

A pull request must:

- build with zero warnings and zero errors;
- preserve Windows, Linux, and macOS source compatibility unless the limitation is explicit;
- include deterministic regression tests for changed behavior;
- preserve the SMV trust policy and virtual-output-only boundary;
- avoid unsupported compliance, calibration, performance, or safety claims;
- document user-visible changes and remaining parity gaps;
- avoid generated binaries, local preferences, captures, secrets, or proprietary SCL data.

## Protection and process-bus changes

Changes affecting protection algorithms, trip attribution, timestamps, quantities, phasors, `smpCnt`, quality, mapping, scaling, or evidence export require tests for the intended operation and at least one secure or restrained condition.

Visual parity must not create a second protection authority. Avalonia controls consume shared immutable state; they must not infer pickup or trip from appearance.

## Commit and pull-request quality

Use focused commits. A strong pull request includes:

1. root cause;
2. implementation boundary;
3. affected platforms;
4. regression tests;
5. build/test evidence;
6. operator-visible result;
7. remaining limitations.

## Licensing, security, and conduct

Code contributions may require acceptance of [`CLA.md`](CLA.md). Do not submit material you do not have the right to contribute.

Do not publish suspected vulnerabilities or restricted operational data. Follow [`SECURITY.md`](SECURITY.md).

Participation is governed by [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).
