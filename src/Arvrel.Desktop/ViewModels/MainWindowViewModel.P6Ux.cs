using Arvrel.Protection;

namespace Arvrel.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    public string LabStatusText => AllowsTrip ? "LAB READY" : "TRIP BLOCKED";

    public string EngineStatusText =>
        $"{ShellVersion} · ARIEC61850 SIBLING READY · P6 UX";

    public string StreamRunStateText => IsProcessBusDisplayActive
        ? ProcessBusRunning ? "RUNNING" : "READY"
        : IsRunning ? "RUNNING" : "STOPPED";

    public string OperationHeaderText => TripLatched
        ? $"· TRIP LATCHED · {DisplayActiveElement}"
        : PickupActive
            ? $"· PICKUP · {DisplayActiveElement}"
            : "· Measurements stable · no pickup";

    public string OperationBadgeText => TripLatched
        ? "TRIP LATCHED"
        : PickupActive
            ? "PICKUP ACTIVE"
            : AllowsTrip
                ? "TRIP PERMITTED"
                : "TRIP BLOCKED";

    public ProtectionElementViewModel PrimaryProtectionElement
    {
        get
        {
            var active = ProtectionElements.FirstOrDefault(element =>
                !string.IsNullOrWhiteSpace(DisplayActiveElement) &&
                DisplayActiveElement.StartsWith(element.Code, StringComparison.OrdinalIgnoreCase));
            return active ?? ProtectionElements
                .OrderByDescending(element => element.Progress)
                .First();
        }
    }

    public IReadOnlyList<ProtectionElementViewModel> SecondaryProtectionElements =>
        ProtectionElements
            .Where(element => !ReferenceEquals(element, PrimaryProtectionElement))
            .ToArray();

    public string FooterStatusText =>
        $"{SettingsGroupText} · {ActiveDisplaySourceText} · VIRTUAL OUTPUT · NO GOOSE · NO PHYSICAL TRIP";

    public PhasorDisplayFrame PhasorFrame =>
        PhasorDisplayProjector.Project(DisplayMeasurement.Phasors, PhasorDisplayMode.Current);
}
