namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>Compiled initialization selectors for Atomic's four movement appearances.</summary>
internal static class AtomicMovementDefinitions
{
    /// <summary>
    /// $A8:E380, InstructionListPointers_Atomic: the four spinning animations, selected by the enemy population's first
    /// parameter.
    /// </summary>
    internal static AtomicProgram InitialProgram(int parameter) => parameter switch
    {
        0 => AtomicProgram.UpRight,
        1 => AtomicProgram.UpLeft,
        2 => AtomicProgram.DownLeft,
        3 => AtomicProgram.DownRight,
        _ => throw new InvalidDataException(
            $"Atomic instruction selector ${parameter:X4} is outside the four authored appearances."),
    };
}
