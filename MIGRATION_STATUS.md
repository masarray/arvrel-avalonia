# Avalonia migration status

## Origin

This repository-ready snapshot was separated from `masarray/arvrel` after the stable product returned to the WPF P6 real-device UX.

- final Avalonia source commit: `8ecec49933f7043c77a8f93b30d0cfe9803d5fff`;
- preserved source branch: `archive/avalonia-p5.9-final-before-split`;
- recommended new repository: `masarray/arvrel-avalonia`;
- intended status: experimental / engineering preview.

## Included

- Avalonia shell and virtual relay faceplate;
- internal injection workflow;
- live capture and PCAP replay abstractions;
- SCL and Sampled Values stream workspace;
- guarded process-bus display handover;
- shared protection and measurement libraries;
- Avalonia and shared-core regression tests;
- Avalonia packaging scripts;
- cross-platform CI definition.

## Excluded

- WPF application project `src/Arvrel.App`;
- WPF-only visual source-contract tests;
- WPF installer and release publication workflow;
- stable-product version, release notes, and public website;
- generated build outputs and Git history.

## Promotion gate

The Avalonia edition should not replace the WPF stable product until it reaches:

1. functional parity;
2. visual parity with the approved P6 device-like UX;
3. stable packaging on Windows, Linux, and macOS;
4. clean-machine validation;
5. no regression in protection, trust, injection, or process-bus evidence behavior.
