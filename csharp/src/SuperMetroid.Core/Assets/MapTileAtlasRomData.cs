namespace SuperMetroid.Core.Assets;

/// <summary>Import-time cartridge sources for shared map artwork.</summary>
internal static class MapTileAtlasRomData
{
    /// <summary>$B6:A000, the second 256 pause-menu background characters loaded by GameState_13.</summary>
    internal const int PauseInterfaceCharacterData = 0xb6a000;
    /// <summary>$B6:8000: first 256 4-bpp characters shared by pause and file-select maps.</summary>
    internal const int CharacterData = 0xb68000;
    /// <summary>$9A:B200: standard 2-bpp BG3/HUD tiles; the following equal-sized clear table is all zero.</summary>
    internal const int HudCharacterData = 0x9ab200;
}
