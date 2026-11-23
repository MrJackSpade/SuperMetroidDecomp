using System.Collections.Frozen;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The stretches of Ceres's destruction the cinematic tests (#1452): each names the phases it covers instead of
/// ordering the phase identities.
/// </summary>
internal static partial class CeresDestructionPhaseGroups
{
    /// <summary>The phases that show Ceres: the approach to the explosion, the flight away from it and its fade out.</summary>
    internal static readonly FrozenSet<CeresDestructionPhase> CeresScene = new[]
    {
        CeresDestructionPhase.WaitForMusicQueue, CeresDestructionPhase.FadeInAndDrift, CeresDestructionPhase.ApproachExplosion,
        CeresDestructionPhase.FlyingAwayFromExplosion, CeresDestructionPhase.HoldAfterExplosion, CeresDestructionPhase.FadeOutCeres,
    }.ToFrozenSet();

    /// <summary>The Ceres scene and the first frame of the flight to Zebes, which still use the Ceres flight colours.</summary>
    internal static readonly FrozenSet<CeresDestructionPhase> ThroughFlightToZebesStart = CeresScene.Concat(new[]
    {
        CeresDestructionPhase.FlyToZebesInitial,
    }).ToFrozenSet();
}
