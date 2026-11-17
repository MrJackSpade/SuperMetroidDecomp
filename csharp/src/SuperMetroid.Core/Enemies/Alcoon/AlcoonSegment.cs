namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// The Alcoon's walking, fireball and airborne animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="AlcoonAnimations"/>.
/// </summary>
internal enum AlcoonSegment
{
    /// <summary>The walk to the left: its move cue and four strides, one cycle.</summary>
    WalkingLeft,
    /// <summary>The fire left animation.</summary>
    FireLeft,
    /// <summary>The airborne left looking up animation.</summary>
    AirborneLeftLookingUp,
    /// <summary>The airborne left looking forward animation.</summary>
    AirborneLeftLookingForward,
    /// <summary>The walk to the right: its move cue and four strides, one cycle.</summary>
    WalkingRight,
    /// <summary>The fire right animation.</summary>
    FireRight,
    /// <summary>The airborne right looking up animation.</summary>
    AirborneRightLookingUp,
    /// <summary>The airborne right looking forward animation.</summary>
    AirborneRightLookingForward,
}
