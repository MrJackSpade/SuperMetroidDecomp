namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Facing-specific animations installed while a Yard is airborne.</summary>
internal readonly record struct YardAirborneAnimationDefinition(
    YardSegment VisibleAnimation,
    YardSegment HidingAnimation);
