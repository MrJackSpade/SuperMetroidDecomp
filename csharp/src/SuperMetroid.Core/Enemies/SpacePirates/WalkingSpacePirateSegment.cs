namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The walking Space Pirates' walk, look, flinch and laser animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="WalkingSpacePirateAnimations"/>.
/// </summary>
internal enum WalkingSpacePirateSegment
{
    /// <summary>Flinching facing left, then walking left again.</summary>
    FlinchFacingLeft,
    /// <summary>Flinching facing right, then walking right again.</summary>
    FlinchFacingRight,
    /// <summary>Setting off to the left, then striding left, repeating.</summary>
    WalkingLeft,
    /// <summary>The stride walking left, looped.</summary>
    WalkingLeftStride,
    /// <summary>
    /// Standing facing left, lifting its claws to fire three lasers left, high, middle and low, then lowering them and
    /// looking around before it walks right.
    /// </summary>
    FireLasersLeft,
    /// <summary>
    /// Standing facing left, looking at the viewer, away and back, then facing the viewer before it walks right.
    /// </summary>
    LookingFacingLeft,
    /// <summary>Setting off to the right, then striding right, repeating.</summary>
    WalkingRight,
    /// <summary>The stride walking right, looped.</summary>
    WalkingRightStride,
    /// <summary>
    /// Standing facing right, lifting its claws to fire three lasers right, high, middle and low, then lowering them
    /// and looking around before it walks left.
    /// </summary>
    FireLasersRight,
    /// <summary>
    /// Standing facing right, looking at the viewer, away and back, then facing the viewer before it walks left.
    /// </summary>
    LookingFacingRight,
}
