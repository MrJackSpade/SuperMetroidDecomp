using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Installed HUD atlas identity and geometry.</summary>
public static class HudTileAtlasFormat
{
    /// <summary>Installation-relative indexed PNG filename for standard HUD/minimap characters, separate from the 4-bpp map atlas.</summary>
    public const string FileName = "hud-tiles.png";
    /// <summary>$1000 bytes: 256 8-by-8 2-bpp characters, sixteen planar bytes each.</summary>
    public const int CharacterByteCount = MapTileAtlasFormat.Width * MapTileAtlasFormat.Height / 4;
    /// <summary>$2000 bytes: the character data plus the equal-sized all-zero half that clears BG2's words.</summary>
    public const ushort ByteCount = CharacterByteCount * 2;
}
