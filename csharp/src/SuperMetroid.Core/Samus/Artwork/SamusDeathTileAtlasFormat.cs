using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Five $400-byte 4-bpp segments, each occupying four 64-pixel-wide tile rows.</summary>
public static class SamusDeathTileAtlasFormat
{
    /// <summary>
    /// Source relation in the five uploaded pages: six unused transparent padding tiles
    /// at $9B:8900/8A60/8B60/8D20/9240/92E0, three named repeated anatomy patches,
    /// otherwise narrowly retained selected anatomical/hair/rupture/spark bitmap contours and pens.
    /// </summary>
    internal static int SourceByte(int index)
    {
        if ((uint)index >= TotalByteCount) throw new ArgumentOutOfRangeException(nameof(index));
        const int tileBytes = 8 * BitsPerPixel;
        int pageBytes = SamusSpecialSequenceRomData.Death.TileSegmentByteCount;
        var pages = SamusSpecialSequenceRomData.Death.TileSegments;
        int address = pages[index / pageBytes].SourceAddress + index % pageBytes;
        int tile = address - address % tileBytes;
        // Unnamed tiles keep their own bitmap; the named ones are padding or repeats.
        var named = (SamusDeathTileAddress)tile;
        int source = !Enum.IsDefined(named) ? tile : named switch
        {
            SamusDeathTileAddress.TransparentPadding0 or SamusDeathTileAddress.TransparentPadding1 or
                SamusDeathTileAddress.TransparentPadding2 or SamusDeathTileAddress.TransparentPadding3 or
                SamusDeathTileAddress.TransparentPadding4 or SamusDeathTileAddress.TransparentPadding5 => -1,
            SamusDeathTileAddress.RepeatedHairTip => (int)SamusDeathTileAddress.HairTipSource,
            SamusDeathTileAddress.RepeatedArmEdge => (int)SamusDeathTileAddress.ArmEdgeSource,
            SamusDeathTileAddress.RepeatedTorso => (int)SamusDeathTileAddress.TorsoSource,
            SamusDeathTileAddress.HairTipSource or SamusDeathTileAddress.ArmEdgeSource or
                SamusDeathTileAddress.TorsoSource => tile,
            _ => throw new InvalidOperationException($"Undefined SamusDeathTileAddress {named}."),
        };
        if (source < 0) return -1;
        source += address % tileBytes;
        for (int page = 0; page < pages.Count; page++)
            if (source >= pages[page].SourceAddress && source < pages[page].SourceAddress + pageBytes)
                return page * pageBytes + source - pages[page].SourceAddress;
        throw new InvalidOperationException("Death artwork relation leaves the native page domain.");
    }

    /// <summary>Indexed PNG filename for the five death-explosion OBJ character segments.</summary>
    public const string ArtworkFileName = "samus-death-explosion.png";
    /// <summary>Companion JSON manifest filename describing the extracted death-explosion artwork resource.</summary>
    public const string ManifestFileName = "samus-death-explosion-manifest.json";
    /// <summary>Four SNES planar bits per pixel, allowing sixteen palette-index pens and encoding each 8-by-8 character in 32 bytes.</summary>
    public const int BitsPerPixel = 4;
    /// <summary>PNG width in pixels: eight 8-pixel characters per row, independently of their final OBJ placement.</summary>
    public const int Width = 64;
    /// <summary>Five $0400-byte segments stacked in queue order: native sources $9B:8400, $8800, $8C00, $9000, then $8000.</summary>
    public const int SegmentCount = 5;
    /// <summary>Total encoded character bytes, $1400, covering 160 tiles across all five uploads.</summary>
    public const int TotalByteCount = SegmentCount * SamusSpecialSequenceRomData.Death.TileSegmentByteCount;
    /// <summary>PNG height in pixels, 160: five segments of four eight-pixel tile rows each.</summary>
    public const int Height = TotalByteCount / 32 / (Width / 8) * 8;
}
