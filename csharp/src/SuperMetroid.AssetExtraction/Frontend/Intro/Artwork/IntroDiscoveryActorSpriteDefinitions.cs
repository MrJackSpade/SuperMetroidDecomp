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
        new("egg-rocking-left", CinematicSpritePose.EggRockingLeft, 9),
        new("egg-rocking-right", CinematicSpritePose.EggRockingRight, 9),
        new("egg-cracking-upper-left", CinematicSpritePose.EggCrackingUpperLeft, 9),
        new("egg-cracking-top", CinematicSpritePose.EggCrackingTop, 9),
        new("egg-cracking-left", CinematicSpritePose.EggCrackingLeft, 9),
        new("egg-cracking-upper-right", CinematicSpritePose.EggCrackingUpperRight, 9),
        new("egg-cracking-right", CinematicSpritePose.EggCrackingRight, 9),
        new("egg-hatched", CinematicSpritePose.EggHatched, 9),
        new("egg-remnant-dome-sagging", CinematicSpritePose.EggRemnantDomeSagging, 3),
        new("egg-remnant-dome-falling", CinematicSpritePose.EggRemnantDomeFalling, 3),
        new("egg-remnant-dome-crumbling", CinematicSpritePose.EggRemnantDomeCrumbling, 3),
        new("egg-remnant-ribs-bared", CinematicSpritePose.EggRemnantRibsBared, 3),
        new("egg-remnant-ribs-shifting", CinematicSpritePose.EggRemnantRibsShifting, 3),
        new("egg-remnant-ribs-settling", CinematicSpritePose.EggRemnantRibsSettling, 3),
        new("egg-remnant-settled", CinematicSpritePose.EggRemnantSettled, 3),
        new("confused-baby-stretched", CinematicSpritePose.ConfusedBabyStretched, 1),
        new("confused-baby-round", CinematicSpritePose.ConfusedBabyRound, 1),
        new("confused-baby-squashed", CinematicSpritePose.ConfusedBabySquashed, 1),
    ];
}
