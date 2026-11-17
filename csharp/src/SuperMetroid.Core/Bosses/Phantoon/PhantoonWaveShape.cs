namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The wave parameters the wave of Phantoon's body was last built from.</summary>
/// <param name="Phase">The wave's phase.</param>
/// <param name="Amplitude">The wave's amplitude.</param>
internal readonly record struct PhantoonWaveShape(RotationAngle Phase, int Amplitude);
