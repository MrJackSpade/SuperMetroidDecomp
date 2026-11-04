namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>One direction, acceleration phase and fractional speed whose every whole speed the motion check adjusts.</summary>
/// <param name="Backwards">Whether the reverse figure-eight adjuster runs.</param>
/// <param name="Phase">The acceleration phase before the adjustment.</param>
/// <param name="Fraction">The fractional speed before the adjustment.</param>
internal readonly record struct PhantoonMotionCase(bool Backwards, ushort Phase, ushort Fraction);
