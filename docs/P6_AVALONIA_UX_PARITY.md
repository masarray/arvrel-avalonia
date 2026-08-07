# P6 Avalonia UX parity

## Objective

Bring the Avalonia operator workspace to functional and visual parity with the approved Windows WPF P6 composition without duplicating protection, capture, trust, injection, annunciation, operation-record, or evidence authorities.

## Completed foundation

- compact product header and source toolbar;
- P6-style SMV waveform workspace;
- native current phasor presentation;
- full-height virtual relay faceplate;
- protection-operation and event evidence region;
- engineering workspaces retained in an overlay tools pane.

## P0.1 — faceplate readability

Completed:

- removed whole-faceplate `Viewbox` scaling;
- made the relay casing stretch within the equipment bay while retaining minimum readable dimensions;
- enlarged LCD text, hardware keys, status labels, and branding;
- rebuilt relay lamps with bezel, cavity, vivid lens, inner glow, bright core, and stronger active colors;
- reduced unused equipment-bay margin while preserving the physical-relay composition.

## P0.2 — operator workspace readability

Completed:

- converted fixed adapter and SV-stream toolbar fields to responsive star-sized columns;
- replaced toolbar Unicode controls with deterministic vector icons and operator tooltips;
- widened the phasor region in DUAL mode;
- reserved an explicit waveform legend band and increased waveform headroom;
- added deterministic phasor-label collision avoidance;
- separated the primary protection operation from secondary stages;
- expanded and made the event trace scrollable;
- replaced the ambiguous `LAB READY` operation badge with the active protection/trust state.

## Remaining visual gates

- screenshot review at 1520 × 900 after P0.2;
- compact-window review at 1280 × 760;
- display-scaling review at 125% and 150%;
- typography comparison on Windows, Ubuntu, and macOS;
- final color, spacing, and equipment-bay polish;
- interaction review for source handover, relay reset, fault injection, SMV degradation, and faceplate navigation.

## Safety boundary

The Avalonia shell is a presentation layer over existing portable/shared authorities. It must not create a second protection engine, capture authority, trust decision, operation recorder, evidence serializer, or physical-output path.
