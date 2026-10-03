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
    /// <remarks>
    /// Independently reviewed for #1165 against NTSC J/U v1.0 and pinned bank_A8.asm.
    /// The first three pointers happen to advance by six because their authored lists
    /// each occupy six bytes. Computing addresses would obscure the named neutral/left/
    /// right selection and couple it to instruction storage layout. The fourth value is
    /// an opcode overread, not a fourth list or a progression endpoint. Explicit cases
    /// express this fixed selection policy without retaining an indexed pointer array.
    /// </remarks>
    internal static ushort GetInitialInstruction(int parameter) => parameter switch
    {
        0 => WorkRobotInstructionProgramDefinitions.NoPowerNeutral,
        1 => WorkRobotInstructionProgramDefinitions.NoPowerLeaningLeft,
        2 => WorkRobotInstructionProgramDefinitions.NoPowerLeaningRight,
        3 => AdjacentMainAiOpcode,
        _ => throw new ArgumentOutOfRangeException(nameof(parameter)),
    };
}
