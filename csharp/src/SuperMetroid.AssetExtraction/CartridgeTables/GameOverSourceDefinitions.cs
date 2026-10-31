using SuperMetroid.Core;


namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the game-over screen's text streams, Baby records and sprite placement (#1416): the
/// import pipeline's private knowledge of the cartridge, decoded once into the installed game-over presentation.
/// </summary>
[EncodingOwner("cartridge addresses of the game-over text streams, Baby records and sprites")]
internal static class GameOverSourceDefinitions
{
    /// <summary>Bank $81, holding the text streams whose bank-relative pointers the menu's initialization at $81:9206 loads.</summary>
    internal const int TextBank = 0x810000;

    /// <summary>Bank $82, holding the Baby animation records, its palettes and the menu spritemaps.</summary>
    internal const int SpriteBank = 0x820000;

    /// <summary>The bytes between consecutive page rows in a text stream's destination: 32 two-byte tilemap entries.</summary>
    internal const int TextRowBytes = 32 * sizeof(ushort);

    /// <summary>The $FFFF word after the final Baby record that restarts the stream at its first record ($82:BC27).</summary>
    internal const ushort BabyAnimationEnd = 0xffff;

    /// <summary>$82:BBBF/$BBD1: the screen X both the Baby and its container are drawn at.</summary>
    internal const ushort BabyX = 124;

    /// <summary>$82:BBC2/$BBD4: the screen Y both the Baby and its container are drawn at.</summary>
    internal const ushort BabyY = 80;

    /// <summary>$82:BBB7: the sprite palette the Baby is drawn with, whose colours its animation replaces.</summary>
    internal const int BabyPalette = 4;

    /// <summary>$82:BBC9: the sprite palette the stationary container is drawn with.</summary>
    internal const int EggPalette = 5;

    /// <summary>$82:BBCE: menu spritemap $64 (table entry $82:C631, composition $82:CFE0), the Baby's container.</summary>
    internal const ushort EggSpritemap = 0x64;

    /// <summary>The screen X of the selection missile, shared by the Yes and No choices.</summary>
    internal const int MissileX = 40;

    /// <summary>The screen Y of the selection missile beside Yes (return to game).</summary>
    internal const int YesMissileY = 160;

    /// <summary>The screen Y of the selection missile beside No (return to title).</summary>
    internal const int NoMissileY = 192;
}
