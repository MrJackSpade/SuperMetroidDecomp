namespace SuperMetroid.Core.Assets;

/// <summary>
/// How a fade's colours move between where it starts and where it ends as its steps are taken (#2563, #3036): the share of
/// the way its steps make, or the cartridge's gradual colour change, which the door fades, the death fade and the time-up
/// whiteout run (<see cref="Compatibility.GradualColourChange"/>).
/// </summary>
public enum FadeArithmetic
{
    /// <summary>Each colour step moves by the fade's share of the way, truncated to a whole step.</summary>
    Proportional,

    /// <summary>
    /// The cartridge's gradual colour change (<c>CalculateTheAth_TransitionalColor_fromXtoY</c>): each step moves each colour
    /// step by what is left of the way divided by the steps left, in eight-bit fractions it then drops, so a fade moves
    /// slowly at first and catches up at its end.
    /// </summary>
    GradualColourChange,
}
