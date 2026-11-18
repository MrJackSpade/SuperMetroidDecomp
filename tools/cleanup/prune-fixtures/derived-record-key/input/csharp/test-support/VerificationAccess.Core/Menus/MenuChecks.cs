using SuperMetroid.Core.Menus;

namespace SuperMetroid.Verification.Menus;

/// <summary>The rows the checks compare.</summary>
internal static class MenuChecks
{
    /// <summary>Whether two rows of one page differing only in the row they name are different rows.</summary>
    internal static bool RowsDiffer =>
        new MenuRow.OnFirstPage(FirstPageRow.Start) != new MenuRow.OnFirstPage(FirstPageRow.Options);
}
