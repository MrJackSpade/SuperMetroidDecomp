using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The cartridge's five simple bank-$87 room-FX animated-tile objects use contiguous
/// frame artwork within each object. These addresses identify visual bytes only;
/// duration, loop control, transfer size and VRAM destination remain compiled in
/// <see cref="RoomFxAnimatedTileMechanicsDefinitions"/>.
/// </summary>
public static class RoomFxAnimatedTileArtworkDefinitions
{
    /// <summary>$87:91E4, four ceiling-sand frames selected by $87:8221.</summary>
    public const int MaridiaSandCeilingFirstSource = 0x8791e4;
    /// <summary>$87:9164, four falling-sand frames selected by $87:8235.</summary>
    public const int MaridiaSandFallingFirstSource = 0x879164;
    /// <summary>$87:A564, five lava frames selected by $87:8293.</summary>
    public const int LavaFirstSource = 0x87a564;
    /// <summary>$87:A6A4, five acid frames selected by $87:82B1.</summary>
    public const int AcidFirstSource = 0x87a6a4;
    /// <summary>$87:A874, five rain frames selected by $87:82CF.</summary>
    public const int RainFirstSource = 0x87a874;

    /// <summary>Returns the native source identity for one compiled frame cursor.</summary>
    public static int SourceAddress(RoomFxAnimatedTileObjectDefinition definition,
        ushort instructionPointer)
    {
        ArgumentNullException.ThrowIfNull(definition);
        int first = definition.ObjectPointer switch
        {
            AnimatedTileObjectPointers.MaridiaSandCeiling => MaridiaSandCeilingFirstSource,
            AnimatedTileObjectPointers.MaridiaSandFalling => MaridiaSandFallingFirstSource,
            AnimatedTileObjectPointers.Lava => LavaFirstSource,
            AnimatedTileObjectPointers.Acid => AcidFirstSource,
            AnimatedTileObjectPointers.Rain => RainFirstSource,
            _ => throw new InvalidDataException(
                $"No compiled artwork source for room-FX object $87:{definition.ObjectPointer:X4}."),
        };
        for (int index = 0; index < definition.Frames.Count; index++)
            if (definition.Frames[index].InstructionPointer == instructionPointer)
                return first + index * definition.TransferByteCount;
        throw new InvalidDataException(
            $"Room-FX object $87:{definition.ObjectPointer:X4} has no artwork frame at $87:{instructionPointer:X4}.");
    }
}

/// <summary>Editable two-bit characters for all 23 simple room-FX animation frames.</summary>
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
        if (offset != transfer.Length)
            throw new InvalidDataException(
                $"Room-FX artwork has {transfer.Length} bytes, expected {offset}.");
    }

    /// <summary>Compiles indexed pixels back into their ordered native 2-bpp DMA frames.</summary>
    public static RoomFxAnimatedTileAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(png,
            RoomFxAnimatedTileAtlasFormat.Width, RoomFxAnimatedTileAtlasFormat.Height);
        byte[] planar = SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height,
            RoomFxAnimatedTileAtlasFormat.BitsPerPixel);
        return new(planar);
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

/// <summary>One ordered strip of 89 native 2-bpp characters (23 animation frames).</summary>
public static class RoomFxAnimatedTileAtlasFormat
{
    public const string FileName = "room-fx-animated-tiles.png";
    public const int BitsPerPixel = 2;
    public const int ColorCount = 4;
    public const int TileCount = 89;
    public const int Width = TileCount * 8;
    public const int Height = 8;
    public const int TotalByteCount = TileCount * 16;
}
