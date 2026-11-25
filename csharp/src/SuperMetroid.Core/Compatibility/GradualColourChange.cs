namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The cartridge's gradual colour change of one colour step (#3036): what <c>Advance_GradualColorChange_ofAllPalettes</c>
/// leaves a channel at after its steps, each call computing the next transitional colour with
/// <c>CalculateTheAth_TransitionalColorComponent_fromXtoY</c>.
/// </summary>
/// <remarks>
/// Call <c>t</c> of a fade of <c>n</c> parts (its palette change denominator plus one) leaves the channel where it is at
/// <c>t = 0</c>, sets the goal at <c>t = n</c>, and otherwise adds what is left of the way divided by the parts left,
/// <c>n - t</c>: the distance in eight-bit fractions (<c>|goal - current| * 256 / (n - t)</c>) is added to or taken from
/// the channel's step held as its eight-bit fraction's whole, whose fraction is dropped. A rising channel so moves by the
/// whole steps of the share, and a falling one by one more where the share leaves a fraction.
/// </remarks>
public static partial class GradualColourChange
{
    /// <summary>
    /// The step a channel shows after <paramref name="step"/> calls of a fade of <paramref name="parts"/> parts from
    /// <paramref name="from"/> toward <paramref name="to"/>.
    /// </summary>
    /// <param name="from">The channel's step as the fade starts, 0-<see cref="FadeLevelColour.FullLevel"/>.</param>
    /// <param name="to">The step the fade ends on, 0-<see cref="FadeLevelColour.FullLevel"/>.</param>
    /// <param name="step">The calls taken after the first, which moves nothing: 0 through <paramref name="parts"/>.</param>
    /// <param name="parts">The parts the fade moves in, one or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">A step lies outside its range.</exception>
    public static int Channel(int from, int to, int step, int parts)
    {
        Ensure.BetweenInclusive(0, FadeLevelColour.FullLevel, from);
        Ensure.BetweenInclusive(0, FadeLevelColour.FullLevel, to);
        Ensure.BetweenInclusive(1, int.MaxValue, parts);
        Ensure.BetweenInclusive(0, parts, step);
        if (step == parts)
            return to;
        int shown = from;
        for (int call = 1; call <= step; call++)
        {
            int distance = to - shown;
            int share = Math.Abs(distance) * 256 / (parts - call);
            shown = distance >= 0 ? (shown * 256 + share) >> 8 : (shown * 256 - share) >> 8;
        }
        return shown;
    }
}
