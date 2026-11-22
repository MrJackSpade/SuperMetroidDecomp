namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>A Nuclear Waffle angle after clamping, and whether the clamp finished the sweep.</summary>
/// <param name="Angle">The angle, held within the sweep's start and end.</param>
/// <param name="Finished">Whether the angle reached the sweep's end.</param>
internal readonly partial record struct NuclearWaffleAngleClamp(NuclearWaffleSweepAngle Angle, bool Finished);
