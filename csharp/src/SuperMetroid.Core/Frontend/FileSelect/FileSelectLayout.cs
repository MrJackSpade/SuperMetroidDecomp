using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>File select's selection counts; its fields are the menus' pictures (#1467).</summary>
/// <remarks>Shared missile animation is defined by <see cref="MenuMissileAnimationDefinitions"/>.</remarks>
internal static class FileSelectLayout
{
    /// <summary>Three save slots on each file-select page.</summary>
    public const int SaveSlotCount = 3;
    /// <summary>Slots A-C followed by Copy, Clear and Exit.</summary>
    public const int MainSelectionCount = 6;
    /// <summary>Slots A-C followed by Exit.</summary>
    public const int DataSelectionCount = 4;
}
