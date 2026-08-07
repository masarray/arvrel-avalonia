using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Arvrel.Application.Settings;
using Arvrel.Desktop.Infrastructure;
using Arvrel.Protection;

namespace Arvrel.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    private readonly ProtectionSettingGroupStore _settingGroupStore;
    private readonly Dictionary<string, ProtectionSettings> _persistedSettingGroups = new(StringComparer.OrdinalIgnoreCase);
    private string? _selectedPersistedSettingGroup;
    private string _settingGroupPersistenceStatus = "READY · no persisted setting group selected";
    private RelayCommand? _saveSettingGroupCommand;
    private RelayCommand? _loadSettingGroupCommand;
    private RelayCommand? _deleteSettingGroupCommand;

    public ObservableCollection<string> PersistedSettingGroupNames { get; } = new();

    public string? SelectedPersistedSettingGroup
    {
        get => _selectedPersistedSettingGroup;
        set
        {
            value = string.IsNullOrWhiteSpace(value) ? null : value;
            if (string.Equals(_selectedPersistedSettingGroup, value, StringComparison.Ordinal))
                return;
            _selectedPersistedSettingGroup = value;
            OnPropertyChanged();
        }
    }

    public string SettingGroupPersistenceStatus => _settingGroupPersistenceStatus;
    public string SettingGroupStorePath => _settingGroupStore.FilePath;

    public ICommand SaveSettingGroupCommand =>
        _saveSettingGroupCommand ??= new RelayCommand(SaveCurrentSettingGroup);

    public ICommand LoadSettingGroupCommand =>
        _loadSettingGroupCommand ??= new RelayCommand(LoadSelectedSettingGroup);

    public ICommand DeleteSettingGroupCommand =>
        _deleteSettingGroupCommand ??= new RelayCommand(DeleteSelectedSettingGroup);

    private void SubscribeProtectionSettingsEditors()
    {
        SettingsEditor.PropertyChanged += SettingsEditor_PropertyChanged;
        foreach (var editor in EnumerateProtectionSettingEditors())
            editor.PropertyChanged += SettingsEditor_PropertyChanged;
    }

    private IEnumerable<INotifyPropertyChanged> EnumerateProtectionSettingEditors()
    {
        yield return SettingsEditor.Phase50;
        yield return SettingsEditor.Phase51;
        yield return SettingsEditor.Earth50;
        yield return SettingsEditor.Earth51;
        yield return SettingsEditor.Undervoltage27;
        yield return SettingsEditor.Overvoltage59;
        yield return SettingsEditor.ResidualOvervoltage59N;
        yield return SettingsEditor.DirectionalPhase67;
        yield return SettingsEditor.DirectionalEarth67N;
    }

    private void RestorePersistedSettingGroups()
    {
        try
        {
            var catalog = _settingGroupStore.Load();
            ReplacePersistedSettingGroups(catalog.Groups);
            SelectedPersistedSettingGroup = catalog.ActiveGroupName ?? PersistedSettingGroupNames.FirstOrDefault();

            if (catalog.ActiveGroupName is not null &&
                _persistedSettingGroups.TryGetValue(catalog.ActiveGroupName, out var active))
            {
                ApplyValidatedProtectionSettings(active, "RESTORED", addEvent: true);
                _settingGroupPersistenceStatus = $"RESTORED · {active.GroupName} revision {active.Revision}";
            }
            else
            {
                _settingGroupPersistenceStatus = catalog.Groups.Count == 0
                    ? "READY · no persisted setting groups"
                    : $"READY · {catalog.Groups.Count} persisted group(s) available";
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            _settingGroupPersistenceStatus = $"LOAD FAILED · {ex.Message}";
            AddEvent("SET STORE", _settingGroupPersistenceStatus);
        }

        OnPropertyChanged(nameof(SettingGroupPersistenceStatus));
        OnPropertyChanged(nameof(SettingGroupStorePath));
    }

    private void SaveCurrentSettingGroup()
    {
        if (!TryBuildProtectionSettings(out var settings))
            return;

        ApplyValidatedProtectionSettings(settings, "APPLIED", addEvent: true);
        _persistedSettingGroups[settings.GroupName] = settings;
        RefreshPersistedSettingGroupNames(settings.GroupName);

        if (!TryPersistSettingGroups(settings.GroupName))
            return;

        _settingGroupPersistenceStatus = $"SAVED · {settings.GroupName} revision {settings.Revision}";
        AddEvent("SET SAVE", $"{settings.GroupName} rev {settings.Revision} · {settings.Fingerprint()[..12]}");
        OnPropertyChanged(nameof(SettingGroupPersistenceStatus));
    }

    private void LoadSelectedSettingGroup()
    {
        if (SelectedPersistedSettingGroup is null ||
            !_persistedSettingGroups.TryGetValue(SelectedPersistedSettingGroup, out var settings))
        {
            SetSettingGroupPersistenceFailure("Select a persisted setting group first.");
            return;
        }

        ApplyValidatedProtectionSettings(settings, "LOADED", addEvent: true);
        if (!TryPersistSettingGroups(settings.GroupName))
            return;

        _settingGroupPersistenceStatus = $"LOADED · {settings.GroupName} revision {settings.Revision}";
        AddEvent("SET LOAD", $"{settings.GroupName} rev {settings.Revision}");
        OnPropertyChanged(nameof(SettingGroupPersistenceStatus));
    }

    private void DeleteSelectedSettingGroup()
    {
        var selected = SelectedPersistedSettingGroup;
        if (selected is null || !_persistedSettingGroups.Remove(selected))
        {
            SetSettingGroupPersistenceFailure("Select a persisted setting group first.");
            return;
        }

        RefreshPersistedSettingGroupNames(PersistedSettingGroupNames.FirstOrDefault());
        if (!TryPersistSettingGroups(SelectedPersistedSettingGroup))
            return;

        _settingGroupPersistenceStatus = $"DELETED · {selected} · active relay settings unchanged";
        AddEvent("SET DELETE", selected);
        OnPropertyChanged(nameof(SettingGroupPersistenceStatus));
    }

    private bool TryBuildProtectionSettings(out ProtectionSettings settings)
    {
        if (SettingsEditor.TryBuild(_settings, out settings, out var error))
            return true;

        _settingsEditorStatus = $"INVALID · {error}";
        _statusText = error;
        AddEvent("SET INVALID", error);
        OnPropertyChanged(string.Empty);
        return false;
    }

    private void ApplySettingsFromEditor()
    {
        if (TryBuildProtectionSettings(out var settings))
            ApplyValidatedProtectionSettings(settings, "APPLIED", addEvent: true);
    }

    private void ApplyValidatedProtectionSettings(
        ProtectionSettings settings,
        string action,
        bool addEvent)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Validate();

        var sourceFingerprint = _workspace.InternalLab.Scenario.InjectionFingerprint;
        var wasRunning = IsRunning;
        var processBusWasDisplayed = IsProcessBusDisplayActive;

        _workspace.InternalLab.ApplySettingsPreservingSource(settings);
        _processBus.UpdateProtectionSettings(settings);
        _settings = settings;

        if (processBusWasDisplayed)
        {
            SelectedProcessBusStream = null;
            ActivateInternalDisplay();
            SetDisplayHandoverStatus(
                "Protection settings changed; display returned to INTERNAL LAB while process-bus runtimes rebuild from fresh frames.");
        }

        SyncSettingsEditor();
        _settingsDraftDirty = false;
        OnPropertyChanged(nameof(SettingsDraftDirty));
        _settingsEditorStatus = $"{action} · all protection timers and trip latches reset";
        _statusText = $"Protection settings {action.ToLowerInvariant()} · {settings.GroupName} revision {settings.Revision}. " +
                      $"Injection remains {(wasRunning ? "RUNNING" : "STOPPED")}.";

        if (addEvent)
            AddEvent("SETTINGS", $"{settings.GroupName} rev {settings.Revision} · {settings.Fingerprint()[..12]}");

        if (!string.Equals(sourceFingerprint, _workspace.InternalLab.Scenario.InjectionFingerprint, StringComparison.Ordinal))
            throw new InvalidOperationException("Applying relay settings unexpectedly changed the injection source.");

        ApplyTick(_workspace.InternalLab.CaptureFrame());
        OnPropertyChanged(string.Empty);
    }

    private void ReplacePersistedSettingGroups(IEnumerable<ProtectionSettings> groups)
    {
        _persistedSettingGroups.Clear();
        foreach (var settings in groups)
            _persistedSettingGroups[settings.GroupName] = settings;
        RefreshPersistedSettingGroupNames(null);
    }

    private void RefreshPersistedSettingGroupNames(string? selected)
    {
        PersistedSettingGroupNames.Clear();
        foreach (var name in _persistedSettingGroups.Keys.OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
            PersistedSettingGroupNames.Add(name);

        SelectedPersistedSettingGroup = selected is not null && _persistedSettingGroups.ContainsKey(selected)
            ? _persistedSettingGroups.Keys.First(name => string.Equals(name, selected, StringComparison.OrdinalIgnoreCase))
            : PersistedSettingGroupNames.FirstOrDefault();
    }

    private bool TryPersistSettingGroups(string? activeGroupName)
    {
        try
        {
            _settingGroupStore.Save(_persistedSettingGroups.Values, activeGroupName);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException)
        {
            SetSettingGroupPersistenceFailure(ex.Message);
            return false;
        }
    }

    private void SetSettingGroupPersistenceFailure(string message)
    {
        _settingGroupPersistenceStatus = $"FAILED · {message}";
        AddEvent("SET STORE", _settingGroupPersistenceStatus);
        OnPropertyChanged(nameof(SettingGroupPersistenceStatus));
    }
}
