using SuperMetroid.Core.Menus;

namespace SuperMetroid.Verification.Menus;

/// <summary>The rows the checks compare.</summary>
internal static class MenuChecks
{
    /// <summary>Whether two rows differing only in their place are different rows.</summary>
    internal static bool RowsDiffer =>
        new LabelledRow(1) != new LabelledRow(2);
}
