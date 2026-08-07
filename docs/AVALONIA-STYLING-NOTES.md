# Avalonia styling notes

Relay hardware buttons use a dedicated `ControlTheme` with a complete `ControlTemplate`. Pointer-over, pressed, focus-visible, and disabled states are expressed with Avalonia pseudo-class selectors. The Fluent button template is not used for relay hardware keys, preventing internal theme parts from replacing the requested background and foreground.

The operator injection view keeps only the information required to configure and execute a secondary-injection test: channel enable, signal, RMS magnitude, phase angle, preset, frequency, and primary actions.
