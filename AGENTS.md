# AGENTS.md — ARVREL Avalonia Production Engineering Contract

These rules apply to every AI/code agent working in this repository. ARVREL Avalonia is the cross-platform migration/engineering-preview surface of the ARVREL Virtual Protection Relay Laboratory. Protection correctness, process-bus trust, deterministic evidence, migration parity, bounded rendering/data flow, and regression safety are product requirements from the first implementation.

## 1. Prime directive

Do not begin with a deliberately naive, disposable, prototype-only implementation when the production architecture is knowable.

Choose the smallest production-quality solution that satisfies the requirement without speculative complexity.

Priority order:
1. protection and engineering correctness;
2. process-bus/capture data integrity and failure containment;
3. behavioral parity with approved product contracts;
4. regression compatibility;
5. UI responsiveness and bounded CPU/memory;
6. cross-platform maintainability and testability.

Cross-platform availability or visual similarity is never proof of product parity.

## 2. Mandatory engineering loop

For non-trivial work:

RECONNAISSANCE -> REPRODUCE/BASELINE -> ROOT CAUSE -> INVARIANTS -> MIGRATION/PARITY IMPACT -> IMPLEMENT -> REGRESSION TEST -> FAILURE TEST -> PERFORMANCE CHECK -> CROSS-PLATFORM BUILD -> WORKFLOW VALIDATION

Before editing:
- identify whether ownership belongs to Protection, ProcessBus, Capture, Application, or Avalonia Desktop;
- trace shared-domain state and all UI consumers;
- identify WPF-product behavior used as approved parity target without copying WPF-only implementation structure unnecessarily;
- locate existing migration status, tests, decoder pinning, lifecycle/cancellation paths, and safety boundaries;
- define what must not regress;
- determine root cause before introducing platform-specific forks, duplicate services, timers, or UI workarounds.

If an attempted fix fails, stop and re-audit assumptions. Do not stack workaround on workaround.

Three symptom patches in one subsystem trigger architecture/root-cause re-audit before a fourth patch.

## 3. Repository/migration boundaries

The portable/shared layers remain authoritative for engineering behavior:

Avalonia Desktop
-> Application orchestration
-> Capture / ProcessBus services
-> Protection core

Rules:
- Avalonia views/view-models do not become an alternative protection engine;
- protection, trust, capture, injection, and evidence authorities stay in portable/shared layers where designed;
- do not duplicate WPF implementation simply to achieve visual parity;
- platform adapters may differ, engineering semantics may not silently diverge;
- keep the pinned ARIEC61850 decoder/source contract explicit and qualified;
- migration changes must not silently redefine stable Windows product semantics.

## 4. Result-oriented failure handling

Expected/recoverable engineering and platform failures should use explicit typed Result/Try/status contracts rather than exception-driven normal control flow.

Examples:
- malformed/unsupported process-bus frame;
- capture unavailable;
- adapter/device open failure;
- decoder rejection;
- invalid settings/project state;
- cancellation/timeout;
- unsupported platform capability;
- evidence export failure;
- invalid numerical/protection input.

For C# prefer nullable reference types, guards, `TryXxx` APIs, coherent typed result/error records, cancellation tokens, and explicit state transitions.

Do not create an ad-hoc Result type for every layer. Keep error taxonomy coherent by domain/boundary.

OS, socket, file, Avalonia, capture-library, decoder, or third-party exceptions may still occur. Catch them at meaningful infrastructure/application boundaries and convert them to structured failures before they become UI crashes or corrupt shared state.

Do not scatter broad catch-all handlers through per-frame/hot loops and do not silently swallow failures.

## 5. Protection and numerical correctness

Protection calculations must remain deterministic and explicit about invalid/unknown inputs.

Validate:
- NaN/Infinity and divide-by-zero;
- sample rate/frequency assumptions;
- channel availability and phase mapping;
- timestamp/order/sequence validity;
- configuration/range limits;
- stale/partial/malformed source data.

Do not invent missing engineering data to make a plot or protection decision appear complete.

Do not change trip/start/zone/threshold semantics as a UI migration shortcut.

A visualization can downsample representation; protection calculations must use the required engineering-resolution data.

## 6. Process-bus/capture hot paths

GOOSE/SV/capture processing can be high frequency and must be bounded.

Do not:
- render one UI update per packet/sample;
- create one Task/thread per event;
- perform synchronous file/UI/logging work in capture callbacks;
- allow unbounded frame/evidence queues;
- retain duplicate full captures without an explicit need.

Use bounded queues/channels, batching, coalescing, latest-state semantics for presentation, or backpressure according to whether loss is allowed.

Lossless engineering evidence and presentation telemetry are different contracts. Do not drop required evidence just because UI can coalesce.

## 7. Asynchronous diagnostics

High-rate processing must not synchronously format or persist diagnostics per event.

Hot producers emit compact structured error/status events/counters into bounded non-blocking channels/queues where appropriate.

A background consumer may aggregate, deduplicate/rate-limit, format, persist, and publish UI diagnostic summaries.

