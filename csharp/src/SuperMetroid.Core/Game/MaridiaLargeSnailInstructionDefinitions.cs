namespace SuperMetroid.Core.Game;

/// <summary>The eight mutually exclusive facing/action animation states used by Oum.</summary>
internal enum MaridiaLargeSnailAnimation : ushort
{
    FacingRightIdle = 0,
    FacingLeftIdle = 1,
    FacingRightRollingForwards = 2,
    FacingLeftRollingForwards = 3,
    FacingRightAttacking = 4,
    FacingLeftAttacking = 5,
    FacingRightRollingBackwards = 6,
    FacingLeftRollingBackwards = 7,
}

/// <summary>Compiled instruction-list selectors for Maridia's large snail enemy, Oum.</summary>
internal static class MaridiaLargeSnailInstructionDefinitions
{
    /// <summary>
    /// <c>InstListPointers_Oum</c> at <c>$A2:CB77-$A2:CB86</c>, indexed by the
    /// mutually-exclusive animation state stored in Oum extra word $00.
    /// </summary>
    private static readonly ushort[] InstructionPointers =
    [
        0xcadb,
        0xca4b,
        0xcb1b,
        0xca8b,
        0xcae1,
        0xca51,
        0xcb43,
        0xcab3,
    ];

    /// <summary>Returns the bank-$A2 instruction list for one authored animation state.</summary>
    internal static ushort InstructionPointer(ushort animationIndex)
    {
        if (animationIndex >= InstructionPointers.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(animationIndex), animationIndex,
                "Maridia Large Snail animation index must be zero through seven.");
        }

        return InstructionPointers[animationIndex];
    }
}
