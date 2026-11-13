namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>The program each of Oum's animations plays.</summary>
internal static class MaridiaLargeSnailInstructionDefinitions
{
    /// <summary>The facing-specific program of each idle, roll and attack animation.</summary>
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
