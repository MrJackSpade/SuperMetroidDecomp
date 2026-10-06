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
    private readonly Dictionary<int, byte> sourceBytes;

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

    private byte ReadByte(int index)
    {
        if (sourceBytes.TryGetValue(index, out byte value)) return value;
        int source = SamusDeathTileAtlasFormat.SourceByte(index);
        return source < 0 ? (byte)0 : source != index ? ReadByte(source)
            : throw new InvalidDataException("Death artwork source byte is unavailable.");
    }

    private byte[] ReadRange(int first, int count)
    {
        var bytes = new byte[count];
        for (int index = 0; index < count; index++) bytes[index] = ReadByte(first + index);
        return bytes;
    }

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
    /// <summary>$9B:8F60 repeats $9B:8CC0: phase-seven hair tip at (6,-28), previously the phase-six upper-right OBJ quadrant.</summary>
    private const int RepeatedHairTip = 0x9b8f60, HairTipSource = 0x9b8cc0;
    /// <summary>$9B:9080 repeats $9B:8F80: outer arm at (-10,-12), shared by late phases six/eight.</summary>
    private const int RepeatedArmEdge = 0x9b9080, ArmEdgeSource = 0x9b8f80;
    /// <summary>$9B:93A0 repeats $9B:90A0: torso patch at (-2,-12), phase-eight lower-left OBJ quadrant and phase-six upper-left.</summary>
    private const int RepeatedTorso = 0x9b93a0, TorsoSource = 0x9b90a0;

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
            0x9b8900 or 0x9b8a60 or 0x9b8b60 or 0x9b8d20 or 0x9b9240 or 0x9b92e0 => -1,
            RepeatedHairTip => HairTipSource,
            RepeatedArmEdge => ArmEdgeSource,
            RepeatedTorso => TorsoSource,
            _ => tile,
        };
        if (source < 0) return -1;
        source += address % tileBytes;
        for (int page = 0; page < pages.Count; page++)
            if (source >= pages[page].SourceAddress && source < pages[page].SourceAddress + pageBytes)
                return page * pageBytes + source - pages[page].SourceAddress;
        throw new InvalidOperationException("Death artwork relation leaves the native page domain.");
    }

    public const string ArtworkFileName = "samus-death-explosion.png";
    public const string ManifestFileName = "samus-death-explosion-manifest.json";
    public const int BitsPerPixel = 4;
    public const int Width = 64;
    public const int SegmentCount = 5;
    public const int TotalByteCount = SegmentCount * SamusSpecialSequenceRomData.Death.TileSegmentByteCount;
    public const int Height = TotalByteCount / 32 / (Width / 8) * 8;
}
