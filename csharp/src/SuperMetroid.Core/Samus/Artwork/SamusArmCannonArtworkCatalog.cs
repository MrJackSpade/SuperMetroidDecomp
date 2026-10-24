using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Installed arm-cannon cover placement, OAM attributes, and indexed 8×8 tiles.</summary>
public sealed class SamusArmCannonArtworkCatalog
{
    /// <summary>Tile-source slots: one row per native aim direction, each holding every cover-art frame slot.</summary>
    private static readonly FixedGridShape CoverTileShape =
        new(SamusRenderingRomData.ArmCannon.DirectionCount, OpeningFrames);
    /// <summary>Cover frames that show a tile: every frame but the closed one.</summary>
    private const int OpeningFrames = SamusArmCannonArtworkFormat.FramesPerDirection - 1;
    /// <summary>Pose descriptor offsets that differ from the native pose table.</summary>
    internal readonly Dictionary<int, int> poseDescriptors = new();
    /// <summary>Descriptor bytes retained when an edit or independent value cannot be derived.</summary>
    internal readonly Dictionary<int, byte> drawingData = new();
    /// <summary>Per-direction OBJ attribute words that differ from the stock orientation rules.</summary>
    internal readonly Dictionary<int, ushort> attributes = new();
    /// <summary>Per-direction opening cover tiles that differ from stock lists.</summary>
    internal readonly Dictionary<int, int> openingTiles = new();
    /// <summary>Installed indexed PNG characters for the twelve nonclosed cover tiles.</summary>
    private readonly RoomCharacterAtlas tiles;
    /// <summary>Optional body geometry used to derive cover attachment coordinates.</summary>
    private readonly SamusBodyArtworkCatalog? bodyGeometry;

