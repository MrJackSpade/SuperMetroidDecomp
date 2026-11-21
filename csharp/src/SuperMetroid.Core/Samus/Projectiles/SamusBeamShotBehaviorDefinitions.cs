using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What the Spazer and Plasma combinations' shots run.</summary>
internal static partial class SamusBeamShotBehaviorDefinitions
{
    /// <summary>
    /// What each Spazer and Plasma combination's shots do
    /// (<see cref="UnauthoredBeamShotBehaviors{TBeams, TBehavior}"/>): uncharged, an effect outside the emulation scope
    /// limit, the Chainsaw's Power Bomb fuse, the Spacetime overwrite and the game's live state; charged, the game's
    /// live state, Samus's changing state twice and the Murder Beam's misaligned start. A shot whose effect lies
    /// outside the emulation scope limit is refused, as it is fired or as it flies (#1500).
    /// </summary>
    internal static readonly UnauthoredBeamShotBehaviors<SamusBeamCombination, SamusShotBehavior> Unauthored = new(
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
