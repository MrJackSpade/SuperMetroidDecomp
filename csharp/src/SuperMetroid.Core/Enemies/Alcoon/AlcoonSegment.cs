namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The Alcoon's walking, fireball and airborne animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="AlcoonAnimations"/>.
/// </summary>
internal enum AlcoonSegment
{
    /// <summary>The walking left animation.</summary>
    WalkingLeft,
    /// <summary>The walking left loop animation.</summary>
    WalkingLeftLoop,
    /// <summary>The fire left animation.</summary>
    FireLeft,
    /// <summary>The airborne left looking up animation.</summary>
    AirborneLeftLookingUp,
    /// <summary>The airborne left looking forward animation.</summary>
    AirborneLeftLookingForward,
    /// <summary>The walking right animation.</summary>
    WalkingRight,
    /// <summary>The walking right loop animation.</summary>
    WalkingRightLoop,
    /// <summary>The fire right animation.</summary>
    FireRight,
    /// <summary>The airborne right looking up animation.</summary>
    AirborneRightLookingUp,
    /// <summary>The airborne right looking forward animation.</summary>
    AirborneRightLookingForward,
}
