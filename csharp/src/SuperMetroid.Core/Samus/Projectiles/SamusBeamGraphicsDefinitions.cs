using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The beam combinations with beam graphics of their own (#1736), each with the combination whose shade its beams show:
/// the twelve combinations equipment makes, each in its own shade; the Chainsaw beam, which shows the shade of the
/// combination it shares its colours with; and the Spacetime beam, in its own. Spazer with Plasma, alone or with Ice and
/// Wave together, has no beam graphics.
/// </summary>
internal static partial class SamusBeamGraphicsDefinitions
{
    /// <summary>Each combination with beam graphics, with the combination whose shade its beams show.</summary>
    internal static readonly IReadOnlyDictionary<SamusBeamCombination, SamusBeamCombination> ShownShades =
        new Dictionary<SamusBeamCombination, SamusBeamCombination>
        {
            [SamusBeamCombination.Power] = SamusBeamCombination.Power,
            [SamusBeamCombination.Wave] = SamusBeamCombination.Wave,
            [SamusBeamCombination.Ice] = SamusBeamCombination.Ice,
            [SamusBeamCombination.IceWave] = SamusBeamCombination.IceWave,
            [SamusBeamCombination.Spazer] = SamusBeamCombination.Spazer,
            [SamusBeamCombination.SpazerWave] = SamusBeamCombination.SpazerWave,
            [SamusBeamCombination.SpazerIce] = SamusBeamCombination.SpazerIce,
            [SamusBeamCombination.SpazerIceWave] = SamusBeamCombination.SpazerIceWave,
            [SamusBeamCombination.Plasma] = SamusBeamCombination.Plasma,
            [SamusBeamCombination.PlasmaWave] = SamusBeamCombination.PlasmaWave,
            [SamusBeamCombination.PlasmaIce] = SamusBeamCombination.PlasmaIce,
            [SamusBeamCombination.PlasmaIceWave] = SamusBeamCombination.PlasmaIceWave,
            [ChainsawBeamGraphicsDefinitions.Selection] = ChainsawBeamGraphicsDefinitions.ShownBeamShade,
            [SpacetimeBeamGraphicsDefinitions.Selection] = SpacetimeBeamGraphicsDefinitions.Selection,
        };
}
