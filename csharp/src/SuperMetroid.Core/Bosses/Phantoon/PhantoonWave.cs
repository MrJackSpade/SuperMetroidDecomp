namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's wave as it stands (#1353, #1468, #1858): his body where it lies in the room, rippled sideways down the whole
/// screen by a sine wave of
/// this amplitude whose angle at the screen's first line is <see cref="Phase"/> and which repeats every
/// <see cref="Period"/> lines. The display draws it as a scroll wave; the boss keeps no table of lines.
/// </summary>
/// <param name="BodyLeft">The room column his body's area's left edge lies on, which the wave moves sideways.</param>
/// <param name="Amplitude">The wave's peak displacement in 1/256ths of a pixel.</param>
/// <param name="Phase">The wave's angle at the screen's first line.</param>
/// <param name="Period">The screen lines of one whole cycle, as the wave's length selects.</param>
public sealed record PhantoonWave(int BodyLeft, int Amplitude, TrigonometryAngle Phase, int Period);
