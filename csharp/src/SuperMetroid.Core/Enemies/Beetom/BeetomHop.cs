namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>The direction and length of a Beetom hop.</summary>
/// <param name="Direction">The direction.</param>
/// <param name="Length">The length.</param>
internal readonly record struct BeetomHop(HorizontalDirection Direction, BeetomHopLength Length);
