namespace SuperMetroid.Verification.Hardware;

/// <summary>The left and right of one interval.</summary>
/// <param name="Left">The left.</param>
/// <param name="Right">The right.</param>
internal readonly record struct Interval(byte Left, byte Right);
