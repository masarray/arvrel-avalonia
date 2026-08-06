using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Arvrel.Application.Laboratory;

namespace Arvrel.Desktop.Controls;

public sealed class WaveformScope : Control
{
    public static readonly StyledProperty<ScenarioWaveform?> WaveformProperty =
        AvaloniaProperty.Register<WaveformScope, ScenarioWaveform?>(nameof(Waveform));

    private static readonly IBrush TickLabelBrush = new SolidColorBrush(Color.Parse("#8FA7B2"));
    private static readonly IBrush StrongLabelBrush = new SolidColorBrush(Color.Parse("#B9C7CE"));
    private static readonly Pen MinorGridPen = new(new SolidColorBrush(Color.Parse("#192A33")), 1);
    private static readonly Pen MajorGridPen = new(new SolidColorBrush(Color.Parse("#29414D")), 1);
    private static readonly Pen CycleGridPen = new(new SolidColorBrush(Color.Parse("#3C5D6B")), 1.2);
    private static readonly Pen ZeroLinePen = new(new SolidColorBrush(Color.Parse("#4A6571")), 1.1);
    private static readonly Pen AxisPen = new(new SolidColorBrush(Color.Parse("#55717D")), 1);
    private static readonly Pen PhaseAPen = new(new SolidColorBrush(Color.Parse("#F04F55")), 2.0);
    private static readonly Pen PhaseBPen = new(new SolidColorBrush(Color.Parse("#F2C84B")), 1.9);
    private static readonly Pen PhaseCPen = new(new SolidColorBrush(Color.Parse("#4B91EA")), 1.9);
    private static readonly Pen ResidualPen = new(new SolidColorBrush(Color.Parse("#42CC82")), 1.9);
    private static readonly Typeface TickTypeface = new("Inter");
    private static readonly Typeface MonoTypeface = new("Cascadia Mono,Consolas,monospace");

    static WaveformScope()
        => AffectsRender<WaveformScope>(WaveformProperty);

    public WaveformScope()
        => ClipToBounds = true;

    public ScenarioWaveform? Waveform
    {
        get => GetValue(WaveformProperty);
        set => SetValue(WaveformProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var width = Bounds.Width;
        var height = Bounds.Height;
        var viewport = WaveformAxisLayout.CreateViewport(width, height);
        if (!viewport.IsRenderable)
            return;

        DrawGrid(context, viewport);
        if (Waveform is not { } waveform)
        {
            DrawAxisBaseline(context, viewport);
            DrawCenteredMessage(context, viewport, "Waiting for a coherent waveform window.");
            return;
        }

        var axis = WaveformAxisLayout.Create(waveform);
        DrawTimeAxis(context, axis, viewport, width);

        var maximum = waveform.PhaseA
            .Concat(waveform.PhaseB)
            .Concat(waveform.PhaseC)
            .Concat(waveform.Residual)
            .Select(Math.Abs)
            .DefaultIfEmpty(1)
            .Max();
        maximum = Math.Max(maximum, 1);

        DrawAmplitudeLabels(context, viewport, maximum);
        DrawSeries(context, waveform.PhaseA, maximum, viewport, PhaseAPen);
        DrawSeries(context, waveform.PhaseB, maximum, viewport, PhaseBPen);
        DrawSeries(context, waveform.PhaseC, maximum, viewport, PhaseCPen);
        DrawSeries(context, waveform.Residual, maximum, viewport, ResidualPen);
    }

    private static void DrawGrid(DrawingContext context, WaveformViewport viewport)
    {
        for (var row = 0; row <= 8; row++)
        {
            var y = viewport.Top + viewport.Height * row / 8d;
            var pen = row == 4 ? ZeroLinePen : row % 2 == 0 ? MajorGridPen : MinorGridPen;
            context.DrawLine(pen, new Point(viewport.Left, y), new Point(viewport.Right, y));
        }
    }

    private static void DrawTimeAxis(
        DrawingContext context,
        WaveformAxis axis,
        WaveformViewport viewport,
        double controlWidth)
    {
        foreach (var tick in axis.Ticks)
        {
            var x = viewport.Left + viewport.Width * tick.NormalizedPosition;
            var gridPen = tick.IsCycleBoundary
                ? CycleGridPen
                : tick.IsMajor
                    ? MajorGridPen
                    : MinorGridPen;

            context.DrawLine(
                gridPen,
                new Point(x, viewport.Top),
                new Point(x, viewport.Bottom));

            var tickLength = tick.IsCycleBoundary ? 7 : tick.IsMajor ? 5 : 3;
            context.DrawLine(
                AxisPen,
                new Point(x, viewport.AxisY),
                new Point(x, viewport.AxisY + tickLength));

            if (tick.Label is null)
                continue;

            var formatted = new FormattedText(
                tick.Label,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                TickTypeface,
                10,
                TickLabelBrush);
            var labelX = Math.Clamp(x - formatted.Width / 2d, 1, Math.Max(1, controlWidth - formatted.Width - 1));
            context.DrawText(formatted, new Point(labelX, viewport.AxisY + 8));
        }

        DrawAxisBaseline(context, viewport);
    }

    private static void DrawAxisBaseline(DrawingContext context, WaveformViewport viewport)
        => context.DrawLine(
            AxisPen,
            new Point(viewport.Left, viewport.AxisY),
            new Point(viewport.Right, viewport.AxisY));

    private static void DrawAmplitudeLabels(
        DrawingContext context,
        WaveformViewport viewport,
        double maximum)
    {
        DrawEngineeringText(context, $"+{maximum:0.##} A", new Point(4, viewport.Top + 3));
        DrawEngineeringText(context, "0", new Point(18, viewport.Top + viewport.Height / 2d - 6));
        DrawEngineeringText(context, $"-{maximum:0.##} A", new Point(4, viewport.Bottom - 14));
    }

    private static void DrawSeries(
        DrawingContext context,
        IReadOnlyList<double> samples,
        double maximum,
        WaveformViewport viewport,
        Pen pen)
    {
        if (samples.Count < 2)
            return;

        var center = viewport.Top + viewport.Height / 2d;
        var scale = viewport.Height * 0.43 / maximum;
        var previous = new Point(viewport.Left, center - samples[0] * scale);

        for (var index = 1; index < samples.Count; index++)
        {
            var current = new Point(
                viewport.Left + viewport.Width * index / (samples.Count - 1d),
                center - samples[index] * scale);
            context.DrawLine(pen, previous, current);
            previous = current;
        }
    }

    private static void DrawCenteredMessage(
        DrawingContext context,
        WaveformViewport viewport,
        string message)
    {
        var formatted = new FormattedText(
            message,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            TickTypeface,
            11,
            TickLabelBrush)
        {
            MaxTextWidth = viewport.Width * 0.8,
            TextAlignment = TextAlignment.Center
        };
        context.DrawText(
            formatted,
            new Point(
                viewport.Left + (viewport.Width - formatted.Width) / 2d,
                viewport.Top + (viewport.Height - formatted.Height) / 2d));
    }

    private static void DrawEngineeringText(DrawingContext context, string text, Point point)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            MonoTypeface,
            9,
            StrongLabelBrush);
        context.DrawText(formatted, point);
    }
}
