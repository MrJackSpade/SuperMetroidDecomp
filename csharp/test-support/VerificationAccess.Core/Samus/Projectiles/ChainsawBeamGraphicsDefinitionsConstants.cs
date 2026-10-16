using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Cartridge values and helpers of <see cref="ChainsawBeamGraphicsDefinitions"/> that only verification reads.</summary>
internal static class ChainsawBeamGraphicsDefinitionsConstants
{
    /// <summary>$90:8000..801E, the Samus_Animate instruction prefix read after the first open-bus byte.</summary>
    internal const int InstructionAddress = 0x908000;
}
