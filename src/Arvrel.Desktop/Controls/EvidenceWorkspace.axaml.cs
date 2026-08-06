using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Arvrel.Desktop.ViewModels;

namespace Arvrel.Desktop.Controls;

public sealed partial class EvidenceWorkspace : UserControl
{
    private static readonly FilePickerFileType JsonEvidenceFiles = new("ARVREL relay evidence")
    {
        Patterns = new[] { "*.json" },
        MimeTypes = new[] { "application/json" }
    };

    public EvidenceWorkspace()
    {
        InitializeComponent();
    }

    private async void ExportEvidence_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is not { } storageProvider)
        {
            viewModel.ReportEvidenceExportFailure("Storage provider is unavailable on this platform.");
            return;
        }

        try
        {
            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export ARVREL relay evidence",
                SuggestedFileName = viewModel.SuggestedEvidenceFileName,
                DefaultExtension = "json",
                FileTypeChoices = new[] { JsonEvidenceFiles }
            });

            if (file is null)
                return;

            await using var stream = await file.OpenWriteAsync();
            if (stream.CanSeek)
                stream.SetLength(0);
            await viewModel.WriteEvidenceAsync(stream);
        }
        catch (Exception ex)
        {
            viewModel.ReportEvidenceExportFailure(ex.Message);
        }
    }
}
