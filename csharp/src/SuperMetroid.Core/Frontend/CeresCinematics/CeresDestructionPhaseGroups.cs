using System.Collections.Frozen;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The stretches of Ceres's destruction the cinematic tests (#1452): each names the phases it covers instead of
/// ordering the phase identities.
/// </summary>
internal static class CeresDestructionPhaseGroups
{
    /// <summary>The approach to the explosion, before the gunship flies away from it.</summary>
    internal static readonly FrozenSet<CeresDestructionPhase> BeforeFlyingAway = new[]
    {
        CeresDestructionPhase.WaitForMusicQueue, CeresDestructionPhase.FadeInAndDrift, CeresDestructionPhase.ApproachExplosion,
    }.ToFrozenSet();

    /// <summary>The phases that show Ceres, through its fade out.</summary>
    internal static readonly FrozenSet<CeresDestructionPhase> CeresScene = BeforeFlyingAway.Concat(new[]
    {
        CeresDestructionPhase.FlyingAwayFromExplosion, CeresDestructionPhase.HoldAfterExplosion, CeresDestructionPhase.FadeOutCeres,
    }).ToFrozenSet();

    /// <summary>The Ceres scene and the first frame of the flight to Zebes, which still use the Ceres flight palette and run no palette effects.</summary>
    internal static readonly FrozenSet<CeresDestructionPhase> ThroughFlightToZebesStart = CeresScene.Concat(new[]
    {
        CeresDestructionPhase.FlyToZebesInitial,
    }).ToFrozenSet();
}
