using System.Collections.Frozen;
using SuperMetroid.Core.Samus.Projectiles;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
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
    /// <summary>Each aim's cover appearance.</summary>
    private readonly FrozenDictionary<SamusProjectileDirection, SpritePart> appearances;
    /// <summary>Each aim's cover orientation, whose pictures its opening shows.</summary>
    private readonly FrozenDictionary<SamusProjectileDirection, SamusArmCannonCoverOrientation> orientations;

    /// <summary>Holds the complete descriptors, cover appearances and cover orientations.</summary>
    private SamusArmCannonSourceArtwork(SamusArmCannonDescriptors descriptors,
        IReadOnlyDictionary<SamusProjectileDirection, SpritePart> appearances,
        IReadOnlyDictionary<SamusProjectileDirection, SamusArmCannonCoverOrientation> orientations)
    {
        Descriptors = descriptors;
        this.appearances = appearances.ToFrozenDictionary();
        this.orientations = orientations.ToFrozenDictionary();
    }

    /// <summary>Each pose's drawing descriptor within the extracted drawing window, which only the import decodes.</summary>
    public SamusArmCannonDescriptors Descriptors { get; }

    /// <summary>Where each pose draws the cover, as its descriptor reads it over the whole drawing window.</summary>
    public InstalledSamusArmCannonArtwork Gameplay => Descriptors.Gameplay;

    /// <summary>SHA-256 of selected cannon placement, cover appearances and tile selectors; the characters are import data (#1636).</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("samus-arm-cannon-v6", content =>
    {
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses) content.Append("pose descriptor", PoseDescriptor(pose));
        Span<byte> selectedDrawing = stackalloc byte[SamusArmCannonArtworkFormat.DrawingDataByteCount];
        for (int index = 0; index < selectedDrawing.Length; index++)
            selectedDrawing[index] = ReadDrawingByte(index);
        content.Append("drawing data", selectedDrawing);
        foreach (SamusProjectileDirection direction in SamusProjectileDirectionCodec.InAimOrder)
            CoverAppearance(direction).AppendIdentity(content);
        foreach (SamusProjectileDirection direction in SamusProjectileDirectionCodec.InAimOrder)
            content.AppendName("cover orientation", SamusArmCannonCoverOrientationKeys.Installed.Of(orientations[direction]));
    });

    /// <summary>Validated document tables: the drawing descriptors, each aim's cover appearance and its cover orientation.</summary>
    internal sealed record DescriptorTables(SamusArmCannonDescriptors Descriptors,
        IReadOnlyDictionary<SamusProjectileDirection, SpritePart> Appearances,
        IReadOnlyDictionary<SamusProjectileDirection, SamusArmCannonCoverOrientation> Orientations);

    /// <summary>The artwork of validated document tables.</summary>
    internal static SamusArmCannonSourceArtwork FromPlacement(DescriptorTables placement) =>
        new(placement.Descriptors, placement.Appearances, placement.Orientations);

    /// <summary>
    /// The placements the installation holds (#1638): each pose's drawing order and directions, and its cover's displacement
    /// at every animation frame the pose shows as <paramref name="shown"/> decided it, where the drawing window holds one.
    /// </summary>
    internal SamusArmCannonLayout Installed(SuperMetroid.Core.Samus.Drawing.SamusShownFrames shown) => Descriptors.Installed(shown);

    /// <summary>Parses the extracted arm-cannon document and validates descriptor, attribute, and tile-source geometry.</summary>
    internal static DescriptorTables LoadPlacement(Stream json)
    {
        SamusArmCannonArtworkDocument document;
        document = JsonAssetDocument.Read<SamusArmCannonArtworkDocument>(json, Options, "arm-cannon artwork");
        if (document.Version != SamusArmCannonArtworkFormat.Version ||
            document.CoverAppearances is null || document.CoverOrientations is null)
            throw new InvalidDataException("Arm-cannon artwork has an invalid version or table geometry.");

        SamusArmCannonDescriptors descriptors = SamusArmCannonDescriptors.Create(document.PoseDescriptors, document.DrawingData);
        var appearances = new Dictionary<SamusProjectileDirection, SpritePart>();
        var orientations = new Dictionary<SamusProjectileDirection, SamusArmCannonCoverOrientation>();
        foreach (SamusProjectileDirection direction in SamusProjectileDirectionCodec.InAimOrder)
        {
            appearances.Add(direction, RequireAppearance(document.CoverAppearances.GetValueOrDefault(direction), $"cover appearance {direction}"));
            if (!document.CoverOrientations.TryGetValue(direction, out SamusArmCannonCoverOrientation orientation))
                throw new InvalidDataException($"Arm-cannon direction {direction} names no cover orientation.");
            orientations.Add(direction, orientation);
        }
        if (document.CoverAppearances.Count != appearances.Count || document.CoverOrientations.Count != orientations.Count)
            throw new InvalidDataException("Arm-cannon artwork names a cover for an aim that has none.");
        return new DescriptorTables(descriptors, appearances, orientations);
    }

    /// <summary>Serializes the extracted document as indented camel-case UTF-8 JSON for the import data's <c>samus-arm-cannon-artwork.json</c>; table bounds and selector validation occur when it is loaded, not here.</summary>
    public static byte[] Write(SamusArmCannonArtworkDocument document)
    {
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        return json;
    }

    /// <summary>Gets where <paramref name="pose"/>'s extracted drawing descriptor begins within the drawing data.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pose has no record in the pose tables.</exception>
    private int PoseDescriptor(SamusPoseId pose) => Descriptors.PoseDescriptor(pose);

    /// <summary>The extracted descriptor byte <paramref name="offset"/> bytes into the drawing data.</summary>
    /// <param name="offset">Byte offset into the drawing data; coordinate bytes are signed pixel offsets.</param>
    /// <exception cref="InvalidDataException">The offset lies outside the extracted drawing window.</exception>
    private byte ReadDrawingByte(int offset) => Descriptors.ReadDrawingByte(offset);

    /// <summary>Gets the installed cover's character, palette, depth, and mirroring for aim <paramref name="direction"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is no aim.</exception>
    public SpritePart CoverAppearance(SamusProjectileDirection direction) => appearances.TryGetValue(direction, out SpritePart appearance)
        ? appearance
        : throw new ArgumentOutOfRangeException(nameof(direction), direction, "No cover is drawn for the aim.");

    /// <summary>The cover picture aim <paramref name="direction"/> shows at opening frame <paramref name="frame"/> (#2556).</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is no aim or <paramref name="frame"/> no cover picture.</exception>
    internal SamusArmCannonCoverTile CoverTile(SamusProjectileDirection direction, SamusArmCannonCoverFrame frame) =>
        !SamusArmCannonCoverFrameCodec.InOpeningOrder.Contains(frame)
            ? throw new ArgumentOutOfRangeException(nameof(frame), frame, "The cover shows no such picture.")
            : orientations.TryGetValue(direction, out SamusArmCannonCoverOrientation orientation)
                ? new SamusArmCannonCoverTile(orientation, frame)
                : throw new ArgumentOutOfRangeException(nameof(direction), direction, "No cover is drawn for the aim.");

    /// <summary>Requires an authored cover appearance to be one small piece at the cover's origin.</summary>
    private static SpritePart RequireAppearance(SpriteVisualPiece? value, string name) =>
        value is { OffsetX: 0, OffsetY: 0, Size: 8 }
            ? value.ToSpritePart(SpriteArtwork.CharactersPerRow)
            : throw new InvalidDataException($"Arm-cannon {name} is not one small piece at the origin.");

    /// <summary>Strict camel-case options for loading and writing placement JSON.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
        Converters = { new SamusPoseIdJsonConverter(), new SamusProjectileDirectionJsonConverter(), new SamusArmCannonCoverOrientationJsonConverter() },
    };
}
