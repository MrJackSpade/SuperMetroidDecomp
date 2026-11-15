using System.Collections.Frozen;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The stretches of the Metroid egg's hatching the intro tests (#1978), each named for what the egg shows.</summary>
internal static class IntroEggPoses
{
    /// <summary>
    /// The egg's burst shell, from its dome sagging as the Baby leaves it to its settled remains: the confused Baby
    /// starts rising on the update the egg first shows one.
    /// </summary>
    internal static readonly FrozenSet<CinematicSpritePose> BurstShell = new[]
    {
        CinematicSpritePose.EggRemnantDomeSagging, CinematicSpritePose.EggRemnantDomeFalling,
        CinematicSpritePose.EggRemnantDomeCrumbling, CinematicSpritePose.EggRemnantRibsBared,
        CinematicSpritePose.EggRemnantRibsShifting, CinematicSpritePose.EggRemnantRibsSettling,
        CinematicSpritePose.EggRemnantSettled,
    }.ToFrozenSet();
}
