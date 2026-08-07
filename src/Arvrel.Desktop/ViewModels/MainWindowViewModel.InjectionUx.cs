namespace Arvrel.Desktop.ViewModels;

public sealed partial class MainWindowViewModel
{
    public IReadOnlyList<InjectionChannelViewModel> CurrentInjectionChannels
        => InjectionChannels
            .Where(static channel => string.Equals(channel.Unit, "A", StringComparison.Ordinal))
            .ToArray();

    public IReadOnlyList<InjectionChannelViewModel> VoltageInjectionChannels
        => InjectionChannels
            .Where(static channel => string.Equals(channel.Unit, "V", StringComparison.Ordinal))
            .ToArray();
}
