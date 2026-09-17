using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable four-bit OBJ characters used by the Ceres and Zebes escape timers.</summary>
public sealed class EscapeTimerTileAtlas
{
    private readonly byte[] transfer;

    private EscapeTimerTileAtlas(byte[] transfer) => this.transfer = transfer;

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
        return new(planar);
    }

    /// <summary>Resolves one native page without combining or retiming its original transfer record.</summary>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => asset switch
    {
        VramAssetId.EscapeTimerFirstTiles => transfer.AsMemory(0, EscapeTimerTileAtlasFormat.FirstByteCount),
        VramAssetId.EscapeTimerSecondTiles => transfer.AsMemory(
            EscapeTimerTileAtlasFormat.FirstByteCount,
            EscapeTimerTileAtlasFormat.SecondByteCount),
        _ => throw new InvalidDataException($"Escape timer artwork cannot resolve VRAM asset {asset}."),
    };

    /// <summary>Queues both native records in their original order and at their original destinations.</summary>
    public void QueueTo(VramWriteQueue queue)
    {
        ArgumentNullException.ThrowIfNull(queue);
        if (transfer.Length != EscapeTimerTileAtlasFormat.TotalByteCount)
            throw new InvalidDataException("Escape timer artwork no longer matches its native transfer pages.");
        queue.EnqueueAsset(VramAssetId.EscapeTimerFirstTiles,
            EscapeTimerTileAtlasFormat.FirstByteCount, EscapeTimerTileAtlasFormat.FirstDestinationWord);
        queue.EnqueueAsset(VramAssetId.EscapeTimerSecondTiles,
            EscapeTimerTileAtlasFormat.SecondByteCount, EscapeTimerTileAtlasFormat.SecondDestinationWord);
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
