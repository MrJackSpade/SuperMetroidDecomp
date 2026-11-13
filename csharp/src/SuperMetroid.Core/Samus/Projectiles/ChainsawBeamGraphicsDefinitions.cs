using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The beam combination the Chainsaw beam's graphics belong to; its colours are the ones the import decodes from past the beam palette table.</summary>
internal static class ChainsawBeamGraphicsDefinitions
{
    /// <summary>Index 13: Plasma/Spazer/Wave indexes two entries beyond the twelve-entry tile table.</summary>
    internal const SamusBeamCombination Selection = SamusBeamCombination.SpazerPlasmaWave;
}
