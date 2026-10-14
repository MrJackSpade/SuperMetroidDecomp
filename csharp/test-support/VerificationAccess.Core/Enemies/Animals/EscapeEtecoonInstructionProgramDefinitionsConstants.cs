
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Cartridge values of <see cref="EscapeEtecoonInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class EscapeEtecoonInstructionProgramDefinitionsConstants
{
    /// <summary><c>Instruction_EtecoonEscape_XPositionPlusY</c>, adjacent code at $B3:E610.</summary>
    public const ushort FirstAdjacentCodeRoutine = 0xe610;

    /// <summary>Provides the neighboring code boundary for escape Etecoon instruction extraction.</summary>
    extension(EscapeEtecoonInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="EscapeEtecoonInstructionProgramDefinitionsConstants.FirstAdjacentCodeRoutine"/>
        internal static ushort FirstAdjacentCodeRoutine => EscapeEtecoonInstructionProgramDefinitionsConstants.FirstAdjacentCodeRoutine;
    }
}
