# P5.11 validation notes

P5.11 must not be promoted solely from UI inspection. The merge gate requires:

1. Avalonia XAML compilation;
2. Windows, Linux, and macOS solution build;
3. all desktop and portable tests;
4. setting-group persistence tests using isolated temporary paths;
5. source-contract confirmation that the UI does not instantiate a protection engine;
6. process-bus runtime rebuild confirmation after a settings change;
7. manual confirmation that the SETTINGS tab remains usable at the minimum supported window size.

Connector-authored commits may not trigger GitHub Actions automatically. Absence of a workflow run is not a successful result and must be reported as unvalidated until a maintainer-dispatched run completes.
