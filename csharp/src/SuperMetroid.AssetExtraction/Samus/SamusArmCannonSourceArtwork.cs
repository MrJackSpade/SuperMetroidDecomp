using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The arm cannon as the import reads it (#1463, #1638): the extracted document's drawing descriptors, cover appearances
/// and cover tile selections, from the component's import data. The import decodes the descriptors into each pose's typed
/// placement (<see cref="Gameplay"/>, #1616), and the game binds only the placements the installation holds
/// (<see cref="Installed"/>, <c>samus-arm-cannon.json</c>); the appearances are packed sprite attributes only the overlay
/// sheet is composed from, and its 8×8 cover characters are import data too (<see cref="SamusArmCannonCoverCharacters"/>,
/// #1636).
/// </summary>
public sealed class SamusArmCannonSourceArtwork
{
    /// <summary>Tile-source slots: one row per native aim direction, each holding every cover-art frame slot.</summary>
    private static readonly FixedGridShape CoverTileShape =
        new(SamusProjectileDirectionCodec.Count, OpeningFrames);
    /// <summary>Cover frames that show a tile: every frame but the closed one.</summary>
    private const int OpeningFrames = SamusArmCannonArtworkFormat.FramesPerDirection - 1;
    /// <summary>Each native aim direction's installed cover appearance.</summary>
    private readonly SpritePart[] attributes;
    /// <summary>Each native aim direction's installed cover tile for opening frames one through three.</summary>
    private readonly int[][] openingTiles;

    /// <summary>Holds the complete descriptors, cover appearances and cover tiles.</summary>
    private SamusArmCannonSourceArtwork(SamusArmCannonDescriptors descriptors, SpritePart[] attributes, int[][] coverTiles)
    {
        Descriptors = descriptors;
        this.attributes = [.. attributes];
        openingTiles = [.. coverTiles.Select(row => row.ToArray())];
    }

    /// <summary>Each pose's drawing descriptor within the extracted drawing window, which only the import decodes.</summary>
    public SamusArmCannonDescriptors Descriptors { get; }

    /// <summary>Where each pose draws the cover, as its descriptor reads it over the whole drawing window.</summary>
    public InstalledSamusArmCannonArtwork Gameplay => Descriptors.Gameplay;

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

    /// <summary>Validated document tables: the drawing descriptors, the cover appearances and the cover tiles.</summary>
    internal sealed record DescriptorTables(SamusArmCannonDescriptors Descriptors, SpritePart[] Attributes, int[][] CoverTiles);

    /// <summary>The artwork of validated document tables.</summary>
    internal static SamusArmCannonSourceArtwork FromPlacement(DescriptorTables placement) =>
        new(placement.Descriptors, placement.Attributes, placement.CoverTiles);

    /// <summary>
    /// The placements the installation holds (#1638): each pose's drawing order and directions, and its cover's displacement
    /// at every animation frame the pose shows when her animation reads past its programs with <paramref name="framesBeyondAnimation"/>,
    /// where the drawing window holds one.
    /// </summary>
    internal SamusArmCannonLayout Installed(FramesBeyondAnimation<SamusPoseId, SamusPoseAnimationStep> framesBeyondAnimation) =>
        Descriptors.Installed(SamusShownFrames.Of(framesBeyondAnimation));

    /// <summary>Parses the extracted arm-cannon document and validates descriptor, attribute, and tile-source geometry.</summary>
    internal static DescriptorTables LoadPlacement(Stream json)
    {
        SamusArmCannonArtworkDocument document;
        document = JsonAssetDocument.Read<SamusArmCannonArtworkDocument>(json, Options, "arm-cannon artwork");
        if (document.Version != SamusArmCannonArtworkFormat.Version ||
            document.CoverAppearances is null ||
            document.CoverAppearances.Length != SamusProjectileDirectionCodec.Count ||
            !CoverTileShape.HasRows(document.CoverTiles))
            throw new InvalidDataException("Arm-cannon artwork has an invalid version or table geometry.");

        SamusArmCannonDescriptors descriptors = SamusArmCannonDescriptors.Create(document.PoseDescriptors, document.DrawingData);
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
        return new DescriptorTables(descriptors, attributes, coverTiles);
    }

    /// <summary>Serializes the extracted document as indented camel-case UTF-8 JSON for the import data's <c>samus-arm-cannon-artwork.json</c>; table bounds and selector validation occur when it is loaded, not here.</summary>
    public static byte[] Write(SamusArmCannonArtworkDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        return json;
    }

    /// <summary>Gets where <paramref name="pose"/>'s extracted drawing descriptor begins within the drawing data.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record in the pose tables.</exception>
    public int PoseDescriptor(SamusPoseId pose) => Descriptors.PoseDescriptor(pose);

    /// <summary>The extracted descriptor byte <paramref name="offset"/> bytes into the drawing data.</summary>
    /// <param name="offset">Byte offset into the drawing data; coordinate bytes are signed pixel offsets.</param>
    /// <exception cref="InvalidDataException">The offset lies outside the extracted drawing window.</exception>
    public byte ReadDrawingByte(int offset) => Descriptors.ReadDrawingByte(offset);

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