Queue saturation has explicit semantics. A slow/full/broken diagnostic sink must never block process-bus processing, protection evaluation, capture shutdown, or UI interaction.

Diagnostics are observational; diagnostic failure cannot become protection/runtime failure.

## 8. Zero UI blocking

The Avalonia UI thread exists for rendering and interaction.

Never perform synchronous long-running:
- capture/file loading;
- network/device discovery;
- large trace parsing;
- bulk protection replay/calculation;
- report/export generation;
- package/update work

on the UI thread.

For 60 Hz interfaces, ~16.7 ms is the total frame budget. Move expensive CPU work to bounded background execution and use async I/O for I/O-bound work.

Marshal minimal immutable/validated state back to UI.

Never use arbitrary delay/sleep as the primary repair for a race or lifecycle issue.

## 9. Rendering, virtualization, and LOD

Large tables, traces, event logs, packet views, waveforms, vectors, and history surfaces must scale with visible data rather than total dataset size.

Use virtualization/lazy loading/paging and viewport-aware downsampling/LOD where appropriate.

For waveform/transient engineering views, preserve extrema/significant events rather than using averaging that hides short disturbances.

Keep static plot layers separate from cursors/hover/live overlays so interaction does not trigger full-data rebuilds.

## 10. State ownership and transactional promotion

Maintain one authoritative state owner for active lab/session/protection configuration.

Candidate settings, decoder state, imported data, or platform resources should be fully validated/prepared before replacing last-known-good state where practical.

Failed migration/import/reconfigure must not leave a half-applied project or mismatched UI/core state.

UI/editor state is a projection/controller of engineering state, not a second mutable authority.

## 11. Resource lifecycle

Every capture handle, socket, worker, subscription, timer, file, cancellation source, native/platform object, and long-lived callback has a clear owner and shutdown path.

Stop/reload/close must:
- prevent new work entering;
- cancel/retire active work;
- release handles/resources;
- prevent callbacks into disposed/superseded state;
- avoid duplicate subscriptions after reopen/navigation.

Do not detach tasks/threads merely to avoid lifecycle management.

## 12. Cross-platform discipline

Platform differences must be isolated behind explicit adapters or capability checks.

Do not:
- pepper shared protection code with OS-specific branches;
- claim parity because the app starts on Linux/macOS;
- replace unsupported functionality with silent fake success;
- let one platform workaround change engineering semantics for all platforms.

Unsupported capabilities remain explicit and testable.

## 13. Performance contract

For performance-sensitive changes measure relevant signals where practical:
- startup-to-interactive time;
- capture/process-bus throughput;
- queue depth/drop count;
- replay/analysis latency;
- UI frame/jank behavior;
- allocation rate/GC pressure;
- working set;
- waveform/trace render latency;
- shutdown/reopen latency.

For the same qualified scenario, a >10% regression in a relevant metric requires explicit explanation/review. This is a visibility threshold, not an automatic rejection when correctness/capability justifies the trade-off.

Do not claim optimization without evidence.

## 14. Migration/parity regression gates

Every bug or migration fix should protect the exact behavior with deterministic tests/checks where practical.

Promotion from engineering preview requires evidence across:
- functional parity;
- protection/trust behavior;
- process-bus/capture behavior;
- visual/interaction parity where required;
- packaging;
- clean-machine startup;
- cross-platform behavior;
- failure/cancellation paths;
- performance/resource stability.

Do not mark parity complete from screenshots alone.

## 15. Safety and evidence

ARVREL remains virtual-output laboratory software. Do not introduce wording or behavior implying certified protection IED, calibrated test set, conformance certification, switching authority, or hard-real-time trip guarantees.

Live network/capture/injection work must respect existing isolated/authorized laboratory boundaries.

Evidence output must distinguish observed facts, derived calculations, warnings, unknowns, and unsupported conditions.

## 16. Definition of done

A task is not complete because `dotnet build` succeeds.

Validate as applicable:
RELEASE BUILD
+ UNIT/REGRESSION TESTS
+ MALFORMED/FAILURE-PATH TESTS
+ PROTECTION NUMERICAL CHECKS
+ PROCESS-BUS/CAPTURE FIXTURES
+ UI RESPONSIVENESS/VIRTUALIZATION
+ RESOURCE/LIFECYCLE CHECK
+ CROSS-PLATFORM CI/BUILD
+ PACKAGING/CLEAN-MACHINE CHECK
+ REAL LAB WORKFLOW WHEN REQUIRED

Never claim validation that was not actually executed.

## 17. Completion report

Report:
- Changed;
- Root cause;
- architecture/state ownership decision;
- Result/error contract affected;
- migration/parity impact;
- regression protection;
- measured performance impact or why not performance-sensitive;
- exact validation executed;
- genuine remaining limitations.

## Final rule

Think like the engineer maintaining one protection laboratory product across multiple platforms for years, not like a migration script trying to make an Avalonia window resemble WPF.

Preserve shared engineering authority. Keep process-bus hot paths bounded. Make failures explicit. Keep UI light. Measure parity. Fix root causes. Prevent regressions.