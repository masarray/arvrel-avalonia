using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arvrel.Protection.Tests;

[TestClass]
public sealed class IecCurveFormulaCultureTests
{
    [TestMethod]
    public void UserDefinedFormulaUsesInvariantDecimalIdentity()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("de-DE");

            var formula = IecCurveCalculator.Formula(
                IecCurveFamily.UserDefined,
                userK: 0.23,
                userAlpha: 0.37,
                userC: 0.08);

            Assert.AreEqual("t = TMS × (0.23 / (M^0.37 − 1) + 0.08)", formula);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}
