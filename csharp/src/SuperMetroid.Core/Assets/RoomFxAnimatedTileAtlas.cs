using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable two-bit characters for simple room-FX and Wrecked Ship treadmill frames.</summary>
public sealed class RoomFxAnimatedTileAtlas : IRomArtworkSource
{
    private readonly byte[] transfer;
    private readonly Dictionary<int, (int Offset, int ByteCount)> frames;

    private RoomFxAnimatedTileAtlas(byte[] transfer)
    {
        this.transfer = transfer;
        frames = new();
        int offset = 0;
        foreach (RoomFxAnimatedTileObjectDefinition definition in
                 RoomFxAnimatedTileMechanicsDefinitions.All)
        foreach (RoomFxAnimatedTileFrameDefinition frame in definition.Frames)
        {
            int source = RoomFxAnimatedTileArtworkDefinitions.SourceAddress(
                definition, frame.InstructionPointer);
            if (!frames.TryAdd(source, (offset, definition.TransferByteCount)))
                throw new InvalidDataException($"Duplicate room-FX artwork source ${source:X6}.");
            offset += definition.TransferByteCount;
        }
        for (int frame = 0; frame < RoomFxAnimatedTileAtlasFormat.TreadmillFrameCount; frame++)
        {
            int source = WreckedShipTreadmillRomData.FrameSource(frame);
            if (!frames.TryAdd(source, (offset, WreckedShipTreadmillRomData.TransferByteCount)))
                throw new InvalidDataException($"Duplicate treadmill artwork source ${source:X6}.");
            offset += WreckedShipTreadmillRomData.TransferByteCount;
        }
        if (offset != transfer.Length)
            throw new InvalidDataException(
                $"Room-FX artwork has {transfer.Length} bytes, expected {offset}.");
    }

    /// <summary>Compiles indexed pixels back into their ordered native 2-bpp DMA frames.</summary>
    public static RoomFxAnimatedTileAtlas Load(Stream png,
        RoomFxAnimatedTileAtlas? stockForLegacyOverride = null)
    {
        ArgumentNullException.ThrowIfNull(png);
        long start = png.CanSeek ? png.Position : 0;
        try
        {
            IndexedPngImage image = IndexedPng.Read(png,
                RoomFxAnimatedTileAtlasFormat.Width, RoomFxAnimatedTileAtlasFormat.Height);
            return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height,
                RoomFxAnimatedTileAtlasFormat.BitsPerPixel));
        }
        catch (InvalidDataException) when (stockForLegacyOverride is not null && png.CanSeek)
        {
            // Earlier overrides contain the same 23 frames but predate the four
            // treadmill frames. Preserve every edited pixel and append only the
            // newly introduced frames from checked, current stock content.
            png.Position = start;
            IndexedPngImage legacy = IndexedPng.Read(png,
                RoomFxAnimatedTileAtlasFormat.LegacyWidth,
                RoomFxAnimatedTileAtlasFormat.Height);
            byte[] legacyPlanar = SnesPlanarTileEncoder.Encode(legacy.Pixels,
                legacy.Width, legacy.Height, RoomFxAnimatedTileAtlasFormat.BitsPerPixel);
            var combined = new byte[RoomFxAnimatedTileAtlasFormat.TotalByteCount];
            legacyPlanar.CopyTo(combined, 0);
            stockForLegacyOverride.transfer.AsSpan(legacyPlanar.Length).CopyTo(
                combined.AsSpan(legacyPlanar.Length));
            return new(combined);
        }
    }

    /// <summary>Returns one complete native frame, rejecting altered transfer geometry.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        if (!frames.TryGetValue(sourceAddress, out var frame))
        {
            data = default;
            return false;
        }
        if (byteCount != frame.ByteCount)
            throw new InvalidDataException(
                $"Room-FX artwork source ${sourceAddress:X6} requires {frame.ByteCount} bytes, not {byteCount}.");
        data = transfer.AsMemory(frame.Offset, frame.ByteCount);
        return true;
    }

    /// <summary>Publishes a selected installed frame through the synchronous liquid/rain owner.</summary>
    public void LoadFrame(SnesVram vram, int sourceAddress, ushort byteCount,
        ushort encodedVramDestination)
    {
        ArgumentNullException.ThrowIfNull(vram);
        if (!TryResolve(sourceAddress, byteCount, out ReadOnlyMemory<byte> data))
            throw new InvalidDataException(
                $"Room-FX frame ${sourceAddress:X6} is missing from installed artwork.");
        vram.ExecuteQueuedAssetWrite(data.Span, encodedVramDestination);
    }
}

/// <summary>One ordered strip of 97 native 2-bpp characters (27 animation frames).</summary>
public static class RoomFxAnimatedTileAtlasFormat
{
    public const string FileName = "room-fx-animated-tiles.png";
    public const int BitsPerPixel = 2;
    public const int ColorCount = 4;
    public const int LegacyTileCount = 89;
    public const int TreadmillFrameCount = 4;
    public const int TileCount = LegacyTileCount + TreadmillFrameCount * 2;
    public const int LegacyWidth = LegacyTileCount * 8;
    public const int Width = TileCount * 8;
    public const int Height = 8;
    public const int TotalByteCount = TileCount * 16;
}
