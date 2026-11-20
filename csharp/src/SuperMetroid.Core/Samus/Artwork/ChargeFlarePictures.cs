using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Which composition a charge-flare component shows for its picture and Samus's facing (#1624). The main flare's
/// pictures are the same either way; each spark ring shows its own facing's six compositions.
/// </summary>
/// <remarks>
/// A component shows only its own pictures. One the Hyper Beam's backward count has stepped back past its first picture
/// shows nothing (<see cref="ChargeFlareComponentState"/>): it never draws the neighbouring run's picture.
/// </remarks>
public static class ChargeFlarePictures
{
    /// <summary>The composition <paramref name="component"/> shows for <paramref name="picture"/> facing left or right.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="picture"/> is not one of the component's pictures.</exception>
    public static ProjectileSpriteId Of(ChargeFlareComponent component, bool facingLeft, ChargeFlarePicture picture)
        => Compositions[FamilyOf(component, facingLeft)].TryGetValue(picture, out ProjectileSpriteId composition)
            ? composition
            : throw new ArgumentOutOfRangeException(nameof(picture), picture, $"Charge-flare {component} shows no such picture.");

    /// <summary>The picture family each component of the charge flare draws from, by Samus's facing.</summary>
    private static IdentityTable<ChargeFlareComponent, Func<bool, ProjectileSpriteGroup>> Families => field ??= new(new Dictionary<ChargeFlareComponent, Func<bool, ProjectileSpriteGroup>>
    {
        [ChargeFlareComponent.MainFlare] = static facingLeft => ProjectileSpriteGroup.FlareMain,
        [ChargeFlareComponent.SlowSparks] = static facingLeft => facingLeft ? ProjectileSpriteGroup.FlareSlowSparksLeft : ProjectileSpriteGroup.FlareSlowSparksRight,
        [ChargeFlareComponent.FastSparks] = static facingLeft => facingLeft ? ProjectileSpriteGroup.FlareFastSparksLeft : ProjectileSpriteGroup.FlareFastSparksRight,
    });

    /// <summary>The family of <paramref name="component"/>'s compositions for the facing.</summary>
    private static ProjectileSpriteGroup FamilyOf(ChargeFlareComponent component, bool facingLeft) => Families[component](facingLeft);

    /// <summary>Each flare family's composition of each picture it shows, as <see cref="ChargeFlareSpriteDefinitions.All"/> lists them.</summary>
    private static readonly Dictionary<ProjectileSpriteGroup, Dictionary<ChargeFlarePicture, ProjectileSpriteId>> Compositions =
        SparkFamilies.Prepend(ProjectileSpriteGroup.FlareMain).ToDictionary(family => family, family =>
            (family == ProjectileSpriteGroup.FlareMain ? MainPictures : SparkPictures).ToDictionary(picture => picture,
                picture => All.Single(composition => composition.Group == family && composition.Pose == ChargeFlarePictureKeys.Installed.Of(picture))));
}
