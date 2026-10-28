namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Animation selectors for Maridia's large snail enemy, Oum.</summary>
internal static class MaridiaLargeSnailInstructionDefinitions
{
    /// <summary>The eight animations in the order the authored animation index counts them.</summary>
    internal static readonly IdentitySequence<MaridiaLargeSnailAnimation> Animations = new(
        MaridiaLargeSnailAnimation.FacingRightIdle, MaridiaLargeSnailAnimation.FacingLeftIdle,
        MaridiaLargeSnailAnimation.FacingRightRollingForwards, MaridiaLargeSnailAnimation.FacingLeftRollingForwards,
        MaridiaLargeSnailAnimation.FacingRightAttacking, MaridiaLargeSnailAnimation.FacingLeftAttacking,
        MaridiaLargeSnailAnimation.FacingRightRollingBackwards, MaridiaLargeSnailAnimation.FacingLeftRollingBackwards);

    /// <summary>$A2:CB77 InstListPointers_Oum: the facing-specific program of each idle, roll and attack state.</summary>
    internal static readonly Dictionary<MaridiaLargeSnailAnimation, MaridiaLargeSnailProgram> Programs = new()
    {
        [MaridiaLargeSnailAnimation.FacingRightIdle] = MaridiaLargeSnailProgram.FacingRightIdle,
        [MaridiaLargeSnailAnimation.FacingLeftIdle] = MaridiaLargeSnailProgram.FacingLeftIdle,
        [MaridiaLargeSnailAnimation.FacingRightRollingForwards] = MaridiaLargeSnailProgram.FacingRightRollingForwards,
        [MaridiaLargeSnailAnimation.FacingLeftRollingForwards] = MaridiaLargeSnailProgram.FacingLeftRollingForwards,
        [MaridiaLargeSnailAnimation.FacingRightAttacking] = MaridiaLargeSnailProgram.FacingRightAttacking,
        [MaridiaLargeSnailAnimation.FacingLeftAttacking] = MaridiaLargeSnailProgram.FacingLeftAttacking,
        [MaridiaLargeSnailAnimation.FacingRightRollingBackwards] = MaridiaLargeSnailProgram.FacingRightRollingBackwards,
        [MaridiaLargeSnailAnimation.FacingLeftRollingBackwards] = MaridiaLargeSnailProgram.FacingLeftRollingBackwards,
    };
}
