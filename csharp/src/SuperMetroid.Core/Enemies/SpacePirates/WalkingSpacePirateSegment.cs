namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// The walking Space Pirates' walk, look, flinch and laser animations as typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="WalkingSpacePirateAnimations"/>.
/// </summary>
internal enum WalkingSpacePirateSegment
{
    /// <summary>The flinch facing left animation.</summary>
    FlinchFacingLeft,
    /// <summary>The flinch facing right animation.</summary>
    FlinchFacingRight,
    /// <summary>The walking left animation.</summary>
    WalkingLeft,
    /// <summary>The loop.</summary>
    WalkingLeftLoop,
    /// <summary>The fire lasers left animation.</summary>
    FireLasersLeft,
    /// <summary>The looking facing left animation.</summary>
    LookingFacingLeft,
    /// <summary>The walking right animation.</summary>
    WalkingRight,
    /// <summary>The loop.</summary>
    WalkingRightLoop,
    /// <summary>The fire lasers right animation.</summary>
    FireLasersRight,
    /// <summary>The looking facing right animation.</summary>
    LookingFacingRight,
}
