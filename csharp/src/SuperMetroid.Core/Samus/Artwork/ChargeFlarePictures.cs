using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteDefinitions;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Which composition a charge-flare component shows for its picture and Samus's facing (#1624, #2553). The main flare's
/// pictures are the same either way; each spark ring shows its own facing's six compositions.
/// </summary>
public static class ChargeFlarePictures
{
    /// <summary>The composition the main flare shows for <paramref name="picture"/>, facing either way.</summary>
    public static ProjectileSpriteId MainFlare(MainFlarePicture picture) => MainFlareCompositions[picture];

    /// <summary>The composition the slow spark ring shows for <paramref name="picture"/> facing left or right.</summary>
    public static ProjectileSpriteId SlowSparks(HorizontalDirection facing, SparkPicture picture) =>
        SparkCompositions[facing == HorizontalDirection.Left ? ProjectileSpriteGroup.FlareSlowSparksLeft : ProjectileSpriteGroup.FlareSlowSparksRight][picture];

    /// <summary>The composition the fast spark ring shows for <paramref name="picture"/> facing left or right.</summary>
    public static ProjectileSpriteId FastSparks(HorizontalDirection facing, SparkPicture picture) =>
        SparkCompositions[facing == HorizontalDirection.Left ? ProjectileSpriteGroup.FlareFastSparksLeft : ProjectileSpriteGroup.FlareFastSparksRight][picture];

    /// <summary>The main flare's composition of each of its pictures, as <see cref="ChargeFlareSpriteDefinitions.All"/> lists them.</summary>
    private static readonly IdentityTable<MainFlarePicture, ProjectileSpriteId> MainFlareCompositions = new(picture =>
        All.Single(composition => composition.Group == ProjectileSpriteGroup.FlareMain && composition.Pose == ChargeFlarePictureKeys.MainFlare.Of(picture)));

    /// <summary>Each spark family's composition of each spark picture, as <see cref="ChargeFlareSpriteDefinitions.All"/> lists them.</summary>
    private static readonly Dictionary<ProjectileSpriteGroup, IdentityTable<SparkPicture, ProjectileSpriteId>> SparkCompositions =
        SparkFamilies.ToDictionary(family => family, family => new IdentityTable<SparkPicture, ProjectileSpriteId>(picture =>
            All.Single(composition => composition.Group == family && composition.Pose == ChargeFlarePictureKeys.Sparks.Of(picture))));
}
