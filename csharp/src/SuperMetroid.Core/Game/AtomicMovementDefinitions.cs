namespace SuperMetroid.Core.Game;

/// <summary>Compiled initialization selectors for Atomic's four movement appearances.</summary>
internal static class AtomicMovementDefinitions
{
    /// <summary>
    /// $A8:E380, InstructionListPointers_Atomic: four instruction-list pointers
    /// selected by the enemy population's first parameter. Named direction cases
    /// select the independently calculated control programs.
    /// </summary>
    internal static ushort InitialInstructionList(ushort parameter) => parameter switch
    {
        0 => AtomicInstructionProgramDefinitions.UpRight,
        1 => AtomicInstructionProgramDefinitions.UpLeft,
        2 => AtomicInstructionProgramDefinitions.DownLeft,
        3 => AtomicInstructionProgramDefinitions.DownRight,
        _ => throw new InvalidDataException(
            $"Atomic instruction selector ${parameter:X4} is outside the four authored appearances."),
    };
}
