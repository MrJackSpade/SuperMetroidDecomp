namespace SuperMetroid.Core.Game;

/// <summary>Compiled initialization selectors for the deactivated Work Robot.</summary>
internal static class WorkRobotInitializationDefinitions
{
    /// <summary>
    /// The first opcode word of <c>MainAI_Robot</c> at $A8:CC36, observed when the
    /// native four-entry range check admits the table's otherwise out-of-range selector 3.
    /// </summary>
    private const ushort AdjacentMainAiOpcode = 0x54ae;

    /// <summary>
    /// $A8:CC30..CC37: three authored instruction-list pointers followed by the
    /// first code word of MainAI_Robot. The native range check accepts parameter
    /// three, so that adjacent-code observation remains part of the definition.
    /// </summary>
    private static ReadOnlySpan<ushort> InitialInstructionLists =>
        [
            WorkRobotInstructionProgramDefinitions.NoPowerNeutral,
            WorkRobotInstructionProgramDefinitions.NoPowerLeaningLeft,
            WorkRobotInstructionProgramDefinitions.NoPowerLeaningRight,
            AdjacentMainAiOpcode,
        ];

    /// <summary>Returns the native initial instruction word for sanitized parameter zero through three.</summary>
    internal static ushort GetInitialInstruction(int parameter)
    {
        if ((uint)parameter >= InitialInstructionLists.Length)
            throw new ArgumentOutOfRangeException(nameof(parameter));
        return InitialInstructionLists[parameter];
    }
}
