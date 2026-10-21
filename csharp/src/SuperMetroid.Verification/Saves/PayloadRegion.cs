namespace SuperMetroid.Verification.Saves;

/// <summary>The offset, length and name of one payload region.</summary>
/// <param name="Offset">The offset.</param>
/// <param name="Length">The length.</param>
/// <param name="Name">The name.</param>
internal readonly record struct PayloadRegion(int Offset, int Length, string Name);
