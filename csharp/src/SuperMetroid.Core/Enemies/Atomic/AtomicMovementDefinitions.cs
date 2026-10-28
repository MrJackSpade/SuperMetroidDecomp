namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>Compiled initialization selectors for Atomic's four movement appearances.</summary>
internal static class AtomicMovementDefinitions
{
    /// <summary>
    /// $A8:E380, InstructionListPointers_Atomic: the four spinning animations, indexed by the enemy population's first
    /// parameter.
    /// </summary>
    internal static readonly AtomicProgram[] InitialPrograms =
        [AtomicProgram.UpRight, AtomicProgram.UpLeft, AtomicProgram.DownLeft, AtomicProgram.DownRight];
}
