using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Spatial;

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
            content.AppendName("draw order", SamusArmCannonDrawOrderKeys.Installed.Of(placement.DrawOrder));
            content.AppendName("first direction", SamusProjectileDirectionKeys.Installed.Of(placement.FirstDirection));
            content.AppendName("later direction", placement.LaterDirection is { } later ? SamusProjectileDirectionKeys.Installed.Of(later) : string.Empty);
            content.Append("displacement count", placement.Offsets.Count);
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
            foreach ((SamusAnimationFrame frame, Offset16<RoomPixels> offset) in placement.Offsets.OrderBy(pair => pair.Key, animation.FrameOrder))
            {
                content.AppendName("animation frame", animation.NameOf(frame));
                content.Append("x", offset.X);
                content.Append("y", offset.Y);
            }
        }
    });

    /// <summary>Parses the installed layout document, as the player may have edited it, and validates its placements.</summary>
    /// <exception cref="InvalidDataException">
    /// The document is malformed or has another version, the placements do not cover exactly the poses with records, or a
    /// displacement names no frame of its pose's animation.
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
                Displacements = placement.Offsets.OrderBy(pair => pair.Key, SamusAnimationDelays.AnimationForPose(pose).FrameOrder).ToDictionary(
                    pair => SamusAnimationDelays.AnimationForPose(pose).NameOf(pair.Key), pair => pair.Value, StringComparer.Ordinal),
            };
        }),
    }, Options);

    /// <summary>The placement <paramref name="document"/> describes for <paramref name="pose"/>.</summary>
    /// <exception cref="InvalidDataException">A displacement names no frame of the pose's animation.</exception>
    private static SamusArmCannonPlacement Placement(SamusPoseId pose, SamusArmCannonPoseDocument document)
    {
        SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(pose);
        return new SamusArmCannonPlacement(document.DrawOrder, document.Direction, document.LaterDirection,
            document.Displacements.ToDictionary(pair => animation.FrameNamed(pair.Key), pair => pair.Value));
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
            new SamusArmCannonDrawOrderJsonConverter(),
            new SamusProjectileDirectionJsonConverter(),
            new SamusPoseIdJsonConverter(),
        },
    };
}
