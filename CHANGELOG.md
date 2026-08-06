# Changelog

All notable changes to the ARVREL Avalonia engineering preview are documented here. This repository has no stable public release yet.

## [Unreleased]

### Repository split

- established `masarray/arvrel-avalonia` as the dedicated cross-platform migration repository;
- imported the final P5.9 migration snapshot from `masarray/arvrel`;
- separated issue scope, CI, metadata, and documentation from the stable Windows WPF P6 product;
- retained the shared protection, capture, process-bus, and application layers required by the Avalonia preview;
- excluded WPF source, WPF installer/release machinery, generated outputs, and pre-split Git history.

### Current migration baseline

- **P5.0** — platform-neutral application orchestration boundary;
- **P5.1** — capture abstraction and PCAP/PCAPNG replay separation;
- **P5.2** — Avalonia base shell and independent desktop solution;
- **P5.3** — waveform axis and frequency-aware presentation work;
- **P5.4** — virtual injection and relay settings parity foundation;
- **P5.5** — cross-platform packaging research and distribution contracts;
- **P5.6–P5.7** — Avalonia virtual relay and annunciation work;
- **P5.8** — process-bus source workspace;
- **P5.9** — guarded live-display handover.

### Known gaps

- visual parity with the WPF P6 real-device faceplate is incomplete;
- cross-platform packages are not yet a supported public release channel;
- clean-machine validation and operating-system-specific capture support remain promotion gates;
- the preview must not be presented as replacing the stable Windows product.

## Source history

Detailed pre-split milestone documents remain under `docs/`. The full development history before the repository split is preserved in `masarray/arvrel` on branch `archive/avalonia-p5.9-final-before-split`.
