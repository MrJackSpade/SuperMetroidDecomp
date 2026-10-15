using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Installed arm-cannon cover placement, OAM attributes, and indexed 8×8 tiles.</summary>
public sealed class SamusArmCannonArtworkCatalog
{
    /// <summary>Tile-source slots: one row per native aim direction, each holding every cover-art frame slot.</summary>
    private static readonly FixedGridShape TileSourceShape =
        new(SamusRenderingRomData.ArmCannon.DirectionCount, SamusArmCannonArtworkFormat.FramesPerDirection);
    /// <summary>Pose descriptor pointers that differ from the native pose table.</summary>
    internal readonly Dictionary<int, ushort> posePointers = new();
    /// <summary>Descriptor bytes retained when an edit or independent value cannot be derived.</summary>
    internal readonly Dictionary<int, byte> drawingData = new();
    /// <summary>Per-direction OBJ attribute words that differ from the stock orientation rules.</summary>
    internal readonly Dictionary<int, ushort> attributes = new();
    /// <summary>Per-direction cover tile source selections that differ from stock lists.</summary>
    internal readonly Dictionary<int, ushort> tileSources = new();
    /// <summary>Installed indexed PNG characters for the twelve nonclosed cover tiles.</summary>
    private readonly RoomCharacterAtlas tiles;
    /// <summary>Optional body geometry used to derive cover attachment coordinates.</summary>
    private readonly SamusBodyArtworkCatalog? bodyGeometry;

