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
    private readonly Dictionary<int, ushort> attributes = new();
    private readonly Dictionary<int, ushort> tileSources = new();
    private readonly RoomCharacterAtlas tiles;

    private SamusArmCannonArtworkCatalog(ushort[] posePointers, byte[] drawingData,
        ushort[] attributes, ushort[][] tileSources, RoomCharacterAtlas tiles)
    {
        this.posePointers = posePointers;
        this.drawingData = drawingData;
        for (int direction = 0; direction < attributes.Length; direction++)
        {
            if (attributes[direction] != SamusArmCannonArtworkFormat.StockSpriteAttributes((SamusProjectileDirection)direction))
                this.attributes.Add(direction, attributes[direction]);
            for (int frame = 0; frame < SamusArmCannonArtworkFormat.FramesPerDirection; frame++)
                if (tileSources[direction][frame] != SamusArmCannonArtworkFormat.StockTileSource((SamusProjectileDirection)direction, frame))
                    this.tileSources.Add(direction * SamusArmCannonArtworkFormat.FramesPerDirection + frame, tileSources[direction][frame]);
        }
        this.tiles = tiles;
    }

    /// <summary>SHA-256 of selected cannon placement, OBJ attributes, tile selectors and characters.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusArmCannonArtworkCatalog), content =>
    {
        content.AppendWords("pose pointers", this.posePointers);
        content.Append("drawing data", this.drawingData);
        Span<ushort> selectedAttributes = stackalloc ushort[SamusRenderingRomData.ArmCannon.DirectionCount];
        for (int direction = 0; direction < selectedAttributes.Length; direction++) selectedAttributes[direction] = SpriteAttributes(direction);
        content.AppendWords("attributes", selectedAttributes);
        Span<ushort> selectedSources = stackalloc ushort[SamusArmCannonArtworkFormat.FramesPerDirection];
        for (int direction = 0; direction < SamusRenderingRomData.ArmCannon.DirectionCount; direction++)
        {
            for (int frame = 0; frame < selectedSources.Length; frame++) selectedSources[frame] = TileSource(direction, frame);
            content.AppendWords("tile sources", selectedSources);
        }
        content.Append("characters", this.tiles.Transfer.Span);
    });

    public static SamusArmCannonArtworkCatalog Load(Stream json, Stream tilePng)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(tilePng);
        Placement placement = LoadPlacement(json);
        RoomCharacterAtlas tiles = RoomCharacterAtlas.Load(tilePng,
            SamusArmCannonArtworkFormat.TileSourcePointers.Length *
                SamusRenderingRomData.ArmCannon.TileUploadByteCount);
        return FromPlacement(placement, tiles);
    }

    // Placement and PNG admission are distinct file boundaries. The installer can
    // identify the failing file without attributing JSON errors to the tile sheet.
    internal sealed record Placement(ushort[] PosePointers, byte[] DrawingData,
        ushort[] Attributes, ushort[][] TileSources);

    internal static SamusArmCannonArtworkCatalog FromPlacement(Placement placement, RoomCharacterAtlas tiles) =>
        new(placement.PosePointers, placement.DrawingData, placement.Attributes, placement.TileSources, tiles);

    internal static Placement LoadPlacement(Stream json)
    {
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
        return new Placement(pointers, data, attributes, sources);
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

    public ushort SpriteAttributes(int direction)
    {
        if ((uint)direction >= SamusRenderingRomData.ArmCannon.DirectionCount) throw new IndexOutOfRangeException();
        return attributes.TryGetValue(direction, out ushort supplied) ? supplied
            : SamusArmCannonArtworkFormat.StockSpriteAttributes((SamusProjectileDirection)direction);
    }

    public ushort TileSource(int direction, int frame)
    {
        if ((uint)direction >= SamusRenderingRomData.ArmCannon.DirectionCount ||
            (uint)frame >= SamusArmCannonArtworkFormat.FramesPerDirection) throw new IndexOutOfRangeException();
        return tileSources.TryGetValue(direction * SamusArmCannonArtworkFormat.FramesPerDirection + frame, out ushort supplied) ? supplied
            : SamusArmCannonArtworkFormat.StockTileSource((SamusProjectileDirection)direction, frame);
    }

    /// <summary>Resolves the queued 32-byte VRAM DMA from the editable indexed PNG.</summary>
    public bool TryResolveTile(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> data)
    {
        if (byteCount == SamusRenderingRomData.ArmCannon.TileUploadByteCount)
        {
            int source = sourceAddress & 0xffff;
            if ((sourceAddress & 0xff0000) == SamusRenderingRomData.Banks.CharacterData &&
                SamusArmCannonArtworkFormat.TileSourcePointers.IndexOf(
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
    private enum TileOrientation
    {
        /// <summary>$90:C7B9 selects vertical frames from $9A:9A00.</summary>
        Vertical,
        /// <summary>$90:C7C1 selects horizontal frames from $9A:A000.</summary>
        Horizontal,
        /// <summary>$90:C7C9 selects downward-diagonal frames from $9A:A600.</summary>
        DownwardDiagonal,
        /// <summary>$90:C7D1 selects upward-diagonal frames from $9A:AC00.</summary>
        UpwardDiagonal,
    }
    /// <summary>$90:C791..C7A4 assigns OBJ character $1F, palette4 and priority2 to the cannon cover.</summary>
    private const ushort CoverSpriteIdentity = 0x001f | (4 << 9) | (2 << 12);
    /// <summary>$90:C791..C7A4 uses bit14 to reflect the selected cover artwork horizontally.</summary>
    private const ushort HorizontalReflection = 1 << 14;
    /// <summary>$90:C791..C7A4 uses bit15 to reflect the vertical source for downward aim.</summary>
    private const ushort VerticalReflection = 1 << 15;

    /// <summary>$90:C7A5..C7D8 chooses a physical artwork orientation and three successive opening frames; frame0 is the closed no-transfer sentinel.</summary>
    internal static ushort StockTileSource(SamusProjectileDirection direction, int frame)
    {
        if (frame == 0) return 0;
        TileOrientation orientation = direction switch
        {
            SamusProjectileDirection.UpRight or SamusProjectileDirection.UpLeft => TileOrientation.UpwardDiagonal,
            SamusProjectileDirection.Right or SamusProjectileDirection.Left => TileOrientation.Horizontal,
            SamusProjectileDirection.DownRight or SamusProjectileDirection.DownLeft => TileOrientation.DownwardDiagonal,
            _ => TileOrientation.Vertical,
        };
        return TileSourcePointers[(int)orientation * (FramesPerDirection - 1) + frame - 1];
    }

    /// <summary>$90:C791..C7A4 reflects the selected vertical/horizontal/diagonal artwork according to native aiming direction.</summary>
    internal static ushort StockSpriteAttributes(SamusProjectileDirection direction)
    {
        bool horizontal = direction is SamusProjectileDirection.DownRight or SamusProjectileDirection.DownFacingLeft
            or SamusProjectileDirection.Left or SamusProjectileDirection.UpLeft or SamusProjectileDirection.UpFacingLeft;
        bool vertical = direction is SamusProjectileDirection.DownFacingRight or SamusProjectileDirection.DownFacingLeft;
        return (ushort)(CoverSpriteIdentity | (horizontal ? HorizontalReflection : 0) | (vertical ? VerticalReflection : 0));
    }
    /// <summary>$9A:9A00, Tiles_NonClosed_ArmCannon_Vertical_0, first of twelve cover tile sources.</summary>
    private const int FirstTileSource = 0x9a00;
    /// <summary>The twelve native cover sources from $9A:9A00 through $9A:B000 occupy successive $200-byte character blocks.</summary>
    private const int TileSourceStride = 0x0200;
    /// <summary>Twelve source identities selected by the four three-frame lists at $90:C7B9..C7D8.</summary>
    public static TileSourceSequence TileSourcePointers => new();

    /// <summary>Calculated source identities in the original exported tile order; no tile pixels are inferred.</summary>
    public readonly record struct TileSourceSequence : IReadOnlyList<ushort>
    {
        public int Count => 12;
        public int Length => Count;
        public ushort this[int index] => (uint)index < Count
            ? (ushort)(FirstTileSource + index * TileSourceStride)
            : throw new IndexOutOfRangeException();
        public int IndexOf(ushort source)
        {
            int index = (source - FirstTileSource) / TileSourceStride;
            return (uint)index < Count && this[index] == source ? index : -1;
        }
        public bool Contains(ushort source) => IndexOf(source) >= 0;
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
