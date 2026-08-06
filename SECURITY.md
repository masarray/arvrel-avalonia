# Security policy

## Supported scope

Security fixes are considered for the current `main` branch of the ARVREL Avalonia engineering preview. This repository does not yet publish a stable or supported production release.

The stable Windows WPF product is maintained separately in [`masarray/arvrel`](https://github.com/masarray/arvrel) and has its own release and support lifecycle.

## Reporting a vulnerability

Use GitHub's **private security advisory** workflow for this repository. Do not disclose suspected vulnerabilities in a public issue, pull request, discussion, screenshot, or video before coordinated disclosure.

Include, when available:

- affected commit;
- operating system, architecture, and display environment;
- reproduction steps or a minimal synthetic fixture;
- expected and observed behavior;
- security or operational impact;
- whether internal injection, live capture, PCAP replay, SCL import, settings, or evidence export is involved.

Do not attach customer captures, employer data, substation SCL files, credentials, network plans, device addresses, or other restricted operational information. Replace them with synthetic data.

## Operational safety boundary

ARVREL Avalonia is experimental laboratory software. It has no active GOOSE trip, MMS control, relay contact, autonomous switching path, or switching authority. It does not establish IEC 61850 conformance, IEC 60255 type-test status, calibration, functional safety, or deterministic real-time performance.

Use live process-bus features only on isolated, authorized test networks. Never connect an experimental build to an operational process bus without an approved test plan, independent controls, and asset-owner authorization.

## Security-sensitive design requirements

Any future active network-output function must remain disabled by default, expose destination and armed state, support dry run, preserve independent evidence, and require explicit laboratory arming. Such functionality is outside the current preview scope.

## Dependency and CI controls

The repository uses pinned critical source dependencies, automated dependency review, and cross-platform CI. A passing CI run is engineering evidence, not a security certification or production-readiness claim.
