# P5.11 — Full protection-settings parity

## Scope

P5.11 completes the Avalonia settings path for the portable protection functions already implemented by `Arvrel.Protection`.

The milestone covers:

- 50P phase instantaneous;
- 51P phase time overcurrent;
- 50N earth instantaneous;
- 51N earth time overcurrent;
- 27 undervoltage;
- 59 overvoltage;
- 59N residual overvoltage;
- 67P directional phase overcurrent;
- 67N directional earth overcurrent;
- user-defined IEC-form inverse curves;
- persistent protection setting groups.

## Authority boundary

Avalonia does not calculate pickup, timing, direction, or trip from UI state. The editor produces a validated immutable `ProtectionSettings` value. Internal laboratory and process-bus protection remain owned by the portable protection engines.

Applying a setting group:

1. validates all enum and engineering-value fields through `ProtectionSettings.Validate()`;
2. updates the internal laboratory while preserving the configured injection source and run state;
3. updates the process-bus controller;
4. clears process-bus stream runtimes so fresh frames rebuild timers, latches, and operation records under one coherent settings identity;
5. returns an active process-bus display to the internal source until a new coherent two-cycle SV window is available.

## User-defined curves

51P and 51N expose `K`, `alpha`, and `C` for `IecCurveFamily.UserDefined`:

```text
t = TMS × (K / (M^alpha − 1) + C)
```

The parameters are stored in `ProtectionSettings`, included in its fingerprint, validated by the portable model, and consumed by `ProtectionEngine` through `IecCurveCalculator`.

## Persistent setting groups

`ProtectionSettingGroupStore` owns the portable JSON format:

```text
arvrel.protection-setting-groups/v1
```

The desktop application stores the catalog under the platform local-application-data directory:

```text
ARVREL-Avalonia/protection-setting-groups.json
```

Properties:

- enum values are serialized as names;
- group names are unique case-insensitively;
- groups are normalized and sorted;
- the active group must exist in the catalog;
- every restored group is revalidated;
- writes use a temporary file followed by atomic replacement;
- malformed or unsupported documents are rejected without replacing active settings.

The parameterless ViewModel constructor does not restore LocalAppData, keeping headless tests deterministic. The desktop `App` explicitly enables automatic restoration.

## Presentation

The relay workspace receives a dedicated **SETTINGS** tab. The existing compact RELAY view remains a second view of the same editor and ViewModel, not a second settings or protection authority.

Protection status cards now include all nine functions and follow the same selected display source as measurement, waveform, annunciation, faceplate, and evidence export.

## Validation

Regression coverage includes:

- full editor-to-model mapping;
- user-defined 51P and 51N parameters;
- 27/59 measurement modes and phase-selection logic;
- 59N residual settings;
- 67P/67N sense, RCA, pickup, delay, dropout, and minimum polarizing voltage;
- invalid angle and undervoltage-reset rejection;
- setting-group JSON round trip;
- active-group restoration;
- malformed and duplicate catalog rejection;
- nine-card display projection;
- source-level protection-authority and process-bus rebuild guards.
