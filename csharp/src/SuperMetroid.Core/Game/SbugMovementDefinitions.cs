namespace SuperMetroid.Core.Game;

/// <summary>Compiled facing-program and activation-function selectors for Sbug movement.</summary>
internal static class SbugMovementDefinitions
{
    /// <summary>
    /// $A3:A111, InstructionListPointers_Sbug: eight instruction-list pointers
    /// selected by the direction index. The mixed instruction programs remain
    /// separate runtime dependencies.
    /// </summary>
    private static ReadOnlySpan<ushort> FacingInstructionLists =>
    [
        SbugInstructionProgramDefinitions.Right,
        SbugInstructionProgramDefinitions.UpRight,
        SbugInstructionProgramDefinitions.Up,
        SbugInstructionProgramDefinitions.UpLeft,
        SbugInstructionProgramDefinitions.Left,
        SbugInstructionProgramDefinitions.DownLeft,
        SbugInstructionProgramDefinitions.Down,
        SbugInstructionProgramDefinitions.DownRight,
    ];

    /// <summary>
    /// $A3:A121, ActivationFunctionPointers_Sbug: seven same-bank function
    /// identities selected by the high byte of population parameter two.
    /// </summary>
    private static ReadOnlySpan<SbugEnemyFunction> ActivationFunctions =>
    [
        SbugEnemyFunction.ActivateMoveForward,
        SbugEnemyFunction.ActivateZigZag,
        SbugEnemyFunction.ActivateMoveTowardSamus,
        SbugEnemyFunction.ActivateRandomUntilCollision,
        SbugEnemyFunction.ActivateRandomAndReverseWhenFar,
        SbugEnemyFunction.ActivateMoveForwardThenWait,
        SbugEnemyFunction.ActivateMoveAwayFromSamus,
    ];

    internal static ushort FacingInstructionList(ushort directionIndex)
    {
        if (directionIndex > 0x000f)
        {
            throw new InvalidDataException(
                $"Sbug direction index ${directionIndex:X4} is outside the eight native selectors.");
        }

        // Native code converts the byte offset to an element index before reading a word,
        // so the cartridge's odd reversal indexes intentionally discard bit zero.
        return FacingInstructionLists[directionIndex >> 1];
    }

    internal static SbugEnemyFunction ActivationFunction(SbugActivationBehavior behavior)
    {
        byte index = (byte)behavior;
        if (index >= ActivationFunctions.Length)
        {
            throw new InvalidDataException(
                $"Sbug activation behavior {index} is outside the seven native selectors.");
        }

        return ActivationFunctions[index];
    }
}
