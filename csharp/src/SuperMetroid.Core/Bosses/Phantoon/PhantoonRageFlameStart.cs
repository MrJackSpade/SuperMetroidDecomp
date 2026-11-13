namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Where an enraged flame starts on its circle round Phantoon's body, and which way it circles.</summary>
/// <param name="Angle">The flame's starting angle round the body.</param>
/// <param name="Turn">The way the flame circles the body as its circle widens.</param>
internal readonly record struct PhantoonRageFlameStart(TrigonometryAngle Angle, RotationDirection Turn);
