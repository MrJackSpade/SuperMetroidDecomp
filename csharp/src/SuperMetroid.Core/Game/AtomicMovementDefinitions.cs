namespace SuperMetroid.Core.Game;

/// <summary>Compiled initialization selectors for Atomic's four movement appearances.</summary>
internal static class AtomicMovementDefinitions
{
    /// <summary>
    /// $A8:E380, InstructionListPointers_Atomic: four instruction-list pointers
    /// selected by the enemy population's first parameter. The mixed instruction
    /// programs themselves remain separate runtime dependencies.
    /// </summary>
    private static ReadOnlySpan<ushort> InitialInstructionLists =>
    [
        AtomicInstructionProgramDefinitions.UpRight,
        AtomicInstructionProgramDefinitions.UpLeft,
        AtomicInstructionProgramDefinitions.DownLeft,
        AtomicInstructionProgramDefinitions.DownRight,
    ];

    internal static ushort InitialInstructionList(ushort parameter)
    {
        if (parameter >= InitialInstructionLists.Length)
        {
            throw new InvalidDataException(
                $"Atomic instruction selector ${parameter:X4} is outside the four authored appearances.");
        }

        return InitialInstructionLists[parameter];
    }
}
