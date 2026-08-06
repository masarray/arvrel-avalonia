# Avalonia migration status

## Split status

The repository split is complete.

- stable Windows WPF product: [`masarray/arvrel`](https://github.com/masarray/arvrel);
- cross-platform migration repository: [`masarray/arvrel-avalonia`](https://github.com/masarray/arvrel-avalonia);
- final source snapshot imported from: `8ecec49933f7043c77a8f93b30d0cfe9803d5fff`;
- preserved pre-split branch in the stable repository: `archive/avalonia-p5.9-final-before-split`;
- current status: experimental engineering preview.

The two repositories have independent issue scope, CI, release decisions, and presentation roadmaps. Changes are not automatically synchronized.

## Included here

- Avalonia shell and virtual-relay faceplate;
- internal injection workflow;
- live-capture and PCAP replay abstractions;
- SCL and Sampled Values process-bus workspace;
- guarded display handover and trust presentation;
- shared Application, Capture, ProcessBus, and Protection libraries required by the preview;
- Avalonia and shared-core regression tests;
- cross-platform source CI for Windows, Linux, and macOS.

## Not included here

- WPF application project `src/Arvrel.App`;
- WPF P6 implementation and WPF-only visual source-contract tests;
- stable Windows installer and public release workflow;
- the stable product website and release versioning;
- generated build outputs or pre-split Git history.

## Current limitations

- the Avalonia faceplate has not yet reached visual parity with the approved WPF P6 real-device UX;
- packaging and clean-machine validation are not yet a supported release channel;
- platform-specific live capture remains subject to backend availability and operating-system permissions;
- this repository does not replace the stable Windows product.

## Promotion gate

The Avalonia edition must not replace the WPF stable product until it reaches:

1. functional parity;
2. visual parity with the approved P6 device-like UX;
3. validated packaging on Windows, Linux, and macOS;
4. clean-machine installation and startup checks;
5. no regression in protection, trust, injection, process-bus, or evidence behavior;
6. an explicit release decision documented in both repositories.
