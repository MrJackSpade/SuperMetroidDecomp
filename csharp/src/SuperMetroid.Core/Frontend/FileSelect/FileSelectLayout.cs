using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>File select's selection counts, cursor positions and the menu tiles its fields are drawn with.</summary>
/// <remarks>Shared missile animation is defined by <see cref="MenuMissileAnimationDefinitions"/>.</remarks>
internal static class FileSelectLayout
{
    /// <summary>Three save slots on each file-select page.</summary>
    public const int SaveSlotCount = 3;
    /// <summary>Slots A-C followed by Copy, Clear and Exit at $81:A312.</summary>
    public const int MainSelectionCount = 6;
    /// <summary>Slots A-C followed by Exit at $81:9772 and $81:9C03.</summary>
    public const int DataSelectionCount = 4;

    /// <summary>The blank menu tile.</summary>
    public static BackgroundTile BlankTile { get; } = new(0x0f, 0, highPriority: false);
    /// <summary>The first numeric menu tile: the digits follow it in order.</summary>
    public static BackgroundTile DigitTileBase { get; } = new(0x60, 0, highPriority: true);
    /// <summary>The first letter tile used to draw a save slot's name in the Copy and Clear prompts: the slots' letters follow it.</summary>
    public static BackgroundTile SamusLetterTileBase { get; } = new(0x6a, 0, highPriority: true);

    /// <summary>The menu tile of decimal digit <paramref name="digit"/>.</summary>
    public static BackgroundTile Digit(int digit) => DigitTileBase with { Tile = DigitTileBase.Tile + digit };

    /// <summary>The menu tile of save slot <paramref name="slot"/>'s letter.</summary>
    public static BackgroundTile SlotLetter(int slot) => SamusLetterTileBase with { Tile = SamusLetterTileBase.Tile + slot };
}
