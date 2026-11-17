namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's wave as it stands (#1353, #1468, #1858, #2031): his body where it is placed, rippled sideways down the whole
/// screen by a sine wave of this amplitude whose angle at the screen's first line is <see cref="Phase"/> and which repeats
/// every <see cref="Period"/> lines. The display draws it as a scroll wave about his body's placement; the boss keeps no
/// table of lines.
/// </summary>
/// <param name="Amplitude">The wave's peak displacement.</param>
/// <param name="Phase">The wave's angle at the screen's first line.</param>
/// <param name="Period">The screen lines of one whole cycle, as the wave's length selects.</param>
public sealed record PhantoonWave(UnsignedSixteenSixteen Amplitude, TrigonometryAngle Phase, int Period);
