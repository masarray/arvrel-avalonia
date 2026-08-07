using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Arvrel.Protection;

namespace Arvrel.Desktop.Controls;

public sealed partial class RelayLamp : UserControl
{
    private static readonly Color OffColor = Color.Parse("#2A3A42");
    private static readonly Color OffStrokeColor = Color.Parse("#53636A");
    private static readonly Color PickupColor = Color.Parse("#FFC94F");
    private static readonly Color TripColor = Color.Parse("#FF515C");

    public static readonly StyledProperty<bool> IsOnProperty =
        AvaloniaProperty.Register<RelayLamp, bool>(nameof(IsOn));

    public static readonly StyledProperty<Color> ActiveColorProperty =
        AvaloniaProperty.Register<RelayLamp, Color>(
            nameof(ActiveColor),
            Color.Parse("#54E882"));

    public static readonly StyledProperty<RelayLampState?> LampStateProperty =
        AvaloniaProperty.Register<RelayLamp, RelayLampState?>(nameof(LampState));

    public RelayLamp()
    {
        InitializeComponent();
        UpdateOptics();
    }

    public bool IsOn
    {
        get => GetValue(IsOnProperty);
        set => SetValue(IsOnProperty, value);
    }

    public Color ActiveColor
    {
        get => GetValue(ActiveColorProperty);
        set => SetValue(ActiveColorProperty, value);
    }

    public RelayLampState? LampState
    {
        get => GetValue(LampStateProperty);
        set => SetValue(LampStateProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsOnProperty ||
            change.Property == ActiveColorProperty ||
            change.Property == LampStateProperty)
        {
            UpdateOptics();
        }
    }

    private void UpdateOptics()
    {
        if (Lens is null ||
            InnerGlow is null ||
            Core is null ||
            Halo is null ||
            GlossSoft is null ||
            GlossSweep is null)
        {
            return;
        }

        if (LampState is { } state)
        {
            switch (state)
            {
                case RelayLampState.Pickup:
                    SetActive(PickupColor);
                    return;
                case RelayLampState.Trip:
                    SetActive(TripColor);
                    return;
                default:
                    SetOff();
                    return;
            }
        }

        if (!IsOn)
        {
            SetOff();
            return;
        }

        SetActive(ActiveColor);
    }

    private void SetOff()
    {
        Lens.Fill = new SolidColorBrush(OffColor);
        Lens.Stroke = new SolidColorBrush(OffStrokeColor);
        InnerGlow.Fill = Brushes.Transparent;
        InnerGlow.Opacity = 0;
        Core.Fill = Brushes.Transparent;
        Core.Opacity = 0;
        Halo.Opacity = 0;
        GlossSoft.Opacity = 0.11;
        GlossSweep.Opacity = 0.07;
    }

    private void SetActive(Color color)
    {
        Lens.Fill = new SolidColorBrush(BlendWithBlack(color, 0.10));
        Lens.Stroke = new SolidColorBrush(BlendWithWhite(color, 0.22));

        InnerGlow.Fill = new SolidColorBrush(Color.FromArgb(178, color.R, color.G, color.B));
        InnerGlow.Opacity = 0.56;

        Core.Fill = new SolidColorBrush(BlendWithWhite(color, 0.46));
        Core.Opacity = 0.84;

        Halo.Opacity = 0.48;
        GlossSoft.Opacity = 0.24;
        GlossSweep.Opacity = 0.13;
    }

    private static Color BlendWithWhite(Color color, double amount)
    {
        static byte Blend(byte channel, double factor)
            => (byte)Math.Clamp(
                (int)Math.Round(channel + ((255 - channel) * factor)),
                0,
                255);

        return Color.FromArgb(
            255,
            Blend(color.R, amount),
            Blend(color.G, amount),
            Blend(color.B, amount));
    }

    private static Color BlendWithBlack(Color color, double amount)
    {
        static byte Blend(byte channel, double factor)
            => (byte)Math.Clamp(
                (int)Math.Round(channel * (1 - factor)),
                0,
                255);

        return Color.FromArgb(
            255,
            Blend(color.R, amount),
            Blend(color.G, amount),
            Blend(color.B, amount));
    }
}
