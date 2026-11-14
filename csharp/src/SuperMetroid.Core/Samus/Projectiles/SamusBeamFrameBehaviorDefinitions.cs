using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The frame behaviours the Spazer and Plasma combinations' shots run.</summary>
internal static class SamusBeamFrameBehaviorDefinitions
{
    /// <summary>
    /// What each Spazer and Plasma combination's shots run (<see cref="UnauthoredBeamFrameBehaviors{TBeams, TBehavior}"/>):
    /// uncharged, an unconnected region, the Chainsaw's Power Bomb fuse, the Spacetime overwrite and the game's live state;
    /// charged, the game's live state, Samus's changing state twice and the Murder Beam's misaligned start. A choice with
    /// no projectile frame behaviour runs data, outside the emulation scope limit (#1500).
    /// </summary>
    internal static readonly UnauthoredBeamFrameBehaviors<SamusBeamCombination, SamusBeamFrameBehaviorDefinition> Unauthored = new(
        new Dictionary<SamusBeamCombination, SamusBeamFrameBehaviorDefinition>
        {
            [SamusBeamCombination.SpazerPlasma] = new(SamusBeamFrameBehaviorChoice.UnchargedSpazerPlasmaRunsUnconnectedRegion, null),
            [SamusBeamCombination.SpazerPlasmaWave] = new(SamusBeamFrameBehaviorChoice.ChainsawPowerBomb,
                SamusProjectileFrameBehavior.ChainsawPowerBomb),
            [SamusBeamCombination.SpazerPlasmaIce] = new(SamusBeamFrameBehaviorChoice.SpaceTimeProgressionOverwrite,
                SamusProjectileFrameBehavior.SpaceTimeProgressionOverwrite),
            [SamusBeamCombination.SpazerPlasmaIceWave] = new(SamusBeamFrameBehaviorChoice.UnchargedAllBeamsRunsLiveState, null),
        },
        new Dictionary<SamusBeamCombination, SamusBeamFrameBehaviorDefinition>
        {
            [SamusBeamCombination.SpazerPlasma] = new(SamusBeamFrameBehaviorChoice.ChargedSpazerPlasmaRunsLiveState, null),
            [SamusBeamCombination.SpazerPlasmaWave] = new(SamusBeamFrameBehaviorChoice.ChargedChainsawRunsSamusState,
                SamusProjectileFrameBehavior.ChargedChainsawRunsSamusState),
            [SamusBeamCombination.SpazerPlasmaIce] = new(SamusBeamFrameBehaviorChoice.ChargedChainsawRunsSamusState,
                SamusProjectileFrameBehavior.ChargedChainsawRunsSamusState),
            [SamusBeamCombination.SpazerPlasmaIceWave] = new(SamusBeamFrameBehaviorChoice.MurderBeamMisalignedExecution,
                SamusProjectileFrameBehavior.MurderBeamMisalignedExecution),
        });
}
