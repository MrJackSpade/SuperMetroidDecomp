using SuperMetroid.Core.Menus;

namespace SuperMetroid.Verification.Menus;

/// <summary>The rows the checks compare.</summary>
internal static class MenuChecks
{
    /// <summary>Whether the base row is abstract, which leaves its rows to the projects using the menu.</summary>
    internal static bool RowsDiffer =>
        typeof(MenuRow).IsAbstract;
}
