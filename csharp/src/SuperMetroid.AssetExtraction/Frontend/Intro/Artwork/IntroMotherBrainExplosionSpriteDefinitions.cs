using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>The twelve OAM compositions selected by the fourth-hit explosion loops: six small, then six large.</summary>
internal static class IntroMotherBrainExplosionSpriteDefinitions
{
    /// <summary>Six small explosion frames (two one-part, then four-part), then six large (one one-part, then four-part).</summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("small-explosion-0", CinematicSpritePose.SmallExplosionSpark, 1),
        new("small-explosion-1", CinematicSpritePose.SmallExplosionFlare, 1),
        new("small-explosion-2", CinematicSpritePose.SmallExplosionFireball, 4),
        new("small-explosion-3", CinematicSpritePose.SmallExplosionHollowing, 4),
        new("small-explosion-4", CinematicSpritePose.SmallExplosionRing, 4),
        new("small-explosion-5", CinematicSpritePose.SmallExplosionFadingRing, 4),
        new("big-explosion-0", CinematicSpritePose.BigExplosionSpark, 1),
        new("big-explosion-1", CinematicSpritePose.BigExplosionFlare, 4),
        new("big-explosion-2", CinematicSpritePose.BigExplosionFireball, 4),
        new("big-explosion-3", CinematicSpritePose.BigExplosionBursting, 4),
        new("big-explosion-4", CinematicSpritePose.BigExplosionSpikedRing, 4),
        new("big-explosion-5", CinematicSpritePose.BigExplosionScatteredSmoke, 4),
    ];
}
