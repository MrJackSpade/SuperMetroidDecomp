using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable four-bit OBJ characters used by the Ceres and Zebes escape timers.</summary>
public sealed class EscapeTimerTileAtlas : IInstalledArtworkTransferSource
{
    // Native digit/label identities select a typeface; they do not generate its selected contours.
    // The reviewed original basis is275 fill sites plus their blank complement and four edge choices.
    // Independent PNG edits remain separate from calculated defaults and shared pixel relationships.
    private readonly HashSet<int> fillPixels;
    private readonly Dictionary<int, bool> fillOverrides;
    private readonly Dictionary<int, byte> pixelOverrides;

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
    }

    private bool IsFill(int index)
    {
        int source = EscapeTimerGlyphDefinitions.SourcePixel(index);
        return source != index && pixelOverrides.TryGetValue(index, out byte value)
            ? value == EscapeTimerGlyphDefinitions.FillInk(index) : BasisFill(source);
    }

    private bool BasisFill(int index) => fillOverrides.TryGetValue(index, out bool supplied) ? supplied :
        EscapeTimerGlyphDefinitions.TryDefaultFill(index, out bool calculated) ? calculated : fillPixels.Contains(index);

    private byte Pixel(int index)
    {
        if (pixelOverrides.TryGetValue(index, out byte value)) return value;
        int source = EscapeTimerGlyphDefinitions.SourcePixel(index);
        if (source != index) return Pixel(source);
        return BasisFill(index) ? EscapeTimerGlyphDefinitions.FillInk(index) : EscapeTimerGlyphDefinitions.Outline(index, IsFill);
    }

    private byte[] Transfer() => SnesPlanarTileEncoder.Encode(
        Enumerable.Range(0, EscapeTimerTileAtlasFormat.Width * EscapeTimerTileAtlasFormat.Height).Select(Pixel).ToArray(),
        EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height, EscapeTimerTileAtlasFormat.BitsPerPixel);

    /// <summary>Loads the indexed PNG and compiles its pixels to the cartridge's two native DMA pages.</summary>
    public static EscapeTimerTileAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(
            png,
            EscapeTimerTileAtlasFormat.Width,
            EscapeTimerTileAtlasFormat.Height);
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4);
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
        if (sourceAddress == EscapeTimerTileRomData.FirstSourceAddress && byteCount == EscapeTimerTileAtlasFormat.FirstByteCount)
        {
            data = Resolve(VramAssetId.EscapeTimerFirstTiles);
            return true;
        }
        if (sourceAddress == EscapeTimerTileRomData.SecondSourceAddress && byteCount == EscapeTimerTileAtlasFormat.SecondByteCount)
        {
            data = Resolve(VramAssetId.EscapeTimerSecondTiles);
            return true;
        }
        data = default;
        return false;
    }

    /// <summary>Queues an installed page when a native transfer record identifies timer artwork.</summary>
    public bool TryQueueNativeTransfer(VramWriteQueue queue, int sourceAddress, ushort byteCount,
        ushort destinationWord)
    {
        ArgumentNullException.ThrowIfNull(queue);
        if (sourceAddress == EscapeTimerTileRomData.FirstSourceAddress &&
            byteCount == EscapeTimerTileAtlasFormat.FirstByteCount &&
            destinationWord == EscapeTimerTileAtlasFormat.FirstDestinationWord)
        {
            if (Resolve(VramAssetId.EscapeTimerFirstTiles).Length != byteCount)
                throw new InvalidDataException("Escape timer first page no longer matches its native transfer record.");
            queue.EnqueueAsset(VramAssetId.EscapeTimerFirstTiles, byteCount, destinationWord);
            return true;
        }
        if (sourceAddress == EscapeTimerTileRomData.SecondSourceAddress &&
            byteCount == EscapeTimerTileAtlasFormat.SecondByteCount &&
            destinationWord == EscapeTimerTileAtlasFormat.SecondDestinationWord)
        {
            if (Resolve(VramAssetId.EscapeTimerSecondTiles).Length != byteCount)
                throw new InvalidDataException("Escape timer second page no longer matches its native transfer record.");
            queue.EnqueueAsset(VramAssetId.EscapeTimerSecondTiles, byteCount, destinationWord);
            return true;
        }
        return false;
    }

    /// <summary>Publishes one native page directly for the Mother Brain sequence's synchronous transfer owner.</summary>
    public bool TryLoadNativeTransfer(SnesVram vram, int sourceAddress, ushort byteCount,
        ushort destinationWord)
    {
        ArgumentNullException.ThrowIfNull(vram);
        VramAssetId asset;
        if (sourceAddress == EscapeTimerTileRomData.FirstSourceAddress &&
            byteCount == EscapeTimerTileAtlasFormat.FirstByteCount &&
            destinationWord == EscapeTimerTileAtlasFormat.FirstDestinationWord)
            asset = VramAssetId.EscapeTimerFirstTiles;
        else if (sourceAddress == EscapeTimerTileRomData.SecondSourceAddress &&
            byteCount == EscapeTimerTileAtlasFormat.SecondByteCount &&
            destinationWord == EscapeTimerTileAtlasFormat.SecondDestinationWord)
            asset = VramAssetId.EscapeTimerSecondTiles;
        else
            return false;

        vram.ExecuteQueuedAssetWrite(Resolve(asset).Span, destinationWord);
        return true;
    }
}

/// <summary>Indexed-PNG and native OBJ transfer geometry for editable escape-timer characters.</summary>
public static class EscapeTimerTileAtlasFormat
{
    public const string FileName = "escape-timer-tiles.png";
    public const int BitsPerPixel = 4;
    public const int ColorCount = 16;
    public const int TileCount = 25;
    public const int Width = TileCount * 8;
    public const int Height = 8;

    public const ushort FirstByteCount = 0x0200;
    public const ushort FirstDestinationWord = 0x7e00;

    public const ushort SecondByteCount = 0x0120;
    public const ushort SecondDestinationWord = 0x7f00;

    public const int TotalByteCount = FirstByteCount + SecondByteCount;
}
