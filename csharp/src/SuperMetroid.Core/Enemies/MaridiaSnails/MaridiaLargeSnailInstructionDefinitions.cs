namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Animation selectors for Maridia's large snail enemy, Oum.</summary>
internal static class MaridiaLargeSnailInstructionDefinitions
{
    /// <summary>The eight animations in the order the authored animation index counts them.</summary>
    private static readonly IdentitySequence<MaridiaLargeSnailAnimation> Animations = new(
        MaridiaLargeSnailAnimation.FacingRightIdle, MaridiaLargeSnailAnimation.FacingLeftIdle,
        MaridiaLargeSnailAnimation.FacingRightRollingForwards, MaridiaLargeSnailAnimation.FacingLeftRollingForwards,
        MaridiaLargeSnailAnimation.FacingRightAttacking, MaridiaLargeSnailAnimation.FacingLeftAttacking,
        MaridiaLargeSnailAnimation.FacingRightRollingBackwards, MaridiaLargeSnailAnimation.FacingLeftRollingBackwards);

    /// <summary>
    /// $A2:CB77 InstListPointers_Oum: named idle, roll and attack states select
    /// their facing-specific programs. The complete domain is zero through seven.
    /// </summary>
    internal static MaridiaLargeSnailProgram Program(int animationIndex) =>
        Animations.At(animationIndex) switch
        {
            MaridiaLargeSnailAnimation.FacingRightIdle => MaridiaLargeSnailProgram.FacingRightIdle,
            MaridiaLargeSnailAnimation.FacingLeftIdle => MaridiaLargeSnailProgram.FacingLeftIdle,
            MaridiaLargeSnailAnimation.FacingRightRollingForwards => MaridiaLargeSnailProgram.FacingRightRollingForwards,
            MaridiaLargeSnailAnimation.FacingLeftRollingForwards => MaridiaLargeSnailProgram.FacingLeftRollingForwards,
            MaridiaLargeSnailAnimation.FacingRightAttacking => MaridiaLargeSnailProgram.FacingRightAttacking,
            MaridiaLargeSnailAnimation.FacingLeftAttacking => MaridiaLargeSnailProgram.FacingLeftAttacking,
            MaridiaLargeSnailAnimation.FacingRightRollingBackwards => MaridiaLargeSnailProgram.FacingRightRollingBackwards,
            MaridiaLargeSnailAnimation.FacingLeftRollingBackwards => MaridiaLargeSnailProgram.FacingLeftRollingBackwards,
            _ => throw new ArgumentOutOfRangeException(nameof(animationIndex), animationIndex,
                "Maridia Large Snail animation index must be zero through seven."),
        };
}
