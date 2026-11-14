using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The beam combination the Chainsaw beam's graphics belong to, and the beam palette it shows.</summary>
internal static class ChainsawBeamGraphicsDefinitions
{
    /// <summary>The Chainsaw beam's combination: Spazer, Plasma and Wave.</summary>
    internal const SamusBeamCombination Selection = SamusBeamCombination.SpazerPlasmaWave;

    /// <summary>
    /// The listed combination whose palette the Chainsaw beam shows: Plasma and Wave, the Chainsaw's beams without Spazer,
    /// which the pause screen otherwise never equips beside Plasma. The beam palette table lists no Spazer-and-Plasma
    /// combination, and the colours the original read past it are a presentation quirk the port does not keep (#1736).
    /// </summary>
    internal const SamusBeamCombination ShownPalette = SamusBeamCombination.PlasmaWave;
}
