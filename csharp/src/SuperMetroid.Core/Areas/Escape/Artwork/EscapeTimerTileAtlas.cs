using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Escape.Artwork;

/// <summary>Editable four-bit OBJ characters used by the Ceres and Zebes escape timers.</summary>
public sealed class EscapeTimerTileAtlas : IInstalledArtworkTransferSource
{
    /// <summary>Independent fill sites of the native timer typeface beyond the calculated defaults.</summary>
    internal readonly HashSet<int> fillPixels;
    /// <summary>PNG edits that change whether a calculated basis pixel uses the fill ink.</summary>
    internal readonly Dictionary<int, bool> fillOverrides;
    /// <summary>Independent PNG colors that differ from a shared source pixel or calculated outline.</summary>
    internal readonly Dictionary<int, byte> pixelOverrides;

    /// <summary>Separates selected timer contours from shared pixel relationships and calculated default fill sites.</summary>
    private EscapeTimerTileAtlas(byte[] pixels)
    {
        fillPixels = new HashSet<int>();
        fillOverrides = new Dictionary<int, bool>();
        for (int index = 0; index < pixels.Length; index++)
        {
            if (EscapeTimerGlyphDefinitions.SourcePixel(index) != index) continue;
            bool filled = pixels[index] == EscapeTimerGlyphDefinitions.FillInk(index);
            if (EscapeTimerGlyphDefinitions.TryDefaultFill(index, out bool expected))
            {
                if (filled != expected) fillOverrides.Add(index, filled);
            }
            else if (filled) fillPixels.Add(index);
        }
        pixelOverrides = Enumerable.Range(0, pixels.Length)
            .Where(index => EscapeTimerGlyphDefinitions.SourcePixel(index) != index && pixels[index] != pixels[EscapeTimerGlyphDefinitions.SourcePixel(index)])
            .ToDictionary(index => index, index => pixels[index]);
        foreach (int index in Enumerable.Range(0, pixels.Length))
            if (EscapeTimerGlyphDefinitions.SourcePixel(index) == index && !IsFill(index) &&
                pixels[index] != EscapeTimerGlyphDefinitions.Outline(index, IsFill))
                pixelOverrides.Add(index, pixels[index]);
        Artwork = new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.EscapeTimerTiles), Transfer());
    }

    /// <summary>The 25 timer characters as owned sprite artwork, in transfer order: character 0 is OBJ character $1E0.</summary>
    public SpriteArtwork Artwork { get; }

    /// <summary>Determines whether a pixel uses the glyph fill ink, honoring an independently edited shared pixel.</summary>
    private bool IsFill(int index)
    {
        int source = EscapeTimerGlyphDefinitions.SourcePixel(index);
        return source != index && pixelOverrides.TryGetValue(index, out byte value)
            ? value == EscapeTimerGlyphDefinitions.FillInk(index) : BasisFill(source);
    }

    /// <summary>Resolves a basis pixel from its PNG override, native calculated fill, or retained fill-site set.</summary>
    private bool BasisFill(int index) => fillOverrides.TryGetValue(index, out bool supplied) ? supplied :
        EscapeTimerGlyphDefinitions.TryDefaultFill(index, out bool calculated) ? calculated : fillPixels.Contains(index);

    /// <summary>Resolves an edited pixel, shared source pixel, or calculated fill and outline ink.</summary>
    private byte Pixel(int index)
    {
        if (pixelOverrides.TryGetValue(index, out byte value)) return value;
        int source = EscapeTimerGlyphDefinitions.SourcePixel(index);
        if (source != index) return Pixel(source);
        return BasisFill(index) ? EscapeTimerGlyphDefinitions.FillInk(index) : EscapeTimerGlyphDefinitions.Outline(index, IsFill);
    }

    /// <summary>Rebuilds all 25 timer characters in native four-bit planar DMA order.</summary>
    internal byte[] Transfer() => SnesPlanarTileEncoder.Encode(
        Enumerable.Range(0, EscapeTimerTileAtlasFormat.Width * EscapeTimerTileAtlasFormat.Height).Select(Pixel).ToArray(),
        new(EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height), EscapeTimerTileAtlasFormat.BitsPerPixel);

    /// <summary>Loads the indexed PNG and compiles its pixels to the cartridge's two native DMA pages.</summary>
    public static EscapeTimerTileAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png,
            new(EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height));
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4);
        if (planar.Length != EscapeTimerTileAtlasFormat.TotalByteCount)
            throw new InvalidDataException(
                $"Escape timer artwork compiled to {planar.Length} bytes; expected {EscapeTimerTileAtlasFormat.TotalByteCount}.");
        return new(image.Pixels);
    }

    /// <summary>Resolves one native page without combining or retiming its original transfer record.</summary>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset)
    {
        byte[] transfer = Transfer();
        return asset switch
        {
            VramAssetId.EscapeTimerFirstTiles => transfer.AsMemory(0, EscapeTimerTileAtlasFormat.FirstByteCount),
            VramAssetId.EscapeTimerSecondTiles => transfer.AsMemory(EscapeTimerTileAtlasFormat.FirstByteCount, EscapeTimerTileAtlasFormat.SecondByteCount),
            _ => throw new InvalidDataException($"Escape timer artwork cannot resolve VRAM asset {asset}."),
        };
    }

    /// <summary>Restored native queue descriptors resolve the same two installed pages as typed uploads.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        if (EscapeTimerTilePages.TryMatch(sourceAddress, byteCount, out EscapeTimerTilePage page))
        {
            data = Resolve(EscapeTimerTilePages.AssetOf(page));
            return true;
        }
        data = default;
        return false;
    }

    /// <summary>Queues an installed page when a native transfer record identifies timer artwork.</summary>
    [PpuLayoutBoundary("codec: queues a matched native escape-timer transfer record's encoded VRAM destination")]
    public bool TryQueueNativeTransfer(VramWriteQueue queue, int sourceAddress, ushort byteCount,
        ushort destinationWord)
    {
        Ensure.NotNull(queue);
        if (!EscapeTimerTilePages.TryMatch(sourceAddress, byteCount, destinationWord, out EscapeTimerTilePage page))
            return false;
        VramAssetId asset = EscapeTimerTilePages.AssetOf(page);
        if (Resolve(asset).Length != byteCount)
            throw new InvalidDataException($"Escape timer {page} page no longer matches its native transfer record.");
        queue.EnqueueAsset(asset, byteCount, VramDmaDestination.FromNative(destinationWord));
        return true;
    }

    /// <summary>Publishes one native page directly for the Mother Brain sequence's synchronous transfer owner.</summary>
    [PpuLayoutBoundary("codec: executes a matched native escape-timer transfer record's encoded VRAM destination")]
    public bool TryLoadNativeTransfer(SnesVram vram, int sourceAddress, ushort byteCount,
        ushort destinationWord)
    {
        Ensure.NotNull(vram);
        if (!EscapeTimerTilePages.TryMatch(sourceAddress, byteCount, destinationWord, out EscapeTimerTilePage page))
            return false;

        vram.ExecuteQueuedAssetWrite(Resolve(EscapeTimerTilePages.AssetOf(page)).Span, destinationWord);
        return true;
    }
}
