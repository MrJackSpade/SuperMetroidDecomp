namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The angle and value of one sine sample.</summary>
/// <param name="Angle">The angle.</param>
/// <param name="Value">The value.</param>
internal readonly record struct SineSample(byte Angle, short Value);