    /// <summary>Separates authored edits from native pointer, placement, and tile-selection rules.</summary>
    private SamusArmCannonArtworkCatalog(ushort[] posePointers, byte[] drawingData,
        ushort[] attributes, ushort[][] tileSources, RoomCharacterAtlas tiles,
        SamusBodyArtworkCatalog? bodyGeometry = null)
    {
        this.bodyGeometry = bodyGeometry;
        for (int pose = 0; pose < posePointers.Length; pose++)
            if (posePointers[pose] != SamusArmCannonArtworkFormat.StockPoseDrawingData(pose))
                this.posePointers.Add(pose, posePointers[pose]);
        for (int index = 0; index < drawingData.Length; index++)
        {
            ushort address = (ushort)(SamusArmCannonArtworkFormat.DrawingDataStart + index);
            bool derived = SamusArmCannonArtworkFormat.TryStockDrawingByte(address, out byte calculated);
            if (!derived && SamusArmCannonArtworkFormat.TryStockCoordinateSource(address, out ushort source))
            {
                calculated = drawingData[source - SamusArmCannonArtworkFormat.DrawingDataStart];
                derived = true;
            }
            if (!derived && SamusArmCannonArtworkFormat.TryStockReflectedXSource(address, out source))
            {
                calculated = SamusArmCannonArtworkFormat.ReflectCoverX(drawingData[source - SamusArmCannonArtworkFormat.DrawingDataStart]);
                derived = true;
            }
            if (!derived && bodyGeometry is not null)
                derived = SamusArmCannonPlacementDefinitions.TryCoordinate(bodyGeometry, address, out calculated);
            if (!derived || drawingData[index] != calculated) this.drawingData.Add(index, drawingData[index]);
        }
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

    /// <summary>Rebinds cover coordinates to installed Samus body geometry while preserving selected edits.</summary>
    internal SamusArmCannonArtworkCatalog WithBodyGeometry(SamusBodyArtworkCatalog body) => new(
        Enumerable.Range(0, SamusBodyArtworkCatalog.PoseCount).Select(PoseDrawingData).ToArray(),
        Enumerable.Range(0, SamusArmCannonArtworkFormat.DrawingDataByteCount)
            .Select(index => ReadDrawingByte((ushort)(SamusArmCannonArtworkFormat.DrawingDataStart + index))).ToArray(),
        Enumerable.Range(0, SamusRenderingRomData.ArmCannon.DirectionCount).Select(SpriteAttributes).ToArray(),
        Enumerable.Range(0, SamusRenderingRomData.ArmCannon.DirectionCount).Select(direction =>
            Enumerable.Range(0, SamusArmCannonArtworkFormat.FramesPerDirection).Select(frame => TileSource(direction, frame)).ToArray()).ToArray(),
        tiles, body);

    /// <summary>SHA-256 of selected cannon placement, OBJ attributes, tile selectors and characters.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusArmCannonArtworkCatalog), content =>
    {
        Span<ushort> selectedPoses = stackalloc ushort[SamusBodyArtworkCatalog.PoseCount];
        for (int pose = 0; pose < selectedPoses.Length; pose++) selectedPoses[pose] = PoseDrawingData(pose);
        content.AppendWords("pose pointers", selectedPoses);
        Span<byte> selectedDrawing = stackalloc byte[SamusArmCannonArtworkFormat.DrawingDataByteCount];
        for (int index = 0; index < selectedDrawing.Length; index++)
            selectedDrawing[index] = ReadDrawingByte((ushort)(SamusArmCannonArtworkFormat.DrawingDataStart + index));
        content.Append("drawing data", selectedDrawing);
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

    /// <summary>Validated JSON placement tables before their separately loaded tile PNG is attached.</summary>
    /// <remarks>Placement and PNG admission remain distinct so installation identifies the failing file.</remarks>
    internal sealed record Placement(ushort[] PosePointers, byte[] DrawingData,
        ushort[] Attributes, ushort[][] TileSources);

    /// <summary>Combines validated descriptor tables with the installed indexed tile sheet.</summary>
    internal static SamusArmCannonArtworkCatalog FromPlacement(Placement placement, RoomCharacterAtlas tiles) =>
        new(placement.PosePointers, placement.DrawingData, placement.Attributes, placement.TileSources, tiles);

    /// <summary>Parses arm-cannon JSON and validates descriptor, attribute, and tile-source geometry.</summary>
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
            !TileSourceShape.HasRows(document.TileSources))
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
            if (!TileSourceShape.HasColumns(native))
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

    /// <summary>Serializes the placement document as indented camel-case UTF-8 JSON for <c>samus-arm-cannon.json</c>; table bounds and selector validation occur when placement is loaded, not here.</summary>
    public static byte[] Write(SamusArmCannonArtworkDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        return json;
    }

    /// <summary>Gets the bank-$90 drawing-descriptor pointer for pose 0–252, using an independent supplied edit or the stock $90:C7DF pose-table mapping.</summary>
    public ushort PoseDrawingData(int pose)
    {
        if ((uint)pose >= SamusBodyArtworkCatalog.PoseCount)
            throw new ArgumentOutOfRangeException(nameof(pose));
        return posePointers.TryGetValue(pose, out ushort selected) ? selected
            : SamusArmCannonArtworkFormat.StockPoseDrawingData(pose);
    }

    /// <summary>Resolves one installed descriptor byte at bank-$90 address $C9D9–$CC38, preferring edits over stock selector/mode bytes and derived body-relative or reflected coordinate bytes.</summary>
    /// <param name="address">Bank-relative descriptor byte address; coordinate bytes are interpreted as signed pixel offsets by the renderer.</param>
    public byte ReadDrawingByte(ushort address)
    {
        int index = address - SamusArmCannonArtworkFormat.DrawingDataStart;
        if ((uint)index >= SamusArmCannonArtworkFormat.DrawingDataByteCount)
            throw new InvalidDataException(
                $"Arm-cannon drawing byte $90:{address:X4} is not installed.");
        if (drawingData.TryGetValue(index, out byte supplied)) return supplied;
        if (SamusArmCannonArtworkFormat.TryStockDrawingByte(address, out byte calculated)) return calculated;
        if (SamusArmCannonArtworkFormat.TryStockCoordinateSource(address, out ushort source)) return ReadDrawingByte(source);
        if (SamusArmCannonArtworkFormat.TryStockReflectedXSource(address, out source))
            return SamusArmCannonArtworkFormat.ReflectCoverX(ReadDrawingByte(source));
        if (bodyGeometry is not null && SamusArmCannonPlacementDefinitions.TryCoordinate(bodyGeometry, address, out byte coordinate))
            return coordinate;
        throw new InvalidDataException("Arm-cannon coordinate basis is incomplete.");
    }

    /// <summary>Gets the packed small-OBJ character, palette, priority, and flip attributes for native direction 0–9; supplied words override the stock $90:C791 table.</summary>
    public ushort SpriteAttributes(int direction)
    {
        if ((uint)direction >= SamusRenderingRomData.ArmCannon.DirectionCount) throw new IndexOutOfRangeException();
        return attributes.TryGetValue(direction, out ushort supplied) ? supplied
            : SamusArmCannonArtworkFormat.StockSpriteAttributes((SamusProjectileDirection)direction);
    }

    /// <summary>Gets the bank-$9A tile-source identity for native direction 0–9 and cover frame 0–3; frame 0 is the closed no-transfer sentinel, while frames 1–3 select installed opening artwork.</summary>
    public ushort TileSource(int direction, int frame)
    {
        if ((uint)direction >= SamusRenderingRomData.ArmCannon.DirectionCount ||
            (uint)frame >= SamusArmCannonArtworkFormat.FramesPerDirection) throw new IndexOutOfRangeException();
        return tileSources.TryGetValue(direction * SamusArmCannonArtworkFormat.FramesPerDirection + frame, out ushort supplied) ? supplied
            : SamusArmCannonArtworkFormat.StockTileSource((SamusProjectileDirection)direction, frame);
    }

    /// <summary>The cover character for native direction 0-9 and opening frame 1-3, as owned artwork.</summary>
    public SpriteArtwork CoverTile(int direction, int frame)
    {
        int index = SamusArmCannonArtworkFormat.TileSourcePointers.IndexOf(TileSource(direction, frame));
        if (index < 0)
            throw new InvalidDataException($"Arm-cannon direction {direction} frame {frame} names no installed cover tile.");
        return coverTiles.GetOrAdd(index, key => new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.SamusArmCannonCover, key),
            tiles.Transfer.Span.Slice(key * SamusRenderingRomData.ArmCannon.TileUploadByteCount, SamusRenderingRomData.ArmCannon.TileUploadByteCount)));
    }

    /// <summary>Cover characters already built, by installed tile index.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<int, SpriteArtwork> coverTiles = new();

    /// <summary>Resolves the queued 32-byte VRAM DMA from the editable indexed PNG.</summary>
    public bool TryResolveTile(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> data)
    {
        if (byteCount == SamusRenderingRomData.ArmCannon.TileUploadByteCount)
        {
            int source = SnesAddressMath.OffsetInBank(sourceAddress);
            if ((SnesAddressMath.BankBase(sourceAddress)) == SamusRenderingRomData.Banks.CharacterData &&
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

    /// <summary>Rejects an authored drawing value outside one unsigned byte.</summary>
    private static byte RequireByte(int value, string name) => (uint)value <= byte.MaxValue
        ? (byte)value
        : throw new InvalidDataException($"Arm-cannon {name} is not a byte.");

    /// <summary>Rejects an authored pointer or attribute outside one unsigned word.</summary>
    private static ushort RequireWord(int value, string name) => (uint)value <= ushort.MaxValue
        ? (ushort)value
        : throw new InvalidDataException($"Arm-cannon {name} is not a word.");

    /// <summary>Strict camel-case options for loading and writing placement JSON.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
