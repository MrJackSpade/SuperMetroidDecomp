using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>File select's fade cadence and opening hold; its fields are the menus' pictures (#1467).</summary>
/// <remarks>Shared missile animation is defined by <see cref="MenuMissileAnimationDefinitions"/>.</remarks>
internal static class FileSelectLayout
{
    /// <summary>Updates each step of the menu's fades takes once the menu has opened: one level every second update.</summary>
    public const int FadeUpdatesPerStep = 2;

    /// <summary>Updates the menu holds its blanked screen black, after blanking it, before setting up its fades.</summary>
    public const int BlackHoldUpdates = 1;
}
