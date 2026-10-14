
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="GoldenTorizoRightwardInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoRightwardInstructionProgramDefinitionsConstants
{
    /// <summary>Instruction boundary at $D369 after the right-facing dodge program.</summary>
    public const ushort End = 0xd369;
    /// <summary>First right-facing dodge instruction, shared with the combat pointer catalog.</summary>
    public const ushort Start = GoldenTorizoCombatInstructionPointers.DodgeTurningRight;

    /// <summary>Provides the native start and end of Golden Torizo's rightward instruction program.</summary>
    extension(GoldenTorizoRightwardInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoRightwardInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoRightwardInstructionProgramDefinitionsConstants.End;
        /// <inheritdoc cref="GoldenTorizoRightwardInstructionProgramDefinitionsConstants.Start"/>
        internal static ushort Start => GoldenTorizoRightwardInstructionProgramDefinitionsConstants.Start;
    }
}
