using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The beam combination the Chainsaw beam's graphics belong to; its colours are <see cref="ChainsawBeamColours{TColour}"/>.</summary>
internal static class ChainsawBeamGraphicsDefinitions
{
    /// <summary>Index 13: Plasma/Spazer/Wave indexes two words beyond the twelve-entry tile table.</summary>
    internal const SamusBeamCombination Selection = SamusBeamCombination.SpazerPlasmaWave;
}
