using SuperMetroid.Core.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>The window widths the checks compare.</summary>
internal static class ListChecks
{
    /// <summary>The width of the wide window over a title.</summary>
    internal static int WideWidth => ListWindows.WideWidth("Items");
}
