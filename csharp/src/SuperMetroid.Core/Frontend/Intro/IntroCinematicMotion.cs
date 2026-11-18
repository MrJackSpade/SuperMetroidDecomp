namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Exact fixed-point additions shared by opening-cinematic actors, on signed screen places (#2000).</summary>
internal static class IntroCinematicMotion
{

    /// <summary>
    /// Adds a signed 1/256-pixel velocity to one axis: the velocity's fraction lands in the upper half of the axis's
    /// fraction, and its overflow past a whole pixel carries into the whole pixels.
    /// </summary>
    public static SignedSixteenSixteen AddEightEight(SignedSixteenSixteen position, SignedEightEight velocity) =>
        position + velocity.ToSixteenSixteenDelta();
}
