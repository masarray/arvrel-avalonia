using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Arvrel.Desktop.Controls;

public sealed partial class VirtualRelayFaceplate : UserControl
{
    private static readonly IBrush HardwareNormalBackground = new SolidColorBrush(Color.Parse("#37434A"));
    private static readonly IBrush HardwareNormalBorder = new SolidColorBrush(Color.Parse("#8998A0"));
    private static readonly IBrush HardwareHoverBackground = new SolidColorBrush(Color.Parse("#586871"));
    private static readonly IBrush HardwareHoverBorder = new SolidColorBrush(Color.Parse("#D6E0E4"));
    private static readonly IBrush HardwareFocusBackground = new SolidColorBrush(Color.Parse("#46545C"));
    private static readonly IBrush HardwareFocusBorder = new SolidColorBrush(Color.Parse("#45B6EA"));
    private static readonly IBrush HardwarePressedBackground = new SolidColorBrush(Color.Parse("#2B363C"));
    private static readonly IBrush HardwarePressedBorder = new SolidColorBrush(Color.Parse("#708087"));
    private static readonly IBrush HardwareTextForeground = new SolidColorBrush(Color.Parse("#F7FAFB"));
    private static readonly IBrush HardwareAccentForeground = new SolidColorBrush(Color.Parse("#65C8F5"));

    private readonly HashSet<Button> _wiredHardwareButtons = new();
    private readonly HashSet<Button> _pressedHardwareButtons = new();
    private readonly Dictionary<Button, IBrush> _hardwareForegrounds = new();

    public VirtualRelayFaceplate()
    {
        InitializeComponent();
        AttachedToVisualTree += (_, _) => AttachHardwareButtonStates();
    }

    private void AttachHardwareButtonStates()
    {
        foreach (var button in this
                     .GetVisualDescendants()
                     .OfType<Button>()
                     .Where(candidate => candidate.Classes.Contains("hardware")))
        {
            if (!_wiredHardwareButtons.Add(button))
                continue;

            _hardwareForegrounds[button] = IsAccentHardwareButton(button)
                ? HardwareAccentForeground
                : HardwareTextForeground;

            button.PointerEntered += (_, _) =>
            {
                // A prior capture loss must never leave a hardware key visually stuck down.
                _pressedHardwareButtons.Remove(button);
                ApplyHardwareState(button, HardwareInteractionState.Hover);
            };

            button.PointerExited += (_, _) =>
            {
                _pressedHardwareButtons.Remove(button);
                ApplyHardwareState(
                    button,
                    button.IsFocused
                        ? HardwareInteractionState.Focused
                        : HardwareInteractionState.Normal);
            };

            button.PointerPressed += (_, _) =>
            {
                _pressedHardwareButtons.Add(button);
                ApplyHardwareState(button, HardwareInteractionState.Pressed);
            };

            button.PointerReleased += (_, _) =>
            {
                _pressedHardwareButtons.Remove(button);
                ApplyHardwareState(
                    button,
                    button.IsPointerOver
                        ? HardwareInteractionState.Hover
                        : button.IsFocused
                            ? HardwareInteractionState.Focused
                            : HardwareInteractionState.Normal);
            };

            button.PointerCaptureLost += (_, _) =>
            {
                _pressedHardwareButtons.Remove(button);
                ApplyHardwareState(
                    button,
                    button.IsPointerOver
                        ? HardwareInteractionState.Hover
                        : button.IsFocused
                            ? HardwareInteractionState.Focused
                            : HardwareInteractionState.Normal);
            };

            button.GotFocus += (_, _) =>
            {
                if (!_pressedHardwareButtons.Contains(button))
                {
                    ApplyHardwareState(
                        button,
                        button.IsPointerOver
                            ? HardwareInteractionState.Hover
                            : HardwareInteractionState.Focused);
                }
            };

            button.LostFocus += (_, _) =>
            {
                _pressedHardwareButtons.Remove(button);
                ApplyHardwareState(
                    button,
                    button.IsPointerOver
                        ? HardwareInteractionState.Hover
                        : HardwareInteractionState.Normal);
            };

            ApplyHardwareState(button, HardwareInteractionState.Normal);
        }
    }

    private void ApplyHardwareState(Button button, HardwareInteractionState state)
    {
        var foreground = _hardwareForegrounds.TryGetValue(button, out var configuredForeground)
            ? configuredForeground
            : HardwareTextForeground;

        button.Opacity = 1;
        button.Foreground = foreground;
        if (button.Content is TextBlock textBlock)
            textBlock.Foreground = foreground;

        button.BorderThickness = state == HardwareInteractionState.Focused
            ? new Thickness(1.5)
            : new Thickness(1.2);
        button.Padding = state == HardwareInteractionState.Pressed
            ? new Thickness(7, 5, 7, 3)
            : new Thickness(7, 4);

        switch (state)
        {
            case HardwareInteractionState.Hover:
                button.Background = HardwareHoverBackground;
                button.BorderBrush = button.IsFocused
                    ? HardwareFocusBorder
                    : HardwareHoverBorder;
                break;

            case HardwareInteractionState.Focused:
                button.Background = HardwareFocusBackground;
                button.BorderBrush = HardwareFocusBorder;
                break;

            case HardwareInteractionState.Pressed:
                button.Background = HardwarePressedBackground;
                button.BorderBrush = HardwarePressedBorder;
                break;

            default:
                button.Background = HardwareNormalBackground;
                button.BorderBrush = HardwareNormalBorder;
                break;
        }
    }

    private static bool IsAccentHardwareButton(Button button)
        => string.Equals(button.Content?.ToString(), "RESET", StringComparison.OrdinalIgnoreCase);

    private enum HardwareInteractionState
    {
        Normal,
        Hover,
        Focused,
        Pressed
    }
}
