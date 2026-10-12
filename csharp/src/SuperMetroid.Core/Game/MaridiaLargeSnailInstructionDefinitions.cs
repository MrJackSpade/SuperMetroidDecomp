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
    /// $A2:CB77 InstListPointers_Oum: named idle, roll and attack states select
    /// their facing-specific programs. The complete domain is zero through seven.
    /// </summary>
    internal static ushort InstructionPointer(ushort animationIndex) =>
        (MaridiaLargeSnailAnimation)animationIndex switch
        {
            MaridiaLargeSnailAnimation.FacingRightIdle => MaridiaLargeSnailInstructionProgramDefinitions.FacingRightIdle,
            MaridiaLargeSnailAnimation.FacingLeftIdle => MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftIdle,
            MaridiaLargeSnailAnimation.FacingRightRollingForwards => MaridiaLargeSnailInstructionProgramDefinitions.FacingRightRollingForwards,
            MaridiaLargeSnailAnimation.FacingLeftRollingForwards => MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftRollingForwards,
            MaridiaLargeSnailAnimation.FacingRightAttacking => MaridiaLargeSnailInstructionProgramDefinitions.FacingRightAttacking,
            MaridiaLargeSnailAnimation.FacingLeftAttacking => MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftAttacking,
            MaridiaLargeSnailAnimation.FacingRightRollingBackwards => MaridiaLargeSnailInstructionProgramDefinitions.FacingRightRollingBackwards,
            MaridiaLargeSnailAnimation.FacingLeftRollingBackwards => MaridiaLargeSnailInstructionProgramDefinitions.FacingLeftRollingBackwards,
            _ => throw new ArgumentOutOfRangeException(nameof(animationIndex), animationIndex,
                "Maridia Large Snail animation index must be zero through seven."),
        };
}