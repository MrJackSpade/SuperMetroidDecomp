using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>File select's selection counts; its fields are the menus' pictures (#1467).</summary>
/// <remarks>Shared missile animation is defined by <see cref="MenuMissileAnimationDefinitions"/>.</remarks>
internal static class FileSelectLayout
{
    /// <summary>Three save slots on each file-select page.</summary>
    public const int SaveSlotCount = 3;

    /// <summary>Updates each step of the menu's fades takes once the menu has opened: one level every second update.</summary>
    public const int FadeUpdatesPerStep = 2;
    /// <summary>Slots A-C followed by Exit.</summary>
    public const int DataSelectionCount = 4;
}
