namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's wave as it stands (#1353, #1468): BG2's horizontal scroll, rippled down the whole screen by a sine wave of
/// this amplitude whose angle at the screen's first line is <see cref="Phase"/> and which repeats every
/// <see cref="Period"/> lines. The display draws it as a scroll wave; the boss keeps no table of lines.
/// </summary>
/// <param name="BaseScroll">The BG2 horizontal scroll the wave moves, 0 through 65535.</param>
/// <param name="Amplitude">The wave's peak displacement in 1/256ths of a pixel, 0 through 65535.</param>
/// <param name="Phase">The wave's angle at the screen's first line.</param>
/// <param name="Period">The screen lines of one whole cycle, as the wave's mode selects.</param>
public sealed record PhantoonWave(int BaseScroll, int Amplitude, ByteAngle Phase, int Period);