    /// <summary>Separates authored edits from native descriptor, placement, and tile-selection rules.</summary>
    private SamusArmCannonArtworkCatalog(int[] poseDescriptors, byte[] drawingData,
        ushort[] attributes, int[][] coverTiles, RoomCharacterAtlas tiles,
        SamusBodyArtworkCatalog? bodyGeometry = null)
    {
        this.bodyGeometry = bodyGeometry;
        for (int pose = 0; pose < poseDescriptors.Length; pose++)
            if (poseDescriptors[pose] != SamusArmCannonArtworkFormat.StockPoseDescriptor(pose))
                this.poseDescriptors.Add(pose, poseDescriptors[pose]);
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
            for (int frame = 1; frame <= OpeningFrames; frame++)
                if (coverTiles[direction][frame - 1] != SamusArmCannonArtworkFormat.StockCoverTile((SamusProjectileDirection)direction, frame))
                    this.openingTiles.Add(direction * OpeningFrames + frame - 1, coverTiles[direction][frame - 1]);
        }
        this.tiles = tiles;
    }

    /// <summary>Rebinds cover coordinates to installed Samus body geometry while preserving selected edits.</summary>
    internal SamusArmCannonArtworkCatalog WithBodyGeometry(SamusBodyArtworkCatalog body) => new(
        Enumerable.Range(0, SamusBodyArtworkCatalog.PoseCount).Select(PoseDescriptor).ToArray(),
        Enumerable.Range(0, SamusArmCannonArtworkFormat.DrawingDataByteCount).Select(ReadDrawingByte).ToArray(),
        Enumerable.Range(0, SamusRenderingRomData.ArmCannon.DirectionCount).Select(SpriteAttributes).ToArray(),
        Enumerable.Range(0, SamusRenderingRomData.ArmCannon.DirectionCount).Select(direction =>
            Enumerable.Range(1, OpeningFrames).Select(frame => OpeningTile(direction, frame)).ToArray()).ToArray(),
        tiles, body);

    /// <summary>SHA-256 of selected cannon placement, OBJ attributes, tile selectors and characters.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("samus-arm-cannon-v2", content =>
    {
        for (int pose = 0; pose < SamusBodyArtworkCatalog.PoseCount; pose++) content.Append("pose descriptor", PoseDescriptor(pose));
        Span<byte> selectedDrawing = stackalloc byte[SamusArmCannonArtworkFormat.DrawingDataByteCount];
        for (int index = 0; index < selectedDrawing.Length; index++)
            selectedDrawing[index] = ReadDrawingByte(index);
        content.Append("drawing data", selectedDrawing);
        Span<ushort> selectedAttributes = stackalloc ushort[SamusRenderingRomData.ArmCannon.DirectionCount];
        for (int direction = 0; direction < selectedAttributes.Length; direction++) selectedAttributes[direction] = SpriteAttributes(direction);
        content.AppendWords("attributes", selectedAttributes);
        for (int direction = 0; direction < SamusRenderingRomData.ArmCannon.DirectionCount; direction++)
            for (int frame = 1; frame <= OpeningFrames; frame++)
                content.Append("cover tile", OpeningTile(direction, frame));
        content.Append("characters", this.tiles.Characters.Span);
    });

    /// <summary>Validated JSON placement tables before their separately loaded tile PNG is attached.</summary>
    /// <remarks>Placement and PNG admission remain distinct so installation identifies the failing file.</remarks>
    internal sealed record Placement(int[] PoseDescriptors, byte[] DrawingData,
        ushort[] Attributes, int[][] CoverTiles);

    /// <summary>Combines validated descriptor tables with the installed indexed tile sheet.</summary>
    internal static SamusArmCannonArtworkCatalog FromPlacement(Placement placement, RoomCharacterAtlas tiles) =>
        new(placement.PoseDescriptors, placement.DrawingData, placement.Attributes, placement.CoverTiles, tiles);

    /// <summary>Parses arm-cannon JSON and validates descriptor, attribute, and tile-source geometry.</summary>
    internal static Placement LoadPlacement(Stream json)
    {
        SamusArmCannonArtworkDocument document;
        document = JsonAssetDocument.Read<SamusArmCannonArtworkDocument>(json, Options, "arm-cannon artwork");
        if (document.Version != SamusArmCannonArtworkFormat.Version ||
            document.PoseDescriptors is null ||
            document.PoseDescriptors.Length != SamusBodyArtworkCatalog.PoseCount ||
            document.DrawingData is null ||
            document.DrawingData.Length != SamusArmCannonArtworkFormat.DrawingDataByteCount ||
            document.SpriteAttributes is null ||
            document.SpriteAttributes.Length != SamusRenderingRomData.ArmCannon.DirectionCount ||
            !CoverTileShape.HasRows(document.CoverTiles))
            throw new InvalidDataException("Arm-cannon artwork has an invalid version or table geometry.");

        int[] descriptors = [.. document.PoseDescriptors];
        foreach (int descriptor in descriptors)
            if (descriptor < 0 || descriptor + 2 >= SamusArmCannonArtworkFormat.DrawingDataByteCount)
                throw new InvalidDataException(
                    $"Arm-cannon pose descriptor {descriptor} lies outside drawing data.");
        byte[] data = document.DrawingData.Select((value, index) =>
            RequireByte(value, $"drawing byte {index}")).ToArray();
        ushort[] attributes = document.SpriteAttributes.Select((value, index) =>
            RequireWord(value, $"attribute {index}")).ToArray();
        int[][] coverTiles = new int[document.CoverTiles.Length][];
        for (int direction = 0; direction < coverTiles.Length; direction++)
        {
            int[]? row = document.CoverTiles[direction];
            if (!CoverTileShape.HasColumns(row))
                throw new InvalidDataException(
                    $"Arm-cannon direction {direction} needs three opening cover tiles.");
            if (row.Any(tile => (uint)tile >= SamusArmCannonArtworkFormat.CoverTileCount))
                throw new InvalidDataException(
                    $"Arm-cannon direction {direction} selects a missing tile.");
            coverTiles[direction] = [.. row];
        }
        return new Placement(descriptors, data, attributes, coverTiles);
    }

    /// <summary>Serializes the placement document as indented camel-case UTF-8 JSON for <c>samus-arm-cannon.json</c>; table bounds and selector validation occur when placement is loaded, not here.</summary>
    public static byte[] Write(SamusArmCannonArtworkDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        return json;
    }

    /// <summary>Gets where pose 0–252's drawing descriptor begins within the drawing data, using an independent supplied edit or the stock pose-table mapping.</summary>
    public int PoseDescriptor(int pose)
    {
        if ((uint)pose >= SamusBodyArtworkCatalog.PoseCount)
            throw new ArgumentOutOfRangeException(nameof(pose));
        return poseDescriptors.TryGetValue(pose, out int selected) ? selected
            : SamusArmCannonArtworkFormat.StockPoseDescriptor(pose);
    }

    /// <summary>Resolves the installed descriptor byte <paramref name="offset"/> bytes into the drawing data, preferring edits over stock selector/mode bytes and derived body-relative or reflected coordinate bytes.</summary>
    /// <param name="offset">Byte offset into the drawing data; coordinate bytes are interpreted as signed pixel offsets by the renderer.</param>
    public byte ReadDrawingByte(int offset)
    {
        if ((uint)offset >= SamusArmCannonArtworkFormat.DrawingDataByteCount)
            throw new InvalidDataException(
                $"Arm-cannon drawing byte {offset} is not installed.");
        if (drawingData.TryGetValue(offset, out byte supplied)) return supplied;
        ushort address = (ushort)(SamusArmCannonArtworkFormat.DrawingDataStart + offset);
        if (SamusArmCannonArtworkFormat.TryStockDrawingByte(address, out byte calculated)) return calculated;
        if (SamusArmCannonArtworkFormat.TryStockCoordinateSource(address, out ushort source))
            return ReadDrawingByte(source - SamusArmCannonArtworkFormat.DrawingDataStart);
        if (SamusArmCannonArtworkFormat.TryStockReflectedXSource(address, out source))
            return SamusArmCannonArtworkFormat.ReflectCoverX(ReadDrawingByte(source - SamusArmCannonArtworkFormat.DrawingDataStart));
        if (bodyGeometry is not null && SamusArmCannonPlacementDefinitions.TryCoordinate(bodyGeometry, address, out byte coordinate))
            return coordinate;
        throw new InvalidDataException("Arm-cannon coordinate basis is incomplete.");
    }

    /// <summary>Gets the packed small-OBJ character, palette, priority, and flip attributes for native direction 0–9; supplied words override the stock $90:C791 table.</summary>
    public ushort SpriteAttributes(int direction)
    {
        if ((uint)direction >= SamusRenderingRomData.ArmCannon.DirectionCount) throw new ArgumentOutOfRangeException(nameof(direction));
        return attributes.TryGetValue(direction, out ushort supplied) ? supplied
            : SamusArmCannonArtworkFormat.StockSpriteAttributes((SamusProjectileDirection)direction);
    }

    /// <summary>The installed cover tile native direction 0–9 shows at cover frame 0–3; the closed frame 0 shows none.</summary>
    public int? CoverTileIndex(int direction, int frame)
    {
        if ((uint)direction >= SamusRenderingRomData.ArmCannon.DirectionCount)
            throw new ArgumentOutOfRangeException(nameof(direction));
        if ((uint)frame >= SamusArmCannonArtworkFormat.FramesPerDirection)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return frame == 0 ? null : OpeningTile(direction, frame);
    }

    /// <summary>The installed cover tile <paramref name="direction"/> shows at opening frame 1–3.</summary>
    private int OpeningTile(int direction, int frame) =>
        openingTiles.TryGetValue(direction * OpeningFrames + frame - 1, out int supplied) ? supplied
            : SamusArmCannonArtworkFormat.StockCoverTile((SamusProjectileDirection)direction, frame);

    /// <summary>The cover character for native direction 0-9 and opening frame 1-3, as owned artwork.</summary>
    public SpriteArtwork CoverTile(int direction, int frame)
    {
        int index = CoverTileIndex(direction, frame) ??
            throw new InvalidDataException($"Arm-cannon direction {direction} frame {frame} names no installed cover tile.");
        return coverTiles.GetOrAdd(index, key => new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.SamusArmCannonCover, key),
            tiles.Characters.Span.Slice(key * SamusRenderingRomData.ArmCannon.TileByteCount, SamusRenderingRomData.ArmCannon.TileByteCount)));
    }

    /// <summary>Cover characters already built, by installed tile index.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<int, SpriteArtwork> coverTiles = new();

    /// <summary>Resolves the queued 32-byte VRAM DMA from the editable indexed PNG.</summary>
    public bool TryResolveTile(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> data)
    {
        if (byteCount == SamusRenderingRomData.ArmCannon.TileByteCount)
        {
            int source = SnesAddressMath.OffsetInBank(sourceAddress);
            if ((SnesAddressMath.BankBase(sourceAddress)) == SamusRenderingRomData.Banks.CharacterData &&
                SamusArmCannonArtworkFormat.CoverTileDmaSources.IndexOf(
                    unchecked((ushort)source)) is int index and >= 0)
            {
                data = tiles.Characters.Slice(index * byteCount, byteCount);
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

    /// <summary>Rejects an authored attribute outside one unsigned word.</summary>
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
