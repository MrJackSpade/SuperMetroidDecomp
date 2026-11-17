namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The Alcoon's walking, fireball and airborne animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="AlcoonAnimations"/>.
/// </summary>
internal enum AlcoonSegment
{
    /// <summary>The walk to the left: its move cue and four strides, one cycle.</summary>
    WalkingLeft,
    /// <summary>
    /// Facing left, its throat glowing as it opens its mouth to spit three fireballs, level, upward and downward,
    /// before it walks again.
    /// </summary>
    FireLeft,
    /// <summary>Facing left in its leaping pose, held while it is in the air.</summary>
    AirborneLeftLookingUp,
    /// <summary>Facing left with its far leg planted, held while it is in the air.</summary>
    AirborneLeftLookingForward,
    /// <summary>The walk to the right: its move cue and four strides, one cycle.</summary>
    WalkingRight,
    /// <summary>
    /// Facing right, its throat glowing as it opens its mouth to spit three fireballs, level, upward and downward,
    /// before it walks again.
    /// </summary>
    FireRight,
    /// <summary>Facing right in its leaping pose, held while it is in the air.</summary>
    AirborneRightLookingUp,
    /// <summary>Facing right with its far leg planted, held while it is in the air.</summary>
    AirborneRightLookingForward,
}
