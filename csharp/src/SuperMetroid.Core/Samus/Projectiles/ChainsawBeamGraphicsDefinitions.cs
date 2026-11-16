using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The beam combination the Chainsaw beam's graphics belong to, and the beam shade it shows.</summary>
internal static class ChainsawBeamGraphicsDefinitions
{
    /// <summary>The Chainsaw beam's combination: Spazer, Plasma and Wave.</summary>
    internal const SamusBeamCombination Selection = SamusBeamCombination.SpazerPlasmaWave;

    /// <summary>
    /// The listed combination whose shade the Chainsaw beam shows: Plasma and Wave, the Chainsaw's beams without Spazer,
    /// which the pause screen otherwise never equips beside Plasma. No beam shade is authored for Spazer with
    /// Plasma (#1736).
    /// </summary>
    internal const SamusBeamCombination ShownBeamShade = SamusBeamCombination.PlasmaWave;
}
