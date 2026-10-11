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
        if (EscapeTimerTilePages.TryMatch(sourceAddress, byteCount, out EscapeTimerTilePage page))
        {
            data = Resolve(EscapeTimerTilePages.AssetOf(page));
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
        if (!EscapeTimerTilePages.TryMatch(sourceAddress, byteCount, destinationWord, out EscapeTimerTilePage page))
            return false;
        VramAssetId asset = EscapeTimerTilePages.AssetOf(page);
        if (Resolve(asset).Length != byteCount)
            throw new InvalidDataException($"Escape timer {page} page no longer matches its native transfer record.");
        queue.EnqueueAsset(asset, byteCount, destinationWord);
        return true;
    }

    /// <summary>Publishes one native page directly for the Mother Brain sequence's synchronous transfer owner.</summary>
    public bool TryLoadNativeTransfer(SnesVram vram, int sourceAddress, ushort byteCount,
        ushort destinationWord)
    {
        ArgumentNullException.ThrowIfNull(vram);
        if (!EscapeTimerTilePages.TryMatch(sourceAddress, byteCount, destinationWord, out EscapeTimerTilePage page))
            return false;

        vram.ExecuteQueuedAssetWrite(Resolve(EscapeTimerTilePages.AssetOf(page)).Span, destinationWord);
        return true;
    }
}

/// <summary>The two native escape-timer character uploads.</summary>
public enum EscapeTimerTilePage
{
    /// <summary>Characters 0..15 from $B0:C000 to VRAM word $7E00.</summary>
    First,
    /// <summary>Characters 16..24 from $B0:C200 to VRAM word $7F00.</summary>
    Second,
}

/// <summary>Native transfer records of the escape-timer character pages.</summary>
public static class EscapeTimerTilePages
{
    private static readonly EscapeTimerTilePage[] Pages = Enum.GetValues<EscapeTimerTilePage>();

    /// <summary>The page's native long source address.</summary>
    public static int SourceAddressOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileRomData.FirstSourceAddress,
        EscapeTimerTilePage.Second => EscapeTimerTileRomData.SecondSourceAddress,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {(int)page}."),
    };

    /// <summary>The page's native upload size in bytes.</summary>
    public static ushort ByteCountOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileAtlasFormat.FirstByteCount,
        EscapeTimerTilePage.Second => EscapeTimerTileAtlasFormat.SecondByteCount,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {(int)page}."),
    };

    /// <summary>The page's native VRAM word destination.</summary>
    public static ushort DestinationWordOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileAtlasFormat.FirstDestinationWord,
        EscapeTimerTilePage.Second => EscapeTimerTileAtlasFormat.SecondDestinationWord,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {(int)page}."),
    };

    /// <summary>The typed VRAM asset installed for the page.</summary>
    public static VramAssetId AssetOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => VramAssetId.EscapeTimerFirstTiles,
        EscapeTimerTilePage.Second => VramAssetId.EscapeTimerSecondTiles,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {(int)page}."),
    };

    /// <summary>True when a native source address is one of the pages' sources.</summary>
    public static bool IsSource(int sourceAddress)
    {
        foreach (EscapeTimerTilePage page in Pages)
            if (SourceAddressOf(page) == sourceAddress)
                return true;
        return false;
    }

    /// <summary>Identifies the page a native source/size descriptor uploads.</summary>
    public static bool TryMatch(int sourceAddress, int byteCount, out EscapeTimerTilePage page)
    {
        foreach (EscapeTimerTilePage candidate in Pages)
        {
            if (SourceAddressOf(candidate) == sourceAddress && ByteCountOf(candidate) == byteCount)
            {
                page = candidate;
                return true;
            }
        }
        page = default;
        return false;
    }

    /// <summary>Identifies the page a complete native transfer record uploads.</summary>
    public static bool TryMatch(int sourceAddress, int byteCount, ushort destinationWord, out EscapeTimerTilePage page) =>
        TryMatch(sourceAddress, byteCount, out page) && DestinationWordOf(page) == destinationWord;
}

/// <summary>Indexed-PNG and native OBJ transfer geometry for editable escape-timer characters.</summary>
public static class EscapeTimerTileAtlasFormat
{
    /// <summary>Indexed-PNG resource filename for the shared Ceres and Zebes escape-timer digit, separator, and TIME-label characters.</summary>
    public const string FileName = "escape-timer-tiles.png";
    /// <summary>Four planar bits per OBJ pixel; each native eight-by-eight character occupies 32 encoded bytes.</summary>
    public const int BitsPerPixel = 4;
    /// <summary>Sixteen possible pixel indices in the four-bit artwork; runtime OBJ palettes supply display colors rather than the PNG palette.</summary>
    public const int ColorCount = 16;
    /// <summary>Twenty-five eight-by-eight characters: ten upper digit halves, ten lower halves, two separators, then three TIME-label tiles.</summary>
    public const int TileCount = 25;
    /// <summary>PNG width in pixels, 200, laying all twenty-five native characters side by side in transfer order.</summary>
    public const int Width = TileCount * 8;
    /// <summary>PNG height in pixels, eight; a sixteen-pixel-tall digit is represented by separate upper and lower characters in the strip.</summary>
    public const int Height = 8;

    /// <summary>First native upload size, $0200 bytes, containing characters 0..15 from source $B0:C000.</summary>
    public const ushort FirstByteCount = 0x0200;
    /// <summary>First upload's VRAM word destination $7E00, equivalent to byte $FC00 and native timer OBJ character $1E0.</summary>
    public const ushort FirstDestinationWord = 0x7e00;

    /// <summary>Second native upload size, $0120 bytes, containing the remaining characters 16..24 from source $B0:C200.</summary>
    public const ushort SecondByteCount = 0x0120;
    /// <summary>Second upload's VRAM word destination $7F00, equivalent to byte $FE00 and native timer OBJ character $1F0.</summary>
    public const ushort SecondDestinationWord = 0x7f00;

    /// <summary>Complete planar strip size, $0320 bytes, preserving the native sixteen-character and nine-character transfer boundary.</summary>
    public const int TotalByteCount = FirstByteCount + SecondByteCount;
}
