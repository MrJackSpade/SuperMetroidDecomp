namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>The program each of Oum's animations plays.</summary>
internal static class MaridiaLargeSnailAnimationDefinitions
{
    /// <summary>The facing-specific program of each idle, roll and attack animation.</summary>
    internal static readonly Dictionary<MaridiaLargeSnailAnimation, MaridiaLargeSnailSegment> Animations = new()
    {
        [MaridiaLargeSnailAnimation.FacingRightIdle] = MaridiaLargeSnailSegment.FacingRightIdle,
        [MaridiaLargeSnailAnimation.FacingLeftIdle] = MaridiaLargeSnailSegment.FacingLeftIdle,
        [MaridiaLargeSnailAnimation.FacingRightRollingForwards] = MaridiaLargeSnailSegment.FacingRightRollingForwards,
        [MaridiaLargeSnailAnimation.FacingLeftRollingForwards] = MaridiaLargeSnailSegment.FacingLeftRollingForwards,
        [MaridiaLargeSnailAnimation.FacingRightAttacking] = MaridiaLargeSnailSegment.FacingRightAttacking,
        [MaridiaLargeSnailAnimation.FacingLeftAttacking] = MaridiaLargeSnailSegment.FacingLeftAttacking,
        [MaridiaLargeSnailAnimation.FacingRightRollingBackwards] = MaridiaLargeSnailSegment.FacingRightRollingBackwards,
        [MaridiaLargeSnailAnimation.FacingLeftRollingBackwards] = MaridiaLargeSnailSegment.FacingLeftRollingBackwards,
    };
}
