using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Which composition a charge-flare component shows for its picture and Samus's facing (#1624). The main flare's
/// pictures are the same either way; each spark ring shows its own facing's six compositions.
/// </summary>
/// <remarks>
/// A component shows only its own pictures. One the Hyper Beam's backward count has run past its first picture shows
/// nothing (<see cref="ChargeFlareComponentState"/>): it does not draw the neighbouring run's picture, which was a
/// presentation quirk (owner decision, 2026-10-12).
/// </remarks>
public static class ChargeFlarePictures
{
    /// <summary>
    /// The composition <paramref name="component"/> shows at place <paramref name="place"/> of its animation facing left
    /// or right.
    /// </summary>
    /// <param name="component">The component.</param>
    /// <param name="facingLeft">Whether Samus faces left.</param>
    /// <param name="place">The place of one of the component's pictures in its animation.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="place"/> shows none of the component's pictures.</exception>
    public static ProjectileSpriteId Of(ChargeFlareComponent component, bool facingLeft, int place) =>
        ChargeFlareAnimations.PictureAt(component, place) is { } picture
            ? Of(component, facingLeft, picture)
            : throw new ArgumentOutOfRangeException(nameof(place), place, $"Charge-flare {component} shows no picture at place {place}.");

    /// <summary>The composition <paramref name="component"/> shows for <paramref name="picture"/> facing left or right.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="picture"/> is not one of the component's pictures.</exception>
    public static ProjectileSpriteId Of(ChargeFlareComponent component, bool facingLeft, ChargeFlarePicture picture)
        => Compositions[FamilyOf(component, facingLeft)].TryGetValue(picture, out ProjectileSpriteId composition)
            ? composition
            : throw new ArgumentOutOfRangeException(nameof(picture), picture, $"Charge-flare {component} shows no such picture.");

    /// <summary>The family of <paramref name="component"/>'s compositions for the facing.</summary>
    private static ProjectileSpriteGroup FamilyOf(ChargeFlareComponent component, bool facingLeft) => component switch
    {
        ChargeFlareComponent.MainFlare => ProjectileSpriteGroup.FlareMain,
        ChargeFlareComponent.SlowSparks => facingLeft ? ProjectileSpriteGroup.FlareSlowSparksLeft : ProjectileSpriteGroup.FlareSlowSparksRight,
        ChargeFlareComponent.FastSparks => facingLeft ? ProjectileSpriteGroup.FlareFastSparksLeft : ProjectileSpriteGroup.FlareFastSparksRight,
        _ => throw new ArgumentOutOfRangeException(nameof(component), component, $"Undefined {nameof(ChargeFlareComponent)}."),
    };

    /// <summary>Each flare family's composition of each picture it shows, as <see cref="ChargeFlareSpriteDefinitions.All"/> lists them.</summary>
    private static readonly Dictionary<ProjectileSpriteGroup, Dictionary<ChargeFlarePicture, ProjectileSpriteId>> Compositions =
        SparkFamilies.Prepend(ProjectileSpriteGroup.FlareMain).ToDictionary(family => family, family =>
            (family == ProjectileSpriteGroup.FlareMain ? MainPictures : SparkPictures).ToDictionary(picture => picture,
                picture => All.Single(composition => composition.Group == family && composition.Pose == picture.ToString())));
}
