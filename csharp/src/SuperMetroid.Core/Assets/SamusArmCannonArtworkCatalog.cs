using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Installed arm-cannon cover placement, OAM attributes, and indexed 8×8 tiles.</summary>
public sealed class SamusArmCannonArtworkCatalog
{
    private readonly ushort[] posePointers;
    private readonly byte[] drawingData;
    private readonly ushort[] attributes;
    private readonly ushort[][] tileSources;
    private readonly RoomCharacterAtlas tiles;

    private SamusArmCannonArtworkCatalog(ushort[] posePointers, byte[] drawingData,
        ushort[] attributes, ushort[][] tileSources, RoomCharacterAtlas tiles)
    {
        this.posePointers = posePointers;
        this.drawingData = drawingData;
        this.attributes = attributes;
        this.tileSources = tileSources;
        this.tiles = tiles;
    }

    public static SamusArmCannonArtworkCatalog Load(Stream json, Stream tilePng)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(tilePng);
        SamusArmCannonArtworkDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            EnemySpritemapCatalog.RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<SamusArmCannonArtworkDocument>(Options)
                ?? throw new InvalidDataException("Arm-cannon artwork JSON is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid arm-cannon artwork JSON.", error);
        }
        if (document.Version != SamusArmCannonArtworkFormat.Version ||
            document.PosePointers is null ||
            document.PosePointers.Length != SamusBodyArtworkCatalog.PoseCount ||
            document.DrawingData is null ||
            document.DrawingData.Length != SamusArmCannonArtworkFormat.DrawingDataByteCount ||
            document.SpriteAttributes is null ||
            document.SpriteAttributes.Length != SamusRenderingRomData.ArmCannon.DirectionCount ||
            document.TileSources is null ||
            document.TileSources.Length != SamusRenderingRomData.ArmCannon.DirectionCount)
            throw new InvalidDataException("Arm-cannon artwork has an invalid version or table geometry.");

        ushort[] pointers = document.PosePointers.Select((value, pose) =>
            RequireWord(value, $"pose {pose} pointer")).ToArray();
        foreach (ushort pointer in pointers)
            if (pointer < SamusArmCannonArtworkFormat.DrawingDataStart ||
                pointer + 2 >= SamusArmCannonArtworkFormat.DrawingDataEndExclusive)
                throw new InvalidDataException(
                    $"Arm-cannon pose points outside drawing data: ${pointer:X4}.");
        byte[] data = document.DrawingData.Select((value, index) =>
            RequireByte(value, $"drawing byte {index}")).ToArray();
        ushort[] attributes = document.SpriteAttributes.Select((value, index) =>
            RequireWord(value, $"attribute {index}")).ToArray();
        ushort[][] sources = new ushort[document.TileSources.Length][];
        for (int direction = 0; direction < sources.Length; direction++)
        {
            int[]? native = document.TileSources[direction];
            if (native is null || native.Length != SamusArmCannonArtworkFormat.FramesPerDirection)
                throw new InvalidDataException(
                    $"Arm-cannon direction {direction} needs four tile-source slots.");
            sources[direction] = native.Select((value, frame) =>
                RequireWord(value, $"tile source {direction}/{frame}")).ToArray();
            if (sources[direction][0] != 0 ||
                sources[direction].Skip(1).Any(source =>
                    !SamusArmCannonArtworkFormat.TileSourcePointers.Contains(source)))
                throw new InvalidDataException(
                    $"Arm-cannon direction {direction} selects a missing tile.");
        }
        RoomCharacterAtlas tiles = RoomCharacterAtlas.Load(tilePng,
            SamusArmCannonArtworkFormat.TileSourcePointers.Length *
                SamusRenderingRomData.ArmCannon.TileUploadByteCount);
        return new SamusArmCannonArtworkCatalog(pointers, data, attributes,
            sources, tiles);
    }

    public static byte[] Write(SamusArmCannonArtworkDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        return json;
    }

    public ushort PoseDrawingData(int pose)
    {
        if ((uint)pose >= posePointers.Length)
            throw new ArgumentOutOfRangeException(nameof(pose));
        return posePointers[pose];
    }

    public byte ReadDrawingByte(ushort address)
    {
        int index = address - SamusArmCannonArtworkFormat.DrawingDataStart;
        if ((uint)index >= drawingData.Length)
            throw new InvalidDataException(
                $"Arm-cannon drawing byte $90:{address:X4} is not installed.");
        return drawingData[index];
    }

    public ushort SpriteAttributes(int direction) => attributes[direction];

    public ushort TileSource(int direction, int frame) => tileSources[direction][frame];

    /// <summary>Resolves the queued 32-byte VRAM DMA from the editable indexed PNG.</summary>
    public bool TryResolveTile(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> data)
    {
        if (byteCount == SamusRenderingRomData.ArmCannon.TileUploadByteCount)
        {
            int source = sourceAddress & 0xffff;
            if ((sourceAddress & 0xff0000) == SamusRenderingRomData.Banks.CharacterData &&
                Array.IndexOf(SamusArmCannonArtworkFormat.TileSourcePointers,
                    unchecked((ushort)source)) is int index and >= 0)
            {
                data = tiles.Transfer.Slice(index * byteCount, byteCount);
                return true;
            }
        }
        data = default;
        return false;
    }

    private static byte RequireByte(int value, string name) => (uint)value <= byte.MaxValue
        ? (byte)value
        : throw new InvalidDataException($"Arm-cannon {name} is not a byte.");

    private static ushort RequireWord(int value, string name) => (uint)value <= ushort.MaxValue
        ? (ushort)value
        : throw new InvalidDataException($"Arm-cannon {name} is not a word.");

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}

public sealed record SamusArmCannonArtworkDocument
{
    public required int Version { get; init; }
    public required int[] PosePointers { get; init; }
    public required int[] DrawingData { get; init; }
    public required int[] SpriteAttributes { get; init; }
    public required int[][] TileSources { get; init; }
}

/// <summary>Bounded retail arm-cannon visual geometry, separate from open/close mechanics.</summary>
public static class SamusArmCannonArtworkFormat
{
    public const int Version = 1;
    public const string JsonFileName = "samus-arm-cannon.json";
    public const string TileFileName = "samus-arm-cannon-tiles.png";
    /// <summary>First pose descriptor at $90:C9D9.</summary>
    public const ushort DrawingDataStart = 0xc9d9;
    /// <summary>Descriptor bytes end immediately before the $90:CC39 code entry.</summary>
    public const ushort DrawingDataEndExclusive = 0xcc39;
    public const int DrawingDataByteCount = DrawingDataEndExclusive - DrawingDataStart;
    public const int FramesPerDirection = 4;
    /// <summary>Twelve distinct bank-$9A 8×8 cover tiles reached through four native lists.</summary>
    public static readonly ushort[] TileSourcePointers =
    [
        0x9a00, 0x9c00, 0x9e00,
        0xa000, 0xa200, 0xa400,
        0xa600, 0xa800, 0xaa00,
        0xac00, 0xae00, 0xb000,
    ];
}
