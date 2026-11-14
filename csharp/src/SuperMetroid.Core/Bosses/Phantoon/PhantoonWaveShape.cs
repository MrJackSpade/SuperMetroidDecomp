namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The wave parameters the wave of Phantoon's body was last built from.</summary>
/// <param name="Phase">The wave's phase.</param>
/// <param name="Amplitude">The wave's amplitude.</param>
/// <param name="BodyLeft">The room column his body's area's left edge lies on, which the wave ripples sideways.</param>
internal readonly record struct PhantoonWaveShape(int Phase, int Amplitude, int BodyLeft);
