using SuperMetroid.Core.Game;

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
    /// <summary>Independently supplied source bytes; padding and repeated patches are resolved on demand.</summary>
    internal readonly Dictionary<int, byte> sourceBytes;

    /// <summary>Validates the five transfer pages and retains bytes that differ from their source relation.</summary>
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

    /// <summary>Resolves an edited character byte through named repeats or transparent padding.</summary>
    private byte ReadByte(int index)
    {
        if (sourceBytes.TryGetValue(index, out byte value)) return value;
        int source = SamusDeathTileAtlasFormat.SourceByte(index);
        return source < 0 ? (byte)0 : source != index ? ReadByte(source)
            : throw new InvalidDataException("Death artwork source byte is unavailable.");
    }

    /// <summary>Materializes a contiguous transfer segment in the native queue order.</summary>
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
            new(SamusDeathTileAtlasFormat.Width, SamusDeathTileAtlasFormat.Height));
        return new SamusDeathTileAtlas(SnesPlanarTileEncoder.Encode(image.Pixels,
            image.Size, SamusDeathTileAtlasFormat.BitsPerPixel));
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
