using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Arvrel.Protection;

namespace Arvrel.Desktop.Controls;

public sealed class PhasorScope : Control
{
    public static readonly StyledProperty<PhasorDisplayFrame?> FrameProperty =
        AvaloniaProperty.Register<PhasorScope, PhasorDisplayFrame?>(nameof(Frame));

    private static readonly Typeface Typeface = new("Inter");
    private static readonly Typeface MonoTypeface = new("Cascadia Mono,Consolas,monospace");
    private static readonly IBrush GridBrush = new SolidColorBrush(Color.Parse("#22343E"));
    private static readonly IBrush MajorGridBrush = new SolidColorBrush(Color.Parse("#344B57"));
    private static readonly IBrush LabelBrush = new SolidColorBrush(Color.Parse("#8299A5"));
    private static readonly IBrush StrongLabelBrush = new SolidColorBrush(Color.Parse("#AFC0C8"));
    private static readonly IBrush PhaseABrush = new SolidColorBrush(Color.Parse("#F04F55"));
    private static readonly IBrush PhaseBBrush = new SolidColorBrush(Color.Parse("#F2C84B"));
    private static readonly IBrush PhaseCBrush = new SolidColorBrush(Color.Parse("#4B91EA"));
    private static readonly IBrush ResidualBrush = new SolidColorBrush(Color.Parse("#42CC82"));
    private static readonly IBrush NeutralBrush = new SolidColorBrush(Color.Parse("#A3B2BA"));

    static PhasorScope()
        => AffectsRender<PhasorScope>(FrameProperty);

    public PhasorScope()
        => ClipToBounds = true;

