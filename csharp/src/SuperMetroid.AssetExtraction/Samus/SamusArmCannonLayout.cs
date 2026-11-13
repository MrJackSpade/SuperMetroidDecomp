using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// What the game binds of the arm cannon (#1638): each pose's typed placement, its drawing order, the aim directions whose
/// cover pictures it shows and the cover's displacement at every animation frame the pose shows. The installed
/// <c>samus-arm-cannon.json</c> holds only this; the drawing descriptors the import decoded the placements from, the cover
/// appearances and the tile selections are import data (<see cref="SamusArmCannonSourceArtwork"/>).
/// </summary>
public sealed class SamusArmCannonLayout
{
    /// <summary>Each pose's placement.</summary>
    private readonly Dictionary<SamusPoseId, SamusArmCannonPlacement> placements;

    /// <summary>Holds a placement for every pose with a record.</summary>
    /// <exception cref="InvalidDataException">The placements do not cover exactly the poses with records.</exception>
    internal SamusArmCannonLayout(IReadOnlyDictionary<SamusPoseId, SamusArmCannonPlacement> placements)
    {
        Ensure.NotNull(placements);
        if (!SamusPoseNumbers.CoversRecordedPoses(placements))
            throw new InvalidDataException("Arm-cannon placements must cover exactly the poses with records.");
        this.placements = placements.ToDictionary();
        Gameplay = new InstalledSamusArmCannonArtwork(this.placements);
    }

    /// <summary>Where each pose draws the cover, which is what the game reads of the arm cannon.</summary>
    public InstalledSamusArmCannonArtwork Gameplay { get; }

    /// <summary>SHA-256 of each pose's drawing order, directions and displacements.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("samus-arm-cannon-layout-v2", content =>
    {
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses)
        {
            SamusArmCannonPlacement placement = placements[pose];
            content.AppendName("draw order", placement.DrawOrder.ToString());
            content.AppendName("first direction", placement.FirstDirection.ToString());
            content.AppendName("later direction", placement.LaterDirection?.ToString() ?? string.Empty);
            content.Append("displacement count", placement.Offsets.Count);
            foreach ((int frame, Offset16<RoomPixels> offset) in placement.Offsets)
            {
                content.Append("animation frame", frame);
                content.Append("x", offset.X);
                content.Append("y", offset.Y);
            }
        }
    });

    /// <summary>Parses the installed layout document, as the player may have edited it, and validates its placements.</summary>
    /// <exception cref="InvalidDataException">
    /// The document is malformed or has another version, the placements do not cover exactly the poses with records, or an
    /// animation frame is outside 0 through 255.
    /// </exception>
    internal static SamusArmCannonLayout Read(Stream json)
    {
        SamusArmCannonLayoutDocument document = JsonAssetDocument.Read<SamusArmCannonLayoutDocument>(json, Options, "arm-cannon layout");
        if (document.Version != SamusArmCannonArtworkFormat.Version || document.Poses is null ||
            document.Poses.Values.Any(pose => pose is null || pose.Displacements is null))
            throw new InvalidDataException("Arm-cannon layout has an invalid version or table geometry.");
        return new SamusArmCannonLayout(document.Poses.ToDictionary(pose => pose.Key, pose => Placement(pose.Key, pose.Value)));
    }

    /// <summary>Serializes the layout as indented camel-case UTF-8 JSON for <c>samus-arm-cannon.json</c>.</summary>
    internal byte[] Write() => JsonSerializer.SerializeToUtf8Bytes(new SamusArmCannonLayoutDocument
    {
        Version = SamusArmCannonArtworkFormat.Version,
        Poses = SamusPoseNumbers.RecordedPoses.ToDictionary(pose => pose, pose =>
        {
            SamusArmCannonPlacement placement = placements[pose];
            return new SamusArmCannonPoseDocument
            {
                DrawOrder = placement.DrawOrder,
                Direction = placement.FirstDirection,
                LaterDirection = placement.LaterDirection,
                Displacements = placement.Offsets.ToDictionary(),
            };
        }),
    }, Options);

    /// <summary>The placement <paramref name="document"/> describes for <paramref name="pose"/>.</summary>
    /// <exception cref="InvalidDataException">An animation frame is outside 0 through 255.</exception>
    private static SamusArmCannonPlacement Placement(SamusPoseId pose, SamusArmCannonPoseDocument document)
    {
        foreach (int frame in document.Displacements.Keys)
            if ((uint)frame >= InstalledSamusBody.AnimationFrames)
                throw new InvalidDataException($"Arm-cannon pose {pose} has a displacement at animation frame {frame}, outside 0 through 255.");
        return new SamusArmCannonPlacement(document.DrawOrder, document.Direction, document.LaterDirection, document.Displacements);
    }

    /// <summary>Strict camel-case options for loading and writing the layout document, with names for every enumeration.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter<SamusArmCannonDrawOrder>(allowIntegerValues: false),
            new JsonStringEnumConverter<SamusProjectileDirection>(allowIntegerValues: false),
        },
    };
}
