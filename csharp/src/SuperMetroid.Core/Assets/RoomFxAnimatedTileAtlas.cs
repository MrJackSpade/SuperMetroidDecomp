using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable two-bit characters for simple room-FX and Wrecked Ship treadmill frames.</summary>
public sealed class RoomFxAnimatedTileAtlas : IInstalledArtworkTransferSource
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
        // The statue program points into one shared contiguous art strip; different
        // statues may upload overlapping 64- or 128-byte windows of that strip.
        offset += TourianStatueAnimatedTileArtworkDefinitions.TransferByteCount;
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
            // Preserve previous user-edited PNGs from before either the treadmill
            // or statue extension; inherit only the newly introduced tail.
            foreach (int width in new[] {
                RoomFxAnimatedTileAtlasFormat.PreStatueWidth,
                RoomFxAnimatedTileAtlasFormat.LegacyWidth })
            {
                png.Position = start;
                try
                {
                    IndexedPngImage legacy = IndexedPng.Read(png, width,
                        RoomFxAnimatedTileAtlasFormat.Height);
                    byte[] legacyPlanar = SnesPlanarTileEncoder.Encode(legacy.Pixels,
                        legacy.Width, legacy.Height, RoomFxAnimatedTileAtlasFormat.BitsPerPixel);
                    var combined = new byte[RoomFxAnimatedTileAtlasFormat.TotalByteCount];
                    legacyPlanar.CopyTo(combined, 0);
                    stockForLegacyOverride.transfer.AsSpan(legacyPlanar.Length).CopyTo(
                        combined.AsSpan(legacyPlanar.Length));
                    return new(combined);
                }
                catch (InvalidDataException) { /* Try the older sheet geometry. */ }
            }
            throw new InvalidDataException("Room-FX PNG matches neither current nor supported legacy sheet geometry.");
        }
    }

    /// <summary>Returns one complete native frame, rejecting altered transfer geometry.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        if (!frames.TryGetValue(sourceAddress, out var frame))
        {
            foreach (TourianStatueAnimatedTileProgramDefinition definition in
                     TourianStatueAnimatedTileMechanicsDefinitions.All)
            foreach (ushort operand in definition.SourceOperandPointers)
            {
                if (TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(definition,
                        operand) != sourceAddress)
                    continue;
                if (byteCount != definition.TransferByteCount)
                    throw new InvalidDataException(
                        $"Tourian statue art ${sourceAddress:X6} requires {definition.TransferByteCount} bytes, not {byteCount}.");
                int offset = RoomFxAnimatedTileAtlasFormat.PreStatueTileCount * 16 +
                    sourceAddress - TourianStatueAnimatedTileArtworkDefinitions.FirstSource;
                data = transfer.AsMemory(offset, byteCount);
                return true;
            }
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

/// <summary>One ordered strip of native 2-bpp room-FX, treadmill, and statue characters.</summary>
public static class RoomFxAnimatedTileAtlasFormat
{
    public const string FileName = "room-fx-animated-tiles.png";
    public const int BitsPerPixel = 2;
    public const int ColorCount = 4;
    public const int LegacyTileCount = 89;
    public const int TreadmillFrameCount = 4;
    public const int PreStatueTileCount = LegacyTileCount + TreadmillFrameCount * 2;
    public const int StatueTileCount = TourianStatueAnimatedTileArtworkDefinitions.TransferByteCount / 16;
    public const int TileCount = PreStatueTileCount + StatueTileCount;
    public const int LegacyWidth = LegacyTileCount * 8;
    public const int PreStatueWidth = PreStatueTileCount * 8;
    public const int Width = TileCount * 8;
    public const int Height = 8;
    public const int TotalByteCount = TileCount * 16;
}
