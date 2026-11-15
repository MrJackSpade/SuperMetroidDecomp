using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What the Spazer and Plasma combinations' shots run.</summary>
internal static class SamusBeamFrameBehaviorDefinitions
{
    /// <summary>
    /// What each Spazer and Plasma combination's shots run (<see cref="UnauthoredBeamFrameBehaviors{TBeams, TBehavior}"/>):
    /// uncharged, an unconnected region, the Chainsaw's Power Bomb fuse, the Spacetime overwrite and the game's live state;
    /// charged, the game's live state, Samus's changing state twice and the Murder Beam's misaligned start. A behaviour
    /// that runs data is refused, as the shot is fired or as it runs, outside the emulation scope limit (#1500).
    /// </summary>
    internal static readonly UnauthoredBeamFrameBehaviors<SamusBeamCombination, SamusShotBehavior> Unauthored = new(
        new Dictionary<SamusBeamCombination, SamusShotBehavior>
        {
            [SamusBeamCombination.SpazerPlasma] = SamusShotBehaviors.UnchargedSpazerPlasma,
            [SamusBeamCombination.SpazerPlasmaWave] = SamusShotBehaviors.ChainsawPowerBomb,
            [SamusBeamCombination.SpazerPlasmaIce] = SamusShotBehaviors.Spacetime,
            [SamusBeamCombination.SpazerPlasmaIceWave] = SamusShotBehaviors.UnchargedAllBeams,
        },
        new Dictionary<SamusBeamCombination, SamusShotBehavior>
        {
            [SamusBeamCombination.SpazerPlasma] = SamusShotBehaviors.ChargedSpazerPlasma,
            [SamusBeamCombination.SpazerPlasmaWave] = SamusShotBehaviors.ChargedChainsaw,
            [SamusBeamCombination.SpazerPlasmaIce] = SamusShotBehaviors.ChargedChainsaw,
            [SamusBeamCombination.SpazerPlasmaIceWave] = SamusShotBehaviors.MurderBeam,
        });
}
