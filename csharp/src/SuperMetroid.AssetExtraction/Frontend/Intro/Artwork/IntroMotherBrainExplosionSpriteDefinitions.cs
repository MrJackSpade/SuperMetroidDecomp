using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>The twelve OAM compositions selected by the fourth-hit explosion loops: six small, then six large.</summary>
internal static class IntroMotherBrainExplosionSpriteDefinitions
{
    /// <summary>Six small explosion frames (two one-part, then four-part), then six large (one one-part, then four-part).</summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("small-explosion-spark", CinematicSpritePose.SmallExplosionSpark, 1),
        new("small-explosion-flare", CinematicSpritePose.SmallExplosionFlare, 1),
        new("small-explosion-fireball", CinematicSpritePose.SmallExplosionFireball, 4),
        new("small-explosion-hollowing", CinematicSpritePose.SmallExplosionHollowing, 4),
        new("small-explosion-ring", CinematicSpritePose.SmallExplosionRing, 4),
        new("small-explosion-fading-ring", CinematicSpritePose.SmallExplosionFadingRing, 4),
        new("big-explosion-spark", CinematicSpritePose.BigExplosionSpark, 1),
        new("big-explosion-flare", CinematicSpritePose.BigExplosionFlare, 4),
        new("big-explosion-fireball", CinematicSpritePose.BigExplosionFireball, 4),
        new("big-explosion-bursting", CinematicSpritePose.BigExplosionBursting, 4),
        new("big-explosion-spiked-ring", CinematicSpritePose.BigExplosionSpikedRing, 4),
        new("big-explosion-scattered-smoke", CinematicSpritePose.BigExplosionScatteredSmoke, 4),
    ];
}
