
namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>One ordered strip of 2-bpp room-FX, treadmill, and statue characters.</summary>
public static class RoomFxBlockAnimationAtlasFormat
{
    /// <summary>89 characters in the original simple room-FX prefix, before treadmill, statue, spore, and spike extensions.</summary>
    public const int LegacyTileCount = 89;
    /// <summary>Four Wrecked Ship treadmill pictures, each containing two 2bpp characters; direction reverses their runtime order.</summary>
    public const int TreadmillFrameCount = 4;
    /// <summary>Character count of the historical prefix after its eight treadmill characters, before the shared Tourian-statue strip.</summary>
    public const int PreStatueTileCount = LegacyTileCount + TreadmillFrameCount * 2;
    /// <summary>Character count in the contiguous Tourian-statue strip; each statue picture is an overlapping window within it.</summary>
    public const int StatueTileCount = 96;
    /// <summary>Character count of the historical prefix including the statue strip but preceding the spore animation extension.</summary>
    public const int PreSporesTileCount = PreStatueTileCount + StatueTileCount;
    /// <summary>Nine 2bpp characters across the three spore pictures.</summary>
    public const int SporesTileCount = 9;
    /// <summary>Character count of the historical prefix including spores but preceding horizontal-spike artwork.</summary>
    public const int PreSpikesTileCount = PreSporesTileCount + SporesTileCount;
    /// <summary>24 characters in the three distinct horizontal-spike pictures; the fourth animation step reuses the second picture rather than adding tiles.</summary>
    public const int SpikeTileCount = 24;
    /// <summary>Total 8-by-8 characters in the current ordered strip, including all historical prefixes and the spike extension.</summary>
    public const int TileCount = PreSpikesTileCount + SpikeTileCount;
    /// <summary>Total compiled planar transfer bytes, sixteen per character, including the shared statue strip rather than duplicating its overlapping frames.</summary>
    public const int TotalByteCount = TileCount * 16;
}
