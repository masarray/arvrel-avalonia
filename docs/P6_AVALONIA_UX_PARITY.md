# P6 Avalonia UX parity

## Objective

The default Avalonia operator workspace must follow the approved WPF P6 information hierarchy while remaining a native, cross-platform Avalonia implementation.

The target composition is:

1. compact product header;
2. source, adapter, SV stream, and view toolbar;
3. large SMV waveform and phasor evidence workspace;
4. protection-operation summary and event trace;
5. full-height physical-relay faceplate;
6. compact engineering status footer.

Advanced source editing, 4I+4V injection, process-bus configuration, protection settings, evidence export, and event history remain available through the engineering-tools pane. They must not permanently reduce the primary monitoring workspace.

## Authority boundary

Visual parity must not introduce a second protection or measurement authority.

- waveform data comes from the existing selected display source;
- phasors come from `PhasorDisplayProjector` and the existing measurement frame;
- lamps and LCD content come from the existing annunciation and faceplate projection;
- source switching continues to use guarded process-bus handover;
- relay reset, source reset, settings application, evidence export, and capture/replay retain their existing semantics.

## Acceptance criteria

- the relay faceplate is readable at the default 1520 × 900 window size;
- waveform and phasor plots are visible simultaneously in the default DUAL view;
- source/adapter/stream context remains visible without opening the tools pane;
- protection operation and event trace remain visible below the signal workspace;
- the tools pane exposes all migration-era engineering functions;
- the application builds and tests on Windows, Linux, and macOS;
- no WPF assembly or `System.Windows` dependency is introduced;
- no raster screenshot is used as the relay implementation;
- manual screenshots are reviewed at default and minimum supported window sizes before merge.

## Remaining visual review

The first rendered build should be compared with the approved P6 reference for:

- faceplate width and vertical scale;
- waveform/phasor split ratio;
- header and toolbar density;
- typography and engineering-value alignment;
- panel borders and background contrast;
- minimum-window behavior and tools-pane overlay.
