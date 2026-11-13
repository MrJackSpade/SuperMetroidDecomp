using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The arm cannon's installed artwork (#1463): the cartridge's descriptor bytes, cover appearances and cover tile
/// selections, from which each pose's typed placement is decoded for gameplay, which receives only <see cref="Gameplay"/> (#1616). Its 8×8 cover characters are import data
/// the installation never holds (<see cref="SamusArmCannonCoverCharacters"/>, #1636), which the overlay sheet is composed from.
/// </summary>
public sealed class SamusArmCannonSourceArtwork
{
    /// <summary>Tile-source slots: one row per native aim direction, each holding every cover-art frame slot.</summary>
    private static readonly FixedGridShape CoverTileShape =
        new(SamusProjectileDirectionCodec.Count, OpeningFrames);
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
    /// <summary>Where each pose's drawing descriptor begins within the drawing data, as installed.</summary>
    private readonly Dictionary<SamusPoseId, int> poseDescriptors;
    /// <summary>The installed drawing window: every descriptor's direction, mode and displacement bytes.</summary>
    private readonly byte[] drawingData;
    /// <summary>Each native aim direction's installed cover appearance.</summary>
    private readonly SpritePart[] attributes;
    /// <summary>Each native aim direction's installed cover tile for opening frames one through three.</summary>
    private readonly int[][] openingTiles;

    /// <summary>Holds the complete installed descriptors, drawing data, cover appearances and cover tiles.</summary>
    private SamusArmCannonSourceArtwork(IReadOnlyDictionary<SamusPoseId, int> poseDescriptors, byte[] drawingData,
        SpritePart[] attributes, int[][] coverTiles)
    {
        Gameplay = new InstalledSamusArmCannonArtwork(Placements(poseDescriptors, drawingData));
        this.poseDescriptors = poseDescriptors.ToDictionary();
        this.drawingData = [.. drawingData];
        this.attributes = [.. attributes];
        openingTiles = [.. coverTiles.Select(row => row.ToArray())];
    }

    /// <summary>Where each pose draws the cover, which is what the game reads of the arm cannon.</summary>
    public InstalledSamusArmCannonArtwork Gameplay { get; }

    /// <summary>
    /// Decodes each pose's descriptor into its typed placement: the descriptor's direction byte, whose top bit makes
    /// the byte after the drawing mode the direction of every later animation frame, then the drawing mode, then one
    /// signed X and Y displacement per animation frame. Poses sharing a descriptor share its placement.
    /// </summary>
    private static Dictionary<SamusPoseId, SamusArmCannonPlacement> Placements(IReadOnlyDictionary<SamusPoseId, int> poseDescriptors, byte[] drawingData)
    {
        RequireEveryRecordedPose(poseDescriptors);
        var placements = new Dictionary<int, SamusArmCannonPlacement>();
        return SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, pose => Placement(poseDescriptors[pose]));

