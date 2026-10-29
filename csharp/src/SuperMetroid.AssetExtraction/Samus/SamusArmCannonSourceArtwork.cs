using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The arm cannon's installed artwork as the import pipeline holds it (#1463): the cartridge's descriptor bytes, cover
/// appearances and indexed 8×8 cover tiles, from which the overlay sheet is composed and each pose's typed placement is
/// decoded for gameplay.
/// </summary>
public sealed class SamusArmCannonSourceArtwork : InstalledSamusArmCannonArtwork
{
    /// <summary>Tile-source slots: one row per native aim direction, each holding every cover-art frame slot.</summary>
    private static readonly FixedGridShape CoverTileShape =
        new(SamusDrawingDefinitions.ArmCannon.DirectionCount, OpeningFrames);
    /// <summary>Cover frames that show a tile: every frame but the closed one.</summary>
    private const int OpeningFrames = SamusArmCannonArtworkFormat.FramesPerDirection - 1;
    /// <summary>The descriptor's flag byte bit that makes its direction change after animation frame zero.</summary>
    private const int FrameDependentDirectionFlag = 1 << 7;
    /// <summary>The bits of a descriptor's direction byte that name the direction.</summary>
    private const int DirectionBits = FrameDependentDirectionFlag - 1;
    /// <summary>Where a descriptor's drawing mode lies after its direction byte.</summary>
    private const int DrawingModeByte = 1;
    /// <summary>Where a frame-dependent descriptor's later direction lies after its direction byte.</summary>
    private const int LaterDirectionByte = 2;
    /// <summary>The bytes before a single-direction descriptor's displacements: direction and drawing mode.</summary>
    private const int SingleDirectionHeaderBytes = 2;
    /// <summary>The bytes before a frame-dependent descriptor's displacements: both directions with their mode bytes.</summary>
    private const int FrameDependentHeaderBytes = 4;
    /// <summary>The bytes of one animation frame's displacement: signed X, then signed Y.</summary>
    private const int DisplacementBytes = 2;
    /// <summary>Pose descriptor offsets that differ from the native pose table.</summary>
    internal readonly Dictionary<int, int> poseDescriptors = new();
    /// <summary>Descriptor bytes retained when an edit or independent value cannot be derived.</summary>
    internal readonly Dictionary<int, byte> drawingData = new();
    /// <summary>Per-direction cover appearances that differ from the stock orientation rules.</summary>
    internal readonly Dictionary<int, SpritePart> attributes = new();
    /// <summary>Per-direction opening cover tiles that differ from stock lists.</summary>
    internal readonly Dictionary<int, int> openingTiles = new();
    /// <summary>Installed indexed PNG characters for the twelve nonclosed cover tiles.</summary>
    private readonly RoomCharacterAtlas tiles;
    /// <summary>Optional body compositions used to derive cover attachment coordinates.</summary>
    private readonly SamusSpritemapSources? bodyGeometry;

    /// <summary>Separates authored edits from native descriptor, placement, and tile-selection rules.</summary>
    private SamusArmCannonSourceArtwork(int[] poseDescriptors, byte[] drawingData,
        SpritePart[] attributes, int[][] coverTiles, RoomCharacterAtlas tiles,
        SamusSpritemapSources? bodyGeometry = null)
        : base(Placements(poseDescriptors, drawingData))
    {
        this.bodyGeometry = bodyGeometry;
        for (int pose = 0; pose < poseDescriptors.Length; pose++)
            if (poseDescriptors[pose] != SamusArmCannonArtworkFormat.StockPoseDescriptor(pose))
                this.poseDescriptors.Add(pose, poseDescriptors[pose]);
        for (int index = 0; index < drawingData.Length; index++)
        {
            bool derived = SamusArmCannonArtworkFormat.TryStockDrawingByte(index, out int calculated);
            if (!derived && SamusArmCannonArtworkFormat.TryStockCoordinateSource(index, out int source))
            {
                calculated = drawingData[source];
                derived = true;
            }
            if (!derived && SamusArmCannonArtworkFormat.TryStockReflectedXSource(index, out source))
            {
                calculated = SamusArmCannonArtworkFormat.ReflectCoverX(drawingData[source]);
                derived = true;
            }
            if (!derived && bodyGeometry is not null)
                derived = SamusArmCannonPlacements.TryCoordinate(bodyGeometry, index, out calculated);
            if (!derived || drawingData[index] != calculated) this.drawingData.Add(index, drawingData[index]);
        }
        for (int direction = 0; direction < attributes.Length; direction++)
        {
            if (attributes[direction] != SamusArmCannonArtworkFormat.StockCoverAppearance(SamusProjectileDirectionCodec.FromAimIndex(direction)))
                this.attributes.Add(direction, attributes[direction]);
            for (int frame = 1; frame <= OpeningFrames; frame++)
                if (coverTiles[direction][frame - 1] != SamusArmCannonArtworkFormat.StockCoverTile(SamusProjectileDirectionCodec.FromAimIndex(direction), frame))
                    this.openingTiles.Add(direction * OpeningFrames + frame - 1, coverTiles[direction][frame - 1]);
        }
        this.tiles = tiles;
    }

