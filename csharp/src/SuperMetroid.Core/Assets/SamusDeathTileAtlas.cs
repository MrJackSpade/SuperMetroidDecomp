using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable four-bit OBJ characters for Samus's five death-explosion transfers.</summary>
/// <remarks>The remaining 151 source tiles retain only the reviewed selected anatomical,
/// hair, suit-rupture and spark contours/pen membership from $9B:8000..93FF. Pose and
/// timer select this drawing; generating its exact human figure and shading would
/// require re-embedding the chosen image. Six blank allocation tiles and three shared
/// patches calculate separately. This narrow bitmap exception excludes palettes,
/// OAM placement, sequence timing and every other atlas.</remarks>
public sealed class SamusDeathTileAtlas : IInstalledArtworkTransferSource
{
    /// <summary>Independently retained planar bytes and edits that differ from their source-tile relation.</summary>
    private readonly Dictionary<int, byte> sourceBytes;

    /// <summary>Validates the complete five-segment atlas and stores only independent or edited source bytes.</summary>
    /// <param name="planar">4-bpp character bytes in the fixed death-transfer queue order.</param>
    private SamusDeathTileAtlas(byte[] planar)
    {
        if (planar.Length != SamusDeathTileAtlasFormat.TotalByteCount)
            throw new InvalidDataException("Samus death tile atlas has the wrong transfer size.");
        sourceBytes = Enumerable.Range(0, planar.Length)
            .Where(index => SamusDeathTileAtlasFormat.SourceByte(index) is int source &&
                (source == index || planar[index] != (source < 0 ? 0 : planar[source])))
            .ToDictionary(index => index, index => planar[index]);
    }

    /// <summary>SHA-256 of all five selected death-explosion character uploads.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusDeathTileAtlas),
        content => content.Append("death characters", ReadRange(0, SamusDeathTileAtlasFormat.TotalByteCount)));

    /// <summary>Resolves a logical atlas byte through its retained value, source-tile alias, or transparent-padding rule.</summary>
    /// <param name="index">Byte offset in the complete encoded atlas.</param>
    /// <returns>The installed planar byte, including zero for transparent padding.</returns>
    private byte ReadByte(int index)
    {
        if (sourceBytes.TryGetValue(index, out byte value)) return value;
        int source = SamusDeathTileAtlasFormat.SourceByte(index);
        return source < 0 ? (byte)0 : source != index ? ReadByte(source)
            : throw new InvalidDataException("Death artwork source byte is unavailable.");
    }

    /// <summary>Materializes a contiguous range of logical atlas bytes, resolving aliases for each offset.</summary>
    /// <param name="first">Starting byte offset in the complete encoded atlas.</param>
    /// <param name="count">Number of consecutive bytes to materialize.</param>
    /// <returns>A new array containing the resolved bytes in order.</returns>
    private byte[] ReadRange(int first, int count)
    {
        var bytes = new byte[count];
        for (int index = 0; index < count; index++) bytes[index] = ReadByte(first + index);
        return bytes;
    }

    /// <summary>Loads the 64-by-160 indexed PNG and encodes its palette indices as 4-bpp OBJ characters in death-transfer queue order, retaining independently edited source bytes.</summary>
    /// <param name="png">Caller-owned artwork stream; pixel indices are pens 0..15, not final CGRAM colors.</param>
    /// <returns>Installed artwork for the five fixed $0400-byte transfers, without changing their destinations, animation timing, or palettes.</returns>
    public static SamusDeathTileAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(png,
            SamusDeathTileAtlasFormat.Width, SamusDeathTileAtlasFormat.Height);
        return new SamusDeathTileAtlas(SnesPlanarTileEncoder.Encode(image.Pixels,
            image.Width, image.Height, SamusDeathTileAtlasFormat.BitsPerPixel));
    }

    /// <summary>Finds one native $400-byte transfer without changing its queued destination.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        var segments = SamusSpecialSequenceRomData.Death.TileSegments;
        for (int index = 0; index < segments.Length; index++)
        {
            if (segments[index].SourceAddress != sourceAddress)
                continue;
            if (byteCount != SamusSpecialSequenceRomData.Death.TileSegmentByteCount)
                throw new InvalidDataException(
                    $"Samus death segment ${sourceAddress:X6} requires " +
                    $"{SamusSpecialSequenceRomData.Death.TileSegmentByteCount} bytes, not {byteCount}.");
            data = ReadRange(index * byteCount, byteCount);
            return true;
        }
        data = default;
        return false;
    }
}

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
        int source = tile switch
        {
            SamusDeathTileAtlasAddresses.TransparentPadding0 or SamusDeathTileAtlasAddresses.TransparentPadding1 or
                SamusDeathTileAtlasAddresses.TransparentPadding2 or SamusDeathTileAtlasAddresses.TransparentPadding3 or
                SamusDeathTileAtlasAddresses.TransparentPadding4 or SamusDeathTileAtlasAddresses.TransparentPadding5 => -1,
            SamusDeathTileAtlasAddresses.RepeatedHairTip => SamusDeathTileAtlasAddresses.HairTipSource,
            SamusDeathTileAtlasAddresses.RepeatedArmEdge => SamusDeathTileAtlasAddresses.ArmEdgeSource,
            SamusDeathTileAtlasAddresses.RepeatedTorso => SamusDeathTileAtlasAddresses.TorsoSource,
            _ => tile,
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
