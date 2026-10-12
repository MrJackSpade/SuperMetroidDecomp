namespace SuperMetroid.Core.Game;

/// <summary>The eight mutually exclusive facing/action animation states used by Oum.</summary>
internal enum MaridiaLargeSnailAnimation : ushort
{
    /// <summary>Selects the one-frame, sleeping right-facing list at $A2:CADB.</summary>
    FacingRightIdle = 0,
    /// <summary>Selects the one-frame, sleeping left-facing list at $A2:CA4B.</summary>
    FacingLeftIdle = 1,
    /// <summary>Selects the right-facing rolling loop that changes attack-rotation permission.</summary>
    FacingRightRollingForwards = 2,
    /// <summary>Selects the corresponding left-facing rolling loop at $A2:CA8B.</summary>
    FacingLeftRollingForwards = 3,
    /// <summary>Selects the right-facing attack with its splash sound and completion callback.</summary>
    FacingRightAttacking = 4,
    /// <summary>Selects the left-facing attack with its splash sound and completion callback.</summary>
    FacingLeftAttacking = 5,
    /// <summary>Selects the right-facing backward roll that loops to its own first frame.</summary>
    FacingRightRollingBackwards = 6,
    /// <summary>Selects the left-facing backward roll that loops to its own first frame.</summary>
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