    /// <summary>
    /// Decodes each pose's descriptor into its typed placement: the descriptor's direction byte, whose top bit makes
    /// the byte after the drawing mode the direction of every later animation frame, then the drawing mode, then one
    /// signed X and Y displacement per animation frame. Poses sharing a descriptor share its placement.
    /// </summary>
    private static SamusArmCannonPlacement[] Placements(int[] poseDescriptors, byte[] drawingData)
    {
        var placements = new Dictionary<int, SamusArmCannonPlacement>();
        return [.. poseDescriptors.Select(descriptor =>
        {
            if (placements.TryGetValue(descriptor, out SamusArmCannonPlacement? shared))
                return shared;
            int direction = drawingData[descriptor];
            bool frameDependent = (direction & FrameDependentDirectionFlag) != 0;
            int offsetsStart = descriptor + (frameDependent ? FrameDependentHeaderBytes : SingleDirectionHeaderBytes);
            int frames = Math.Max(0, (drawingData.Length - offsetsStart) / DisplacementBytes);
            var placement = new SamusArmCannonPlacement(drawingData[descriptor + DrawingModeByte], direction & DirectionBits,
                frameDependent ? drawingData[descriptor + LaterDirectionByte] & DirectionBits : null,
                Enumerable.Range(0, frames).Select(frame => offsetsStart + frame * DisplacementBytes).Select(at => Offset16<RoomPixels>.Of(
                    unchecked((sbyte)drawingData[at]), unchecked((sbyte)drawingData[at + 1]))));
            placements.Add(descriptor, placement);
            return placement;
        })];
    }

    /// <summary>Rebinds cover coordinates to installed Samus body compositions while preserving selected edits.</summary>
    internal SamusArmCannonSourceArtwork WithBodyGeometry(SamusSpritemapSources body) => new(
        Enumerable.Range(0, InstalledSamusBodyArtwork.PoseCount).Select(PoseDescriptor).ToArray(),
        Enumerable.Range(0, SamusArmCannonArtworkFormat.DrawingDataByteCount).Select(ReadDrawingByte).ToArray(),
        Enumerable.Range(0, SamusDrawingDefinitions.ArmCannon.DirectionCount).Select(CoverAppearance).ToArray(),
        Enumerable.Range(0, SamusDrawingDefinitions.ArmCannon.DirectionCount).Select(direction =>
            Enumerable.Range(1, OpeningFrames).Select(frame => OpeningTile(direction, frame)).ToArray()).ToArray(),
        tiles, Ensure.NotNull(body));

    /// <summary>SHA-256 of selected cannon placement, cover appearances, tile selectors and characters.</summary>
    public override string ContentIdentity => SelectedPresentationHash.Create("samus-arm-cannon-v3", content =>
    {
        for (int pose = 0; pose < InstalledSamusBodyArtwork.PoseCount; pose++) content.Append("pose descriptor", PoseDescriptor(pose));
        Span<byte> selectedDrawing = stackalloc byte[SamusArmCannonArtworkFormat.DrawingDataByteCount];
        for (int index = 0; index < selectedDrawing.Length; index++)
            selectedDrawing[index] = ReadDrawingByte(index);
        content.Append("drawing data", selectedDrawing);
        for (int direction = 0; direction < SamusDrawingDefinitions.ArmCannon.DirectionCount; direction++)
            CoverAppearance(direction).AppendIdentity(content);
        for (int direction = 0; direction < SamusDrawingDefinitions.ArmCannon.DirectionCount; direction++)
            for (int frame = 1; frame <= OpeningFrames; frame++)
                content.Append("cover tile", OpeningTile(direction, frame));
        content.Append("characters", this.tiles.Characters.Span);
    });

    /// <summary>Validated JSON placement tables before their separately loaded tile PNG is attached.</summary>
    /// <remarks>Placement and PNG admission remain distinct so installation identifies the failing file.</remarks>
    internal sealed record DescriptorTables(int[] PoseDescriptors, byte[] DrawingData,
        SpritePart[] Attributes, int[][] CoverTiles);

    /// <summary>Combines validated descriptor tables with the installed indexed tile sheet.</summary>
    internal static SamusArmCannonSourceArtwork FromPlacement(DescriptorTables placement, RoomCharacterAtlas tiles) =>
        new(placement.PoseDescriptors, placement.DrawingData, placement.Attributes, placement.CoverTiles, tiles);

    /// <summary>Parses arm-cannon JSON and validates descriptor, attribute, and tile-source geometry.</summary>
    internal static DescriptorTables LoadPlacement(Stream json)
    {
        SamusArmCannonArtworkDocument document;
        document = JsonAssetDocument.Read<SamusArmCannonArtworkDocument>(json, Options, "arm-cannon artwork");
        if (document.Version != SamusArmCannonArtworkFormat.Version ||
            document.PoseDescriptors is null ||
            document.PoseDescriptors.Length != InstalledSamusBodyArtwork.PoseCount ||
            document.DrawingData is null ||
            document.DrawingData.Length != SamusArmCannonArtworkFormat.DrawingDataByteCount ||
            document.CoverAppearances is null ||
            document.CoverAppearances.Length != SamusDrawingDefinitions.ArmCannon.DirectionCount ||
            !CoverTileShape.HasRows(document.CoverTiles))
            throw new InvalidDataException("Arm-cannon artwork has an invalid version or table geometry.");

        int[] descriptors = [.. document.PoseDescriptors];
        foreach (int descriptor in descriptors)
            if (descriptor < 0 || descriptor + 2 >= SamusArmCannonArtworkFormat.DrawingDataByteCount)
                throw new InvalidDataException(
                    $"Arm-cannon pose descriptor {descriptor} lies outside drawing data.");
        byte[] data = document.DrawingData.Select((value, index) =>
            RequireByte(value, $"drawing byte {index}")).ToArray();
        SpritePart[] attributes = document.CoverAppearances.Select((value, index) =>
            RequireAppearance(value, $"cover appearance {index}")).ToArray();
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
        return new DescriptorTables(descriptors, data, attributes, coverTiles);
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
        if ((uint)pose >= InstalledSamusBodyArtwork.PoseCount)
            throw new ArgumentOutOfRangeException(nameof(pose));
        return poseDescriptors.TryGetValue(pose, out int selected) ? selected
            : SamusArmCannonArtworkFormat.StockPoseDescriptor(pose);
    }

    /// <summary>Resolves the installed descriptor byte <paramref name="offset"/> bytes into the drawing data, preferring edits over stock selector/mode bytes and derived body-relative or reflected coordinate bytes.</summary>
    /// <param name="offset">Byte offset into the drawing data; coordinate bytes are signed pixel offsets.</param>
    public byte ReadDrawingByte(int offset)
    {
        if ((uint)offset >= SamusArmCannonArtworkFormat.DrawingDataByteCount)
            throw new InvalidDataException(
                $"Arm-cannon drawing byte {offset} is not installed.");
        if (drawingData.ContainsKey(offset)) return drawingData[offset];
        if (SamusArmCannonArtworkFormat.TryStockDrawingByte(offset, out int calculated)) return (byte)calculated;
        if (SamusArmCannonArtworkFormat.TryStockCoordinateSource(offset, out int source))
            return ReadDrawingByte(source);
        if (SamusArmCannonArtworkFormat.TryStockReflectedXSource(offset, out source))
            return SamusArmCannonArtworkFormat.ReflectCoverX(ReadDrawingByte(source));
        if (bodyGeometry is not null && SamusArmCannonPlacements.TryCoordinate(bodyGeometry, offset, out int coordinate))
            return (byte)coordinate;
        throw new InvalidDataException("Arm-cannon coordinate basis is incomplete.");
    }

