# UX validation notes

The correction must be validated against the rendered Avalonia control template, not only against `Button.Background` and `Button.Foreground` values. The relay hardware theme owns the complete visual tree so Fluent pseudo-state template parts cannot replace those values.