    public PhasorDisplayFrame? Frame
    {
        get => GetValue(FrameProperty);
        set => SetValue(FrameProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var width = Bounds.Width;
        var height = Bounds.Height;
        if (width < 100 || height < 120)
            return;

        var footerHeight = Math.Min(42, height * 0.16);
        var plotHeight = height - footerHeight;
        var center = new Point(width * 0.5, plotHeight * 0.52);
        var radius = Math.Max(24, Math.Min(width * 0.40, plotHeight * 0.39));

        DrawGrid(context, center, radius);
        DrawHeader(context, width);

        if (Frame is not { IsAvailable: true } frame || frame.Vectors.Count == 0)
        {
            DrawCenteredText(context, Frame?.Status ?? "Waiting for phasor data.", center, 10, LabelBrush);
            DrawFooter(context, width, height, footerHeight, Frame);
            return;
        }

        var maxMagnitude = frame.Vectors.Select(vector => vector.Magnitude).DefaultIfEmpty(1).Max();
        maxMagnitude = Math.Max(maxMagnitude, 1e-9);

        foreach (var vector in frame.Vectors)
            DrawVector(context, center, radius, vector, maxMagnitude);

        DrawFooter(context, width, height, footerHeight, frame);
    }

    private static void DrawGrid(DrawingContext context, Point center, double radius)
    {
        var gridPen = new Pen(GridBrush, 1);
        var majorPen = new Pen(MajorGridBrush, 1.1);

        for (var ring = 1; ring <= 4; ring++)
            context.DrawEllipse(null, ring == 4 ? majorPen : gridPen, center, radius * ring / 4d, radius * ring / 4d);

        for (var angle = 0; angle < 360; angle += 30)
        {
            var radians = angle * Math.PI / 180d;
            var endpoint = new Point(
                center.X + Math.Cos(radians) * radius,
                center.Y - Math.Sin(radians) * radius);
            context.DrawLine(angle % 90 == 0 ? majorPen : gridPen, center, endpoint);
        }

        DrawText(context, "0°", new Point(center.X + radius + 5, center.Y - 6), 9, LabelBrush);
        DrawText(context, "+90°", new Point(center.X - 14, center.Y - radius - 18), 9, LabelBrush);
        DrawText(context, "-90°", new Point(center.X - 14, center.Y + radius + 5), 9, LabelBrush);
        DrawText(context, "±180°", new Point(center.X - radius - 34, center.Y - 6), 9, LabelBrush);
    }

    private static void DrawHeader(DrawingContext context, double width)
    {
        DrawText(context, "PHASOR", new Point(8, 7), 9.5, StrongLabelBrush, FontWeight.SemiBold);
        var right = new FormattedText(
            "CURRENT",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface,
            8.5,
            LabelBrush);
        context.DrawText(right, new Point(Math.Max(8, width - right.Width - 8), 8));
    }

    private static void DrawVector(
        DrawingContext context,
        Point center,
        double radius,
        PhasorDisplayVector vector,
        double maximum)
    {
        var brush = ResolveBrush(vector.Key);
        var pen = new Pen(brush, vector.IsResidual ? 2.4 : 2.1);
        var length = radius * Math.Clamp(vector.Magnitude / maximum, 0.08, 0.94);
        var radians = vector.AngleDegrees * Math.PI / 180d;
        var tip = new Point(
            center.X + Math.Cos(radians) * length,
            center.Y - Math.Sin(radians) * length);

        context.DrawLine(pen, center, tip);

        var back = new Point(
            tip.X - Math.Cos(radians) * 10,
            tip.Y + Math.Sin(radians) * 10);
        var left = new Point(
            back.X + Math.Cos(radians + Math.PI / 2) * 4,
            back.Y - Math.Sin(radians + Math.PI / 2) * 4);
        var right = new Point(
            back.X + Math.Cos(radians - Math.PI / 2) * 4,
            back.Y - Math.Sin(radians - Math.PI / 2) * 4);
        context.DrawLine(pen, tip, left);
        context.DrawLine(pen, tip, right);

        var label = vector.Label;
        var formatted = new FormattedText(
            label,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            MonoTypeface,
            9,
            brush);
        var labelX = tip.X + (Math.Cos(radians) >= 0 ? 5 : -formatted.Width - 5);
        var labelY = tip.Y + (Math.Sin(radians) >= 0 ? -formatted.Height - 3 : 3);
        context.DrawText(formatted, new Point(labelX, labelY));
    }

    private static void DrawFooter(
        DrawingContext context,
        double width,
        double height,
        double footerHeight,
        PhasorDisplayFrame? frame)
    {
        var y = height - footerHeight;
        context.DrawLine(new Pen(MajorGridBrush, 1), new Point(0, y), new Point(width, y));

        var status = frame?.Status ?? "NO PHASOR";
        DrawText(context, status, new Point(8, y + 8), 8.5, LabelBrush);

        if (frame is not { IsAvailable: true })
            return;

        var maxText = frame.Mode == PhasorDisplayMode.Voltage
            ? $"MAX {frame.MaximumVoltage:0.###} V"
            : $"MAX {frame.MaximumCurrent:0.###} A";
        var formatted = new FormattedText(
            maxText,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            MonoTypeface,
            8.5,
            StrongLabelBrush);
        context.DrawText(formatted, new Point(Math.Max(8, width - formatted.Width - 8), y + 8));

        DrawText(context, frame.ReferenceLabel, new Point(8, y + 23), 8, LabelBrush);
    }

    private static IBrush ResolveBrush(string key) => key switch
    {
        "IA" or "VA" or "I1" or "V1" => PhaseABrush,
        "IB" or "VB" => PhaseBBrush,
        "IC" or "VC" => PhaseCBrush,
        "3I0" or "3V0" => ResidualBrush,
        _ => NeutralBrush
    };

    private static void DrawCenteredText(
        DrawingContext context,
        string text,
        Point center,
        double size,
        IBrush brush)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface,
            size,
            brush)
        {
            MaxTextWidth = 210,
            TextAlignment = TextAlignment.Center
        };
        context.DrawText(formatted, new Point(center.X - formatted.Width / 2, center.Y - formatted.Height / 2));
    }

    private static void DrawText(
        DrawingContext context,
        string text,
        Point point,
        double size,
        IBrush brush,
        FontWeight? weight = null)
    {
        var typeface = weight is null ? Typeface : new Typeface("Inter", FontStyle.Normal, weight.Value);
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            typeface,
            size,
            brush);
        context.DrawText(formatted, point);
    }
}
