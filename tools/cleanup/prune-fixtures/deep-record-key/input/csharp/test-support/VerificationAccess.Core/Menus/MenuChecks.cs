using SuperMetroid.Core.Menus;

namespace SuperMetroid.Verification.Menus;

/// <summary>The rows the checks compare.</summary>
internal static class MenuChecks
{
    /// <summary>Whether two rows differing only in the button number they hold are different rows.</summary>
    internal static bool RowsDiffer =>
        new MenuRow.OnControllerPage(new ControllerRow.AssignsButton(new ControllerButton.Numbered(1))) !=
        new MenuRow.OnControllerPage(new ControllerRow.AssignsButton(new ControllerButton.Numbered(2)));
}
