using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>The twelve OAM compositions selected by the fourth-hit explosion loops: six small, then six large.</summary>
internal static class IntroMotherBrainExplosionSpriteDefinitions
{
    /// <summary>Six small explosion frames (two one-part, then four-part), then six large (one one-part, then four-part).</summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("small-explosion-0", CinematicSpritePose.SmallExplosion0, 1),
        new("small-explosion-1", CinematicSpritePose.SmallExplosion1, 1),
        new("small-explosion-2", CinematicSpritePose.SmallExplosion2, 4),
        new("small-explosion-3", CinematicSpritePose.SmallExplosion3, 4),
        new("small-explosion-4", CinematicSpritePose.SmallExplosion4, 4),
        new("small-explosion-5", CinematicSpritePose.SmallExplosion5, 4),
        new("big-explosion-0", CinematicSpritePose.BigExplosion0, 1),
        new("big-explosion-1", CinematicSpritePose.BigExplosion1, 4),
        new("big-explosion-2", CinematicSpritePose.BigExplosion2, 4),
        new("big-explosion-3", CinematicSpritePose.BigExplosion3, 4),
        new("big-explosion-4", CinematicSpritePose.BigExplosion4, 4),
        new("big-explosion-5", CinematicSpritePose.BigExplosion5, 4),
    ];
}
