using System.Collections.Frozen;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// How the cartridge lays a gameplay message out as tile words (#1416, #1466): its border rows, the save confirmation's
/// selection row, and the button glyphs an item panel's words show and where they go. The import composes each message
/// form's image from them; the game names the form it shows.
/// </summary>
public static class GameplayMessageTilemapDefinitions
{
    /// <summary>Number of rows occupied by the upper and lower borders together.</summary>
    public const int BorderRows = 2;

    /// <summary>The message row the save confirmation's YES/NO selection occupies, counted from the upper border.</summary>
    public const int SaveSelectionRow = 4;

    /// <summary>Cells in one save-selection row.</summary>
    public const int SaveSelectionRowWords = 32;

    /// <summary>The blank glyph drawn when no button the glyphs show is bound.</summary>
    public static BackgroundTile UnknownGlyph { get; } = new(0x04e, 2, highPriority: true);

    /// <summary>The glyph of each button a panel can name, and of no such button.</summary>
    public static FrozenDictionary<ControllerButton, BackgroundTile> Glyphs { get; } = new Dictionary<ControllerButton, BackgroundTile>
    {
        [ControllerButton.A] = new(0x0e0, 2, highPriority: true),
        [ControllerButton.B] = new(0x0e1, 7, highPriority: true),
        [ControllerButton.X] = new(0x0f7, 3, highPriority: true),
        [ControllerButton.Y] = new(0x0f8, 6, highPriority: true),
        [ControllerButton.Select] = new(0x0d0, 6, highPriority: true),
        [ControllerButton.L] = new(0x0eb, 6, highPriority: true),
        [ControllerButton.R] = new(0x0f1, 6, highPriority: true),
        [ControllerButton.None] = UnknownGlyph,
    }.ToFrozenDictionary();

    /// <summary>The byte offset of the button glyph in each panel that names a button.</summary>
    public static FrozenDictionary<GameplayMessageId, int> GlyphByteOffsets { get; } = new Dictionary<GameplayMessageId, int>
    {
        [GameplayMessageId.MissileTank] = 0x12a,
        [GameplayMessageId.SuperMissileTank] = 0x12a,
        [GameplayMessageId.Bombs] = 0x12a,
        [GameplayMessageId.PowerBombTank] = 0x12c,
        [GameplayMessageId.GrappleBeam] = 0x12c,
        [GameplayMessageId.XrayScope] = 0x12c,
        [GameplayMessageId.SpeedBooster] = 0x120,
    }.ToFrozenDictionary();
}
