using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Visual OAM compositions selected by the SR388 egg and confused-baby lists.</summary>
internal static class IntroDiscoveryActorSpriteDefinitions
{
    /// <summary>
    /// Sixteen egg states (intact, seven cracking stages, hatched, seven remnants) and three confused Baby states: every pose
    /// the discovery scene's two actors show (#1645). The egg's lists, from $8B:CB33 to $8B:CB9F, show the egg's sixteen;
    /// the confused baby's one list, $8B:CC2B, shows its three. The large asteroids' composition ($8C:909D) is no pose of
    /// theirs: the list after the baby's, $8B:CC3F, is the Ceres station's large asteroids', which only the Ceres
    /// destruction draws, over its own characters.
    /// </summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("egg-intact", CinematicSpritePose.EggIntact, 6),
        new("egg-crack-1", CinematicSpritePose.EggRockingLeft, 9),
        new("egg-crack-2", CinematicSpritePose.EggRockingRight, 9),
        new("egg-crack-3", CinematicSpritePose.EggCrackingUpperLeft, 9),
        new("egg-crack-4", CinematicSpritePose.EggCrackingTop, 9),
        new("egg-crack-5", CinematicSpritePose.EggCrackingLeft, 9),
        new("egg-crack-6", CinematicSpritePose.EggCrackingUpperRight, 9),
        new("egg-crack-7", CinematicSpritePose.EggCrackingRight, 9),
        new("egg-hatched", CinematicSpritePose.EggHatched, 9),
        new("egg-remnant-1", CinematicSpritePose.EggRemnantDomeSagging, 3),
        new("egg-remnant-2", CinematicSpritePose.EggRemnantDomeFalling, 3),
        new("egg-remnant-3", CinematicSpritePose.EggRemnantDomeCrumbling, 3),
        new("egg-remnant-4", CinematicSpritePose.EggRemnantRibsBared, 3),
        new("egg-remnant-5", CinematicSpritePose.EggRemnantRibsShifting, 3),
        new("egg-remnant-6", CinematicSpritePose.EggRemnantRibsSettling, 3),
        new("egg-remnant-7", CinematicSpritePose.EggRemnantSettled, 3),
        new("confused-baby-1", CinematicSpritePose.ConfusedBabyStretched, 1),
        new("confused-baby-2", CinematicSpritePose.ConfusedBabyRound, 1),
        new("confused-baby-3", CinematicSpritePose.ConfusedBabySquashed, 1),
    ];
}
