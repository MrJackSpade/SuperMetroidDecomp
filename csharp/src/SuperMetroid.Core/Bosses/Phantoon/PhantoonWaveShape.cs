namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The wave parameters Phantoon's per-line wave was last built from.</summary>
/// <param name="Phase">The wave's phase.</param>
/// <param name="Amplitude">The wave's amplitude.</param>
/// <param name="BaseScroll">The scroll the wave offsets.</param>
internal readonly record struct PhantoonWaveShape(int Phase, int Amplitude, int BaseScroll);
