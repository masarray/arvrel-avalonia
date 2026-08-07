using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Arvrel.Protection;

namespace Arvrel.Desktop.ViewModels;

public sealed class ProtectionSettingsEditorViewModel : INotifyPropertyChanged
{
    private string _groupName = "GROUP A";
    private string _revisionText = "1";

    public ProtectionSettingsEditorViewModel()
    {
        Phase50 = new InstantaneousStageEditorViewModel("50P", "Phase instantaneous");
        Phase51 = new TimeStageEditorViewModel("51P", "Phase inverse time");
        Earth50 = new InstantaneousStageEditorViewModel("50N", "Earth instantaneous");
        Earth51 = new TimeStageEditorViewModel("51N", "Earth inverse time");
        Undervoltage27 = new VoltageStageEditorViewModel("27", "Undervoltage", isUndervoltage: true);
        Overvoltage59 = new VoltageStageEditorViewModel("59", "Overvoltage", isUndervoltage: false);
        ResidualOvervoltage59N = new ResidualVoltageStageEditorViewModel("59N", "Residual overvoltage");
        DirectionalPhase67 = new DirectionalStageEditorViewModel("67P", "Directional phase overcurrent");
        DirectionalEarth67N = new DirectionalStageEditorViewModel("67N", "Directional earth overcurrent");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public InstantaneousStageEditorViewModel Phase50 { get; }
    public TimeStageEditorViewModel Phase51 { get; }
    public InstantaneousStageEditorViewModel Earth50 { get; }
    public TimeStageEditorViewModel Earth51 { get; }
    public VoltageStageEditorViewModel Undervoltage27 { get; }
    public VoltageStageEditorViewModel Overvoltage59 { get; }
    public ResidualVoltageStageEditorViewModel ResidualOvervoltage59N { get; }
    public DirectionalStageEditorViewModel DirectionalPhase67 { get; }
    public DirectionalStageEditorViewModel DirectionalEarth67N { get; }

    public IReadOnlyList<IecCurveFamily> CurveFamilies { get; } = Enum.GetValues<IecCurveFamily>();
    public IReadOnlyList<ProtectionResetMode> ResetModes { get; } = Enum.GetValues<ProtectionResetMode>();
    public IReadOnlyList<VoltageMeasurementMode> VoltageMeasurementModes { get; } = Enum.GetValues<VoltageMeasurementMode>();
    public IReadOnlyList<VoltageSelectionLogic> VoltageSelectionLogics { get; } = Enum.GetValues<VoltageSelectionLogic>();
    public IReadOnlyList<DirectionalSense> DirectionalSenses { get; } = Enum.GetValues<DirectionalSense>();

    public string GroupName
    {
        get => _groupName;
        set => SetField(ref _groupName, value ?? string.Empty);
    }

    public string RevisionText
    {
        get => _revisionText;
        set => SetField(ref _revisionText, value ?? string.Empty);
    }

    public void Apply(ProtectionSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _groupName = settings.GroupName;
        _revisionText = settings.Revision.ToString(CultureInfo.InvariantCulture);
        Phase50.Apply(
            settings.PhaseInstantaneousEnabled,
            settings.PhaseInstantaneousPickupA,
            settings.PhaseInstantaneousDelay,
            settings.PhaseInstantaneousDropoutRatio);
        Phase51.Apply(
            settings.PhaseTimeEnabled,
            settings.PhaseTimePickupA,
            settings.PhaseTimeCurve,
            settings.PhaseTimeMultiplier,
            settings.PhaseTimeDefiniteDelay,
            settings.PhaseTimeMinimumOperateTime,
            settings.PhaseTimeDropoutRatio,
            settings.PhaseTimeResetMode,
            settings.PhaseTimeResetDelay,
            settings.PhaseTimeUserK,
            settings.PhaseTimeUserAlpha,
            settings.PhaseTimeUserC);
        Earth50.Apply(
            settings.EarthInstantaneousEnabled,
            settings.EarthInstantaneousPickupA,
            settings.EarthInstantaneousDelay,
            settings.EarthInstantaneousDropoutRatio);
        Earth51.Apply(
            settings.EarthTimeEnabled,
            settings.EarthTimePickupA,
            settings.EarthTimeCurve,
            settings.EarthTimeMultiplier,
            settings.EarthTimeDefiniteDelay,
            settings.EarthTimeMinimumOperateTime,
            settings.EarthTimeDropoutRatio,
            settings.EarthTimeResetMode,
            settings.EarthTimeResetDelay,
            settings.EarthTimeUserK,
            settings.EarthTimeUserAlpha,
            settings.EarthTimeUserC);

        var feeder = settings.Feeder;
        Undervoltage27.Apply(
            feeder.Undervoltage27Enabled,
            feeder.Undervoltage27PickupV,
            feeder.Undervoltage27Delay,
            feeder.Undervoltage27ResetRatio,
            feeder.Undervoltage27Mode,
            feeder.Undervoltage27Logic);
        Overvoltage59.Apply(
            feeder.Overvoltage59Enabled,
            feeder.Overvoltage59PickupV,
            feeder.Overvoltage59Delay,
            feeder.Overvoltage59DropoutRatio,
            feeder.Overvoltage59Mode,
            feeder.Overvoltage59Logic);
        ResidualOvervoltage59N.Apply(
            feeder.ResidualOvervoltage59NEnabled,
            feeder.ResidualOvervoltage59NPickupV,
            feeder.ResidualOvervoltage59NDelay,
            feeder.ResidualOvervoltage59NDropoutRatio);
        DirectionalPhase67.Apply(
            feeder.DirectionalPhase67Enabled,
            feeder.DirectionalPhase67PickupA,
            feeder.DirectionalPhase67Delay,
            feeder.DirectionalPhase67DropoutRatio,
            feeder.DirectionalPhase67CharacteristicAngleDeg,
            feeder.DirectionalPhase67MinimumPolarizingVoltageV,
            feeder.DirectionalPhase67Sense);
        DirectionalEarth67N.Apply(
            feeder.DirectionalEarth67NEnabled,
            feeder.DirectionalEarth67NPickupA,
            feeder.DirectionalEarth67NDelay,
            feeder.DirectionalEarth67NDropoutRatio,
            feeder.DirectionalEarth67NCharacteristicAngleDeg,
            feeder.DirectionalEarth67NMinimumPolarizingVoltageV,
            feeder.DirectionalEarth67NSense);
        OnPropertyChanged(string.Empty);
    }

    public bool TryBuild(
        ProtectionSettings current,
        out ProtectionSettings settings,
        out string error)
    {
        ArgumentNullException.ThrowIfNull(current);
        settings = current;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(GroupName))
        {
            error = "Setting group name is required.";
            return false;
        }

        if (!int.TryParse(RevisionText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var revision) || revision < 1)
        {
            error = "Revision must be an integer greater than or equal to one.";
            return false;
        }

        if (!Phase50.TryRead(out var phase50, out error) ||
            !Phase51.TryRead(out var phase51, out error) ||
            !Earth50.TryRead(out var earth50, out error) ||
            !Earth51.TryRead(out var earth51, out error) ||
            !Undervoltage27.TryRead(out var undervoltage27, out error) ||
            !Overvoltage59.TryRead(out var overvoltage59, out error) ||
            !ResidualOvervoltage59N.TryRead(out var residualOvervoltage59N, out error) ||
            !DirectionalPhase67.TryRead(out var directionalPhase67, out error) ||
            !DirectionalEarth67N.TryRead(out var directionalEarth67N, out error))
        {
            return false;
        }

        try
        {
            settings = current with
            {
                GroupName = GroupName.Trim(),
                Revision = revision,
                PhaseInstantaneousEnabled = phase50.Enabled,
                PhaseInstantaneousPickupA = phase50.Pickup,
                PhaseInstantaneousDelay = phase50.Delay,
                PhaseInstantaneousDropoutRatio = phase50.Dropout,
                PhaseTimeEnabled = phase51.Enabled,
                PhaseTimePickupA = phase51.Pickup,
                PhaseTimeCurve = phase51.Curve,
                PhaseTimeMultiplier = phase51.Multiplier,
                PhaseTimeDefiniteDelay = phase51.DefiniteDelay,
                PhaseTimeMinimumOperateTime = phase51.MinimumOperateTime,
                PhaseTimeDropoutRatio = phase51.Dropout,
                PhaseTimeResetMode = phase51.ResetMode,
                PhaseTimeResetDelay = phase51.ResetDelay,
                PhaseTimeUserK = phase51.UserK,
                PhaseTimeUserAlpha = phase51.UserAlpha,
                PhaseTimeUserC = phase51.UserC,
                EarthInstantaneousEnabled = earth50.Enabled,
                EarthInstantaneousPickupA = earth50.Pickup,
                EarthInstantaneousDelay = earth50.Delay,
                EarthInstantaneousDropoutRatio = earth50.Dropout,
                EarthTimeEnabled = earth51.Enabled,
                EarthTimePickupA = earth51.Pickup,
                EarthTimeCurve = earth51.Curve,
                EarthTimeMultiplier = earth51.Multiplier,
                EarthTimeDefiniteDelay = earth51.DefiniteDelay,
                EarthTimeMinimumOperateTime = earth51.MinimumOperateTime,
                EarthTimeDropoutRatio = earth51.Dropout,
                EarthTimeResetMode = earth51.ResetMode,
                EarthTimeResetDelay = earth51.ResetDelay,
                EarthTimeUserK = earth51.UserK,
                EarthTimeUserAlpha = earth51.UserAlpha,
                EarthTimeUserC = earth51.UserC,
                Feeder = current.Feeder with
                {
                    Undervoltage27Enabled = undervoltage27.Enabled,
                    Undervoltage27PickupV = undervoltage27.Pickup,
                    Undervoltage27Delay = undervoltage27.Delay,
                    Undervoltage27ResetRatio = undervoltage27.Ratio,
                    Undervoltage27Mode = undervoltage27.Mode,
                    Undervoltage27Logic = undervoltage27.Logic,
                    Overvoltage59Enabled = overvoltage59.Enabled,
                    Overvoltage59PickupV = overvoltage59.Pickup,
                    Overvoltage59Delay = overvoltage59.Delay,
                    Overvoltage59DropoutRatio = overvoltage59.Ratio,
                    Overvoltage59Mode = overvoltage59.Mode,
                    Overvoltage59Logic = overvoltage59.Logic,
                    ResidualOvervoltage59NEnabled = residualOvervoltage59N.Enabled,
                    ResidualOvervoltage59NPickupV = residualOvervoltage59N.Pickup,
                    ResidualOvervoltage59NDelay = residualOvervoltage59N.Delay,
                    ResidualOvervoltage59NDropoutRatio = residualOvervoltage59N.Dropout,
                    DirectionalPhase67Enabled = directionalPhase67.Enabled,
                    DirectionalPhase67PickupA = directionalPhase67.Pickup,
                    DirectionalPhase67Delay = directionalPhase67.Delay,
                    DirectionalPhase67DropoutRatio = directionalPhase67.Dropout,
                    DirectionalPhase67CharacteristicAngleDeg = directionalPhase67.CharacteristicAngle,
                    DirectionalPhase67MinimumPolarizingVoltageV = directionalPhase67.MinimumPolarizingVoltage,
                    DirectionalPhase67Sense = directionalPhase67.Sense,
                    DirectionalEarth67NEnabled = directionalEarth67N.Enabled,
                    DirectionalEarth67NPickupA = directionalEarth67N.Pickup,
                    DirectionalEarth67NDelay = directionalEarth67N.Delay,
                    DirectionalEarth67NDropoutRatio = directionalEarth67N.Dropout,
                    DirectionalEarth67NCharacteristicAngleDeg = directionalEarth67N.CharacteristicAngle,
                    DirectionalEarth67NMinimumPolarizingVoltageV = directionalEarth67N.MinimumPolarizingVoltage,
                    DirectionalEarth67NSense = directionalEarth67N.Sense
                }
            };
            settings.Validate();
            return true;
        }
        catch (ArgumentException ex)
        {
            error = ex.Message;
            settings = current;
            return false;
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public sealed class InstantaneousStageEditorViewModel : INotifyPropertyChanged
{
    private bool _enabled;
    private string _pickupText = "1";
    private string _delayMsText = "0";
    private string _dropoutText = "0.95";

    public InstantaneousStageEditorViewModel(string code, string label)
    {
        Code = code;
        Label = label;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Code { get; }
    public string Label { get; }

    public bool Enabled { get => _enabled; set => SetField(ref _enabled, value); }
    public string PickupText { get => _pickupText; set => SetField(ref _pickupText, value ?? string.Empty); }
    public string DelayMsText { get => _delayMsText; set => SetField(ref _delayMsText, value ?? string.Empty); }
    public string DropoutText { get => _dropoutText; set => SetField(ref _dropoutText, value ?? string.Empty); }

    public void Apply(bool enabled, double pickup, TimeSpan delay, double dropout)
    {
        _enabled = enabled;
        _pickupText = Format(pickup);
        _delayMsText = Format(delay.TotalMilliseconds);
        _dropoutText = Format(dropout);
        OnPropertyChanged(string.Empty);
    }

    public bool TryRead(out InstantaneousStageDraft draft, out string error)
    {
        draft = default;
        if (!TryPositive(PickupText, $"{Code} pickup", out var pickup, out error) ||
            !TryNonNegative(DelayMsText, $"{Code} delay", out var delayMs, out error) ||
            !TryRatio(DropoutText, $"{Code} dropout", out var dropout, out error))
        {
            return false;
        }

        draft = new InstantaneousStageDraft(Enabled, pickup, TimeSpan.FromMilliseconds(delayMs), dropout);
        return true;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    internal static bool TryPositive(string text, string name, out double value, out string error)
    {
        if (!TryNumber(text, out value) || value <= 0)
        {
            error = $"{name} must be a finite positive number.";
            return false;
        }
        error = string.Empty;
        return true;
    }

    internal static bool TryNonNegative(string text, string name, out double value, out string error)
    {
        if (!TryNumber(text, out value) || value < 0)
        {
            error = $"{name} must be a finite non-negative number.";
            return false;
        }
        error = string.Empty;
        return true;
    }

    internal static bool TryRatio(string text, string name, out double value, out string error)
    {
        if (!TryNumber(text, out value) || value is <= 0 or > 1)
        {
            error = $"{name} must be greater than zero and no greater than one.";
            return false;
        }
        error = string.Empty;
        return true;
    }

    internal static bool TryResetRatio(string text, string name, out double value, out string error)
    {
        if (!TryNumber(text, out value) || value is < 1 or > 2)
        {
            error = $"{name} must be between one and two.";
            return false;
        }
        error = string.Empty;
        return true;
    }

    internal static bool TryAngle(string text, string name, out double value, out string error)
    {
        if (!TryNumber(text, out value) || value is < -180 or > 180)
        {
            error = $"{name} must be between -180 and 180 degrees.";
            return false;
        }
        error = string.Empty;
        return true;
    }

    internal static string Format(double value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);

    internal static bool TryNumber(string text, out double value)
        => InjectionChannelViewModel.TryParseEngineeringDouble(text, out value) && double.IsFinite(value);
}

public sealed class TimeStageEditorViewModel : INotifyPropertyChanged
{
    private bool _enabled;
    private string _pickupText = "1";
    private IecCurveFamily _curve;
    private string _multiplierText = "0.1";
    private string _definiteDelayMsText = "500";
    private string _minimumOperateMsText = "20";
    private string _dropoutText = "0.95";
    private ProtectionResetMode _resetMode;
    private string _resetDelayMsText = "1000";
    private string _userKText = "0.14";
    private string _userAlphaText = "0.02";
    private string _userCText = "0";

    public TimeStageEditorViewModel(string code, string label)
    {
        Code = code;
        Label = label;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Code { get; }
    public string Label { get; }

    public bool Enabled { get => _enabled; set => SetField(ref _enabled, value); }
    public string PickupText { get => _pickupText; set => SetField(ref _pickupText, value ?? string.Empty); }
    public IecCurveFamily Curve
    {
        get => _curve;
        set
        {
            if (!SetField(ref _curve, value))
                return;
            OnPropertyChanged(nameof(IsUserDefined));
            OnPropertyChanged(nameof(CurveFormula));
        }
    }
    public string MultiplierText { get => _multiplierText; set => SetField(ref _multiplierText, value ?? string.Empty); }
    public string DefiniteDelayMsText { get => _definiteDelayMsText; set => SetField(ref _definiteDelayMsText, value ?? string.Empty); }
    public string MinimumOperateMsText { get => _minimumOperateMsText; set => SetField(ref _minimumOperateMsText, value ?? string.Empty); }
    public string DropoutText { get => _dropoutText; set => SetField(ref _dropoutText, value ?? string.Empty); }
    public ProtectionResetMode ResetMode { get => _resetMode; set => SetField(ref _resetMode, value); }
    public string ResetDelayMsText { get => _resetDelayMsText; set => SetField(ref _resetDelayMsText, value ?? string.Empty); }
    public string UserKText
    {
        get => _userKText;
        set
        {
            if (SetField(ref _userKText, value ?? string.Empty))
                OnPropertyChanged(nameof(CurveFormula));
        }
    }
    public string UserAlphaText
    {
        get => _userAlphaText;
        set
        {
            if (SetField(ref _userAlphaText, value ?? string.Empty))
                OnPropertyChanged(nameof(CurveFormula));
        }
    }
    public string UserCText
    {
        get => _userCText;
        set
        {
            if (SetField(ref _userCText, value ?? string.Empty))
                OnPropertyChanged(nameof(CurveFormula));
        }
    }

    public bool IsUserDefined => Curve == IecCurveFamily.UserDefined;
    public string CurveFormula
    {
        get
        {
            var k = InstantaneousStageEditorViewModel.TryNumber(UserKText, out var parsedK) ? parsedK : 0.14;
            var alpha = InstantaneousStageEditorViewModel.TryNumber(UserAlphaText, out var parsedAlpha) ? parsedAlpha : 0.02;
            var c = InstantaneousStageEditorViewModel.TryNumber(UserCText, out var parsedC) ? parsedC : 0;
            return IecCurveCalculator.Formula(Curve, k, alpha, c);
        }
    }

    public void Apply(
        bool enabled,
        double pickup,
        IecCurveFamily curve,
        double multiplier,
        TimeSpan definiteDelay,
        TimeSpan minimumOperateTime,
        double dropout,
        ProtectionResetMode resetMode,
        TimeSpan resetDelay,
        double userK,
        double userAlpha,
        double userC)
    {
        _enabled = enabled;
        _pickupText = InstantaneousStageEditorViewModel.Format(pickup);
        _curve = curve;
        _multiplierText = InstantaneousStageEditorViewModel.Format(multiplier);
        _definiteDelayMsText = InstantaneousStageEditorViewModel.Format(definiteDelay.TotalMilliseconds);
        _minimumOperateMsText = InstantaneousStageEditorViewModel.Format(minimumOperateTime.TotalMilliseconds);
        _dropoutText = InstantaneousStageEditorViewModel.Format(dropout);
        _resetMode = resetMode;
        _resetDelayMsText = InstantaneousStageEditorViewModel.Format(resetDelay.TotalMilliseconds);
        _userKText = InstantaneousStageEditorViewModel.Format(userK);
        _userAlphaText = InstantaneousStageEditorViewModel.Format(userAlpha);
        _userCText = InstantaneousStageEditorViewModel.Format(userC);
        OnPropertyChanged(string.Empty);
    }

    public bool TryRead(out TimeStageDraft draft, out string error)
    {
        draft = default;
        if (!InstantaneousStageEditorViewModel.TryPositive(PickupText, $"{Code} pickup", out var pickup, out error) ||
            !InstantaneousStageEditorViewModel.TryPositive(MultiplierText, $"{Code} TMS", out var multiplier, out error) ||
            !InstantaneousStageEditorViewModel.TryPositive(DefiniteDelayMsText, $"{Code} definite delay", out var definiteDelayMs, out error) ||
            !InstantaneousStageEditorViewModel.TryNonNegative(MinimumOperateMsText, $"{Code} minimum operate time", out var minimumOperateMs, out error) ||
            !InstantaneousStageEditorViewModel.TryRatio(DropoutText, $"{Code} dropout", out var dropout, out error) ||
            !InstantaneousStageEditorViewModel.TryPositive(ResetDelayMsText, $"{Code} reset delay", out var resetDelayMs, out error) ||
            !InstantaneousStageEditorViewModel.TryPositive(UserKText, $"{Code} user curve K", out var userK, out error) ||
            !InstantaneousStageEditorViewModel.TryPositive(UserAlphaText, $"{Code} user curve alpha", out var userAlpha, out error) ||
            !InstantaneousStageEditorViewModel.TryNonNegative(UserCText, $"{Code} user curve C", out var userC, out error))
        {
            return false;
        }

        draft = new TimeStageDraft(
            Enabled,
            pickup,
            Curve,
            multiplier,
            TimeSpan.FromMilliseconds(definiteDelayMs),
            TimeSpan.FromMilliseconds(minimumOperateMs),
            dropout,
            ResetMode,
            TimeSpan.FromMilliseconds(resetDelayMs),
            userK,
            userAlpha,
            userC);
        return true;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public sealed class VoltageStageEditorViewModel : INotifyPropertyChanged
{
    private readonly bool _isUndervoltage;
    private bool _enabled;
    private string _pickupText = "100";
    private string _delayMsText = "1000";
    private string _ratioText = "0.95";
    private VoltageMeasurementMode _mode;
    private VoltageSelectionLogic _logic;

    public VoltageStageEditorViewModel(string code, string label, bool isUndervoltage)
    {
        Code = code;
        Label = label;
        _isUndervoltage = isUndervoltage;
        _ratioText = isUndervoltage ? "1.05" : "0.95";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Code { get; }
    public string Label { get; }
    public string RatioLabel => _isUndervoltage ? "RESET RATIO" : "DROPOUT";

    public bool Enabled { get => _enabled; set => SetField(ref _enabled, value); }
    public string PickupText { get => _pickupText; set => SetField(ref _pickupText, value ?? string.Empty); }
    public string DelayMsText { get => _delayMsText; set => SetField(ref _delayMsText, value ?? string.Empty); }
    public string RatioText { get => _ratioText; set => SetField(ref _ratioText, value ?? string.Empty); }
    public VoltageMeasurementMode Mode { get => _mode; set => SetField(ref _mode, value); }
    public VoltageSelectionLogic Logic { get => _logic; set => SetField(ref _logic, value); }

    public void Apply(
        bool enabled,
        double pickup,
        TimeSpan delay,
        double ratio,
        VoltageMeasurementMode mode,
        VoltageSelectionLogic logic)
    {
        _enabled = enabled;
        _pickupText = InstantaneousStageEditorViewModel.Format(pickup);
        _delayMsText = InstantaneousStageEditorViewModel.Format(delay.TotalMilliseconds);
        _ratioText = InstantaneousStageEditorViewModel.Format(ratio);
        _mode = mode;
        _logic = logic;
        OnPropertyChanged(string.Empty);
    }

    public bool TryRead(out VoltageStageDraft draft, out string error)
    {
        draft = default;
        if (!InstantaneousStageEditorViewModel.TryPositive(PickupText, $"{Code} pickup", out var pickup, out error) ||
            !InstantaneousStageEditorViewModel.TryNonNegative(DelayMsText, $"{Code} delay", out var delayMs, out error))
        {
            return false;
        }

        double ratio;
        if (_isUndervoltage)
        {
            if (!InstantaneousStageEditorViewModel.TryResetRatio(RatioText, $"{Code} reset ratio", out ratio, out error))
                return false;
        }
        else if (!InstantaneousStageEditorViewModel.TryRatio(RatioText, $"{Code} dropout", out ratio, out error))
        {
            return false;
        }

        draft = new VoltageStageDraft(
            Enabled,
            pickup,
            TimeSpan.FromMilliseconds(delayMs),
            ratio,
            Mode,
            Logic);
        return true;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public sealed class ResidualVoltageStageEditorViewModel : INotifyPropertyChanged
{
    private bool _enabled;
    private string _pickupText = "10";
    private string _delayMsText = "500";
    private string _dropoutText = "0.95";

    public ResidualVoltageStageEditorViewModel(string code, string label)
    {
        Code = code;
        Label = label;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Code { get; }
    public string Label { get; }
    public bool Enabled { get => _enabled; set => SetField(ref _enabled, value); }
    public string PickupText { get => _pickupText; set => SetField(ref _pickupText, value ?? string.Empty); }
    public string DelayMsText { get => _delayMsText; set => SetField(ref _delayMsText, value ?? string.Empty); }
    public string DropoutText { get => _dropoutText; set => SetField(ref _dropoutText, value ?? string.Empty); }

    public void Apply(bool enabled, double pickup, TimeSpan delay, double dropout)
    {
        _enabled = enabled;
        _pickupText = InstantaneousStageEditorViewModel.Format(pickup);
        _delayMsText = InstantaneousStageEditorViewModel.Format(delay.TotalMilliseconds);
        _dropoutText = InstantaneousStageEditorViewModel.Format(dropout);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    public bool TryRead(out ResidualVoltageStageDraft draft, out string error)
    {
        draft = default;
        if (!InstantaneousStageEditorViewModel.TryPositive(PickupText, $"{Code} pickup", out var pickup, out error) ||
            !InstantaneousStageEditorViewModel.TryNonNegative(DelayMsText, $"{Code} delay", out var delayMs, out error) ||
            !InstantaneousStageEditorViewModel.TryRatio(DropoutText, $"{Code} dropout", out var dropout, out error))
        {
            return false;
        }

        draft = new ResidualVoltageStageDraft(Enabled, pickup, TimeSpan.FromMilliseconds(delayMs), dropout);
        return true;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

public sealed class DirectionalStageEditorViewModel : INotifyPropertyChanged
{
    private bool _enabled;
    private string _pickupText = "1";
    private string _delayMsText = "300";
    private string _dropoutText = "0.95";
    private string _characteristicAngleText = "45";
    private string _minimumPolarizingVoltageText = "5";
    private DirectionalSense _sense;

    public DirectionalStageEditorViewModel(string code, string label)
    {
        Code = code;
        Label = label;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Code { get; }
    public string Label { get; }
    public bool Enabled { get => _enabled; set => SetField(ref _enabled, value); }
    public string PickupText { get => _pickupText; set => SetField(ref _pickupText, value ?? string.Empty); }
    public string DelayMsText { get => _delayMsText; set => SetField(ref _delayMsText, value ?? string.Empty); }
    public string DropoutText { get => _dropoutText; set => SetField(ref _dropoutText, value ?? string.Empty); }
    public string CharacteristicAngleText { get => _characteristicAngleText; set => SetField(ref _characteristicAngleText, value ?? string.Empty); }
    public string MinimumPolarizingVoltageText { get => _minimumPolarizingVoltageText; set => SetField(ref _minimumPolarizingVoltageText, value ?? string.Empty); }
    public DirectionalSense Sense { get => _sense; set => SetField(ref _sense, value); }

    public void Apply(
        bool enabled,
        double pickup,
        TimeSpan delay,
        double dropout,
        double characteristicAngle,
        double minimumPolarizingVoltage,
        DirectionalSense sense)
    {
        _enabled = enabled;
        _pickupText = InstantaneousStageEditorViewModel.Format(pickup);
        _delayMsText = InstantaneousStageEditorViewModel.Format(delay.TotalMilliseconds);
        _dropoutText = InstantaneousStageEditorViewModel.Format(dropout);
        _characteristicAngleText = InstantaneousStageEditorViewModel.Format(characteristicAngle);
        _minimumPolarizingVoltageText = InstantaneousStageEditorViewModel.Format(minimumPolarizingVoltage);
        _sense = sense;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }

    public bool TryRead(out DirectionalStageDraft draft, out string error)
    {
        draft = default;
        if (!InstantaneousStageEditorViewModel.TryPositive(PickupText, $"{Code} pickup", out var pickup, out error) ||
            !InstantaneousStageEditorViewModel.TryNonNegative(DelayMsText, $"{Code} delay", out var delayMs, out error) ||
            !InstantaneousStageEditorViewModel.TryRatio(DropoutText, $"{Code} dropout", out var dropout, out error) ||
            !InstantaneousStageEditorViewModel.TryAngle(CharacteristicAngleText, $"{Code} characteristic angle", out var angle, out error) ||
            !InstantaneousStageEditorViewModel.TryPositive(MinimumPolarizingVoltageText, $"{Code} minimum polarizing voltage", out var minimumPolarizingVoltage, out error))
        {
            return false;
        }

        draft = new DirectionalStageDraft(
            Enabled,
            pickup,
            TimeSpan.FromMilliseconds(delayMs),
            dropout,
            angle,
            minimumPolarizingVoltage,
            Sense);
        return true;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}

public readonly record struct InstantaneousStageDraft(
    bool Enabled,
    double Pickup,
    TimeSpan Delay,
    double Dropout);

public readonly record struct TimeStageDraft(
    bool Enabled,
    double Pickup,
    IecCurveFamily Curve,
    double Multiplier,
    TimeSpan DefiniteDelay,
    TimeSpan MinimumOperateTime,
    double Dropout,
    ProtectionResetMode ResetMode,
    TimeSpan ResetDelay,
    double UserK,
    double UserAlpha,
    double UserC);

public readonly record struct VoltageStageDraft(
    bool Enabled,
    double Pickup,
    TimeSpan Delay,
    double Ratio,
    VoltageMeasurementMode Mode,
    VoltageSelectionLogic Logic);

public readonly record struct ResidualVoltageStageDraft(
    bool Enabled,
    double Pickup,
    TimeSpan Delay,
    double Dropout);

public readonly record struct DirectionalStageDraft(
    bool Enabled,
    double Pickup,
    TimeSpan Delay,
    double Dropout,
    double CharacteristicAngle,
    double MinimumPolarizingVoltage,
    DirectionalSense Sense);