        SamusArmCannonPlacement Placement(int descriptor)
        {
            if (placements.TryGetValue(descriptor, out SamusArmCannonPlacement? shared))
                return shared;
            int direction = drawingData[descriptor];
            bool frameDependent = (direction & FrameDependentDirectionFlag) != 0;
            int offsetsStart = descriptor + (frameDependent ? FrameDependentHeaderBytes : SingleDirectionHeaderBytes);
            int frames = Math.Max(0, (drawingData.Length - offsetsStart) / DisplacementBytes);
            // The drawing-mode byte's low four bits choose whether and when the cover draws; each direction byte names an aim.
            var placement = new SamusArmCannonPlacement(SamusArmCannonDrawOrderCodec.FromDrawingMode(drawingData[descriptor + DrawingModeByte]),
                SamusProjectileDirectionCodec.FromAimIndex(direction & DirectionBits),
                frameDependent ? SamusProjectileDirectionCodec.FromAimIndex(drawingData[descriptor + LaterDirectionByte] & DirectionBits) : null,
                Enumerable.Range(0, frames).Select(frame => offsetsStart + frame * DisplacementBytes).Select(at => Offset16<RoomPixels>.Of(
                    unchecked((sbyte)drawingData[at]), unchecked((sbyte)drawingData[at + 1]))));
            placements.Add(descriptor, placement);
            return placement;
        }
    }

    /// <summary>SHA-256 of selected cannon placement, cover appearances and tile selectors; the characters are import data (#1636).</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("samus-arm-cannon-v5", content =>
    {
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses) content.Append("pose descriptor", PoseDescriptor(pose));
        Span<byte> selectedDrawing = stackalloc byte[SamusArmCannonArtworkFormat.DrawingDataByteCount];
        for (int index = 0; index < selectedDrawing.Length; index++)
            selectedDrawing[index] = ReadDrawingByte(index);
        content.Append("drawing data", selectedDrawing);
        for (int direction = 0; direction < SamusProjectileDirectionCodec.Count; direction++)
            CoverAppearance(direction).AppendIdentity(content);
        for (int direction = 0; direction < SamusProjectileDirectionCodec.Count; direction++)
            for (int frame = 1; frame <= OpeningFrames; frame++)
                content.Append("cover tile", OpeningTile(direction, frame));
    });

    /// <summary>Validated JSON placement tables.</summary>
    internal sealed record DescriptorTables(IReadOnlyDictionary<SamusPoseId, int> PoseDescriptors, byte[] DrawingData,
        SpritePart[] Attributes, int[][] CoverTiles);

    /// <summary>The artwork of validated descriptor tables.</summary>
    internal static SamusArmCannonSourceArtwork FromPlacement(DescriptorTables placement) =>
        new(placement.PoseDescriptors, placement.DrawingData, placement.Attributes, placement.CoverTiles);

    /// <summary>Parses arm-cannon JSON and validates descriptor, attribute, and tile-source geometry.</summary>
    internal static DescriptorTables LoadPlacement(Stream json)
    {
        SamusArmCannonArtworkDocument document;
        document = JsonAssetDocument.Read<SamusArmCannonArtworkDocument>(json, Options, "arm-cannon artwork");
        if (document.Version != SamusArmCannonArtworkFormat.Version ||
            document.PoseDescriptors is null ||
            document.DrawingData is null ||
            document.DrawingData.Length != SamusArmCannonArtworkFormat.DrawingDataByteCount ||
            document.CoverAppearances is null ||
            document.CoverAppearances.Length != SamusProjectileDirectionCodec.Count ||
            !CoverTileShape.HasRows(document.CoverTiles))
            throw new InvalidDataException("Arm-cannon artwork has an invalid version or table geometry.");

        RequireEveryRecordedPose(document.PoseDescriptors);
        Dictionary<SamusPoseId, int> descriptors = document.PoseDescriptors.ToDictionary();
        foreach (int descriptor in descriptors.Values)
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

    /// <summary>Gets where <paramref name="pose"/>'s installed drawing descriptor begins within the drawing data.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record in the pose tables.</exception>
    public int PoseDescriptor(SamusPoseId pose) =>
        poseDescriptors.TryGetValue(pose, out int selected) ? selected
            : throw new ArgumentOutOfRangeException(nameof(pose));

    /// <summary>Requires <paramref name="poseDescriptors"/> to hold a descriptor for exactly the poses with records.</summary>
    /// <exception cref="InvalidDataException">A pose with a record has no descriptor, or a pose without one has.</exception>
    private static void RequireEveryRecordedPose(IReadOnlyDictionary<SamusPoseId, int> poseDescriptors)
    {
        if (!SamusPoseNumbers.CoversRecordedPoses(poseDescriptors))
            throw new InvalidDataException("Arm-cannon placements must cover exactly the poses with records.");
    }

    /// <summary>The installed descriptor byte <paramref name="offset"/> bytes into the drawing data.</summary>
    /// <param name="offset">Byte offset into the drawing data; coordinate bytes are signed pixel offsets.</param>
    /// <exception cref="InvalidDataException">The offset lies outside the installed drawing window.</exception>
    public byte ReadDrawingByte(int offset) => (uint)offset < (uint)drawingData.Length
        ? drawingData[offset]
        : throw new InvalidDataException($"Arm-cannon drawing byte {offset} is not installed.");

    /// <summary>Gets the installed cover's character, palette, priority, and mirroring for native direction 0–9.</summary>
    public SpritePart CoverAppearance(int direction) => (uint)direction < (uint)attributes.Length
        ? attributes[direction]
        : throw new ArgumentOutOfRangeException(nameof(direction));

    /// <summary>The installed cover tile native direction 0–9 shows at cover frame 0–3; the closed frame 0 shows none.</summary>
    public int? CoverTileIndex(int direction, int frame)
    {
        if ((uint)direction >= SamusProjectileDirectionCodec.Count)
            throw new ArgumentOutOfRangeException(nameof(direction));
        if ((uint)frame >= SamusArmCannonArtworkFormat.FramesPerDirection)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return frame == 0 ? null : OpeningTile(direction, frame);
    }

    /// <summary>The installed cover tile <paramref name="direction"/> shows at opening frame 1–3.</summary>
    private int OpeningTile(int direction, int frame) => openingTiles[direction][frame - 1];

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
