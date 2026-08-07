# P0.4 operator UX correction

This milestone replaces the failed relay-button styling override with a dedicated Avalonia `ControlTheme` and `ControlTemplate`, so Fluent theme pseudo-state visuals cannot replace the relay button background or foreground.

The injection tab is reduced to two visual heroes:

- current outputs (`4 I`);
- voltage outputs (`4 V`).

Per-row provenance prose and repeated explanatory labels were removed from the primary operator view. Existing injection commands and the portable laboratory authority remain unchanged.
