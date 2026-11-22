
namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>One ordered strip of 2-bpp room-FX, treadmill, and statue characters.</summary>
public static partial class RoomFxBlockAnimationAtlasFormat
{
    /// <summary>The characters of the simple room-FX pictures, which open the strip.</summary>
    public const int SimpleEffectTileCount = 89;
    /// <summary>Four Wrecked Ship treadmill pictures, each containing two 2bpp characters; direction reverses their runtime order.</summary>
    public const int TreadmillFrameCount = 4;
    /// <summary>The first character of the shared Tourian-statue segment, after the simple room-FX and treadmill characters.</summary>
    public const int StatueFirstTile = SimpleEffectTileCount + TreadmillFrameCount * 2;
    /// <summary>Character count in the contiguous Tourian-statue strip; each statue picture is an overlapping window within it.</summary>
    public const int StatueTileCount = 96;
    /// <summary>The first character of the spore segment, after the statue segment.</summary>
    internal const int SporesFirstTile = StatueFirstTile + StatueTileCount;
    /// <summary>Nine 2bpp characters across the three spore pictures.</summary>
    public const int SporesTileCount = 9;
    /// <summary>The first character of the horizontal-spike segment, after the spore segment.</summary>
    public const int SpikesFirstTile = SporesFirstTile + SporesTileCount;
    /// <summary>24 characters in the three distinct horizontal-spike pictures; the fourth animation step reuses the second picture rather than adding tiles.</summary>
    public const int SpikeTileCount = 24;
    /// <summary>Total 8-by-8 characters in the ordered strip: every segment through the horizontal spikes.</summary>
    public const int TileCount = SpikesFirstTile + SpikeTileCount;
    /// <summary>Total compiled planar transfer bytes, sixteen per character, including the shared statue strip rather than duplicating its overlapping frames.</summary>
    public const int TotalByteCount = TileCount * 16;
}
