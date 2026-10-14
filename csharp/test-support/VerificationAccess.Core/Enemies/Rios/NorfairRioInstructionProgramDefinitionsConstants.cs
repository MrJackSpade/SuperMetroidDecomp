
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Cartridge values of <see cref="NorfairRioInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class NorfairRioInstructionProgramDefinitionsConstants
{
    /// <summary><c>GerutaConstants</c>, adjacent non-instruction data at $A2:C1B7.</summary>
    public const ushort AdjacentMovementDefinitions = 0xc1b7;

    /// <summary>Provides the movement-definition boundary adjacent to Norfair Rio instructions.</summary>
    extension(NorfairRioInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="NorfairRioInstructionProgramDefinitionsConstants.AdjacentMovementDefinitions"/>
        internal static ushort AdjacentMovementDefinitions => NorfairRioInstructionProgramDefinitionsConstants.AdjacentMovementDefinitions;
    }
}
