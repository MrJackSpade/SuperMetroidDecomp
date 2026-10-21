namespace SuperMetroid.Verification.Harness;

/// <summary>The duration and colors of one lookup stream4 row.</summary>
/// <param name="Duration">The duration.</param>
/// <param name="Colors">The colors.</param>
internal readonly record struct LookupStream4Row(ushort Duration, ushort[] Colors);
