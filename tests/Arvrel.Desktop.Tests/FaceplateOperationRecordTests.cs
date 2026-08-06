using Arvrel.Desktop.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Desktop.Tests;

[TestClass]
public sealed class FaceplateOperationRecordTests
{
    [TestMethod]
    public async Task InternalFaultProducesPortablePickupAndTripRecord()
    {
        await using var viewModel = new MainWindowViewModel();

        Assert.IsNull(viewModel.CurrentOperationRecord);

        viewModel.InjectAgFault();
        for (var index = 0; index < 6; index++)
            viewModel.Tick();

        var record = viewModel.CurrentOperationRecord;
        Assert.IsNotNull(record);
        Assert.IsTrue(record.TripTimestamp.HasValue);
        Assert.IsFalse(string.IsNullOrWhiteSpace(record.PickupElement));
        Assert.IsFalse(string.IsNullOrWhiteSpace(record.TripElement));
        Assert.IsTrue(record.PickupQuantity > 0);
        Assert.IsTrue(record.TripQuantity > 0);
        Assert.IsTrue(record.OperateTime >= TimeSpan.Zero);

        viewModel.FaceplateRecordsCommand.Execute(null);

        Assert.AreEqual("RECORDS", viewModel.FaceplatePageName);
        StringAssert.Contains(viewModel.FaceplateRow1Value, "TRIP");
        StringAssert.Contains(viewModel.FaceplateRow1Value, $"#{record.Sequence:0000}");
        StringAssert.Contains(viewModel.FaceplateDetailText, "operate");
    }

    [TestMethod]
    public async Task RelayResetDoesNotEraseLastCompletedOperationRecord()
    {
        await using var viewModel = new MainWindowViewModel();
        viewModel.InjectAgFault();
        for (var index = 0; index < 6; index++)
            viewModel.Tick();

        var sequence = viewModel.CurrentOperationRecord?.Sequence;
        Assert.IsNotNull(sequence);

        viewModel.ResetRelay();

        Assert.IsFalse(viewModel.TripLatched);
        Assert.AreEqual(sequence, viewModel.CurrentOperationRecord?.Sequence);
        Assert.IsTrue(viewModel.CurrentOperationRecord?.TripTimestamp.HasValue == true);
    }
}