    /// <summary>Gets the cover's character, palette, priority, and mirroring for native direction 0–9; supplied appearances override the stock $90:C791 table.</summary>
    public SpritePart CoverAppearance(int direction)
    {
        if ((uint)direction >= SamusDrawingDefinitions.ArmCannon.DirectionCount) throw new ArgumentOutOfRangeException(nameof(direction));
        return attributes.ContainsKey(direction) ? attributes[direction]
            : SamusArmCannonArtworkFormat.StockCoverAppearance(SamusProjectileDirectionCodec.FromAimIndex(direction));
    }

    /// <summary>The installed cover tile native direction 0–9 shows at cover frame 0–3; the closed frame 0 shows none.</summary>
    public int? CoverTileIndex(int direction, int frame)
    {
        if ((uint)direction >= SamusDrawingDefinitions.ArmCannon.DirectionCount)
            throw new ArgumentOutOfRangeException(nameof(direction));
        if ((uint)frame >= SamusArmCannonArtworkFormat.FramesPerDirection)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return frame == 0 ? null : OpeningTile(direction, frame);
    }

    /// <summary>The installed cover tile <paramref name="direction"/> shows at opening frame 1–3.</summary>
    private int OpeningTile(int direction, int frame) =>
        openingTiles.TryGetValue(direction * OpeningFrames + frame - 1, out int supplied) ? supplied
            : SamusArmCannonArtworkFormat.StockCoverTile(SamusProjectileDirectionCodec.FromAimIndex(direction), frame);

    /// <summary>The cover character for native direction 0-9 and opening frame 1-3, as owned artwork.</summary>
    public SpriteArtwork CoverTile(int direction, int frame)
    {
        int index = CoverTileIndex(direction, frame) ??
            throw new InvalidDataException($"Arm-cannon direction {direction} frame {frame} names no installed cover tile.");
        return coverTiles.GetOrAdd(index, key => new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.SamusArmCannonCover, key),
            tiles.Characters.Span.Slice(key * SamusArmCannonArtworkFormat.CoverTileByteCount, SamusArmCannonArtworkFormat.CoverTileByteCount)));
    }

    /// <summary>Cover characters already built, by installed tile index.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<int, SpriteArtwork> coverTiles = new();

    /// <summary>The bytes of installed cover tile <paramref name="index"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No such cover tile is installed.</exception>
    internal ReadOnlyMemory<byte> CoverTile(int index) => (uint)index < SamusArmCannonArtworkFormat.CoverTileCount
        ? tiles.Characters.Slice(index * SamusArmCannonArtworkFormat.CoverTileByteCount, SamusArmCannonArtworkFormat.CoverTileByteCount)
        : throw new ArgumentOutOfRangeException(nameof(index), index, "No such arm-cannon cover tile is installed.");

    /// <summary>Rejects an authored drawing value outside one unsigned byte.</summary>
    private static byte RequireByte(int value, string name) => (uint)value <= byte.MaxValue
        ? (byte)value
        : throw new InvalidDataException($"Arm-cannon {name} is not a byte.");

    /// <summary>Requires an authored cover appearance to be one small piece at the cover's origin.</summary>
    private static SpritePart RequireAppearance(SpriteVisualPart? value, string name) =>
        value is { OffsetX: 0, OffsetY: 0, Size: 8 }
            ? value.ToSpritePart(SpriteArtwork.CharactersPerRow)
            : throw new InvalidDataException($"Arm-cannon {name} is not one small piece at the origin.");

    /// <summary>Strict camel-case options for loading and writing placement JSON.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
}
