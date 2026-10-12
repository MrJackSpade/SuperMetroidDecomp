namespace SuperMetroid.Core.Game;

/// <summary>The eight mutually exclusive facing/action animation states used by Oum.</summary>
internal enum MaridiaLargeSnailAnimation : ushort
{
    /// <summary>Identifies facing right idle.</summary>
    FacingRightIdle = 0,
    /// <summary>Identifies facing left idle.</summary>
    FacingLeftIdle = 1,
    /// <summary>Identifies facing right rolling forwards.</summary>
    FacingRightRollingForwards = 2,
    /// <summary>Identifies facing left rolling forwards.</summary>
    FacingLeftRollingForwards = 3,
    /// <summary>Identifies facing right attacking.</summary>
    FacingRightAttacking = 4,
    /// <summary>Identifies facing left attacking.</summary>
    FacingLeftAttacking = 5,
    /// <summary>Identifies facing right rolling backwards.</summary>
    FacingRightRollingBackwards = 6,
    /// <summary>Identifies facing left rolling backwards.</summary>
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