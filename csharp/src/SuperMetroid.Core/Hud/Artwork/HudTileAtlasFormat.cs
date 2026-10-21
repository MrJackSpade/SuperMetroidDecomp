using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Installed HUD atlas identity and standard transfer geometry.</summary>
public static class HudTileAtlasFormat
{
    /// <summary>Installation-relative indexed PNG filename for standard HUD/minimap characters, separate from the 4-bpp map atlas.</summary>
    public const string FileName = "hud-tiles.png";
    /// <summary>$1000 bytes: 256 8-by-8 2-bpp characters, sixteen planar bytes each.</summary>
    public const int CharacterByteCount = MapTileAtlasFormat.Width * MapTileAtlasFormat.Height / 4;
    /// <summary>The standard BG3 characters' restore quarters.</summary>
    public const int RestoreQuarterCount = 4;
    /// <summary>Bytes in one restore quarter of the standard BG3 characters.</summary>
    public const ushort RestoreQuarterBytes = CharacterByteCount / RestoreQuarterCount;
    /// <summary>$2000-byte native upload: the character data plus the equal-sized all-zero BG2 tilemap-clearing region.</summary>
    public const ushort TransferByteCount = CharacterByteCount * 2;
    /// <summary>$9A:B200, Tiles_Standard_BG3; the installed transfer supplies these characters and the following Clear BG2 tilemap region rather than rereading cartridge bytes.</summary>
    public const int SourceAddress = MapTileAtlasRomData.HudCharacterData;
}
