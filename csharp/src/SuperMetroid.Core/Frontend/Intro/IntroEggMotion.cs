using static SuperMetroid.Core.Frontend.Intro.IntroEggMotionDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Moves the intro egg's shell fragments and slime (<see cref="IntroEggMotionDefinitions"/>): the bounded gravity curves each
/// follows, and the shell's steps past its curve, which the installed <see cref="ShellFragmentFallPastCurve"/> holds.
/// </summary>
internal static class IntroEggMotion
{
    /// <summary>62 odd / 69 even gravity records, selected by actor parity, not frame parity.</summary>
    /// <remarks>
    /// Issues #625 and #979: the odd-actor table has
    /// 62 signed 1/65536-pixel records matching
    /// the authored data. For f=0..13, g=f/7 and r=f%7
    /// give whole=g-2 and fraction=0 for r=0, otherwise (7-r) eighths of a pixel.
    /// For f=14..61 the signed value is (f-14) eighths of a pixel, ending at five and seven eighths.
    /// The slime motion chooses this table when the actor timer is odd,
    /// then indexes by the timer over 256; ordinary odd actors are IDs 1 and 3.
    /// In the 96-pixel start fixture, fractional starts from zero to just under a
    /// whole pixel all ground at frame 51, inside the table. The method rejects
    /// frame 62 and negative frames instead of reading the adjacent even
    /// table; fractional addition and the overflow into the whole part stay exact.
    ///
    /// Issues #625 and #980: the separate even-actor table
    /// has 69 signed 1/65536-pixel records matching the authored data. For
    /// f=0..20, g=f/7 and r=f%7 give whole=g-3 and fraction=0 for r=0,
    /// otherwise (7-r) eighths of a pixel. For f=21..68 the signed value is
    /// (f-21) eighths of a pixel, ending at five and seven eighths. An even timer selects
    /// this table for ordinary actor IDs 0 and 2, and the timer over 256 indexes
    /// the records. The 96-pixel start fixture with fractions from zero to just
    /// under a whole pixel grounds at frame 62, within the table. Frame 69 and
    /// negative frames throw instead of reading adjacent data.
    /// </remarks>
    public static SignedSixteenSixteen SlimeY(int frame, bool odd) => Curve(frame, odd ? 62 : 69, odd ? 2 : 3);

    /// <summary>Produces one bounded signed 1/65536-pixel gravity step for shell or parity-selected slime trajectories.</summary>
    private static SignedSixteenSixteen Curve(int frame, int count, int negativeGroups)
    {
        if ((uint)frame >= count)
            throw new ArgumentOutOfRangeException(nameof(frame));
        int prefix = negativeGroups * 7;
        if (frame >= prefix)
            return GravityStep * (frame - prefix);
        // Each group of seven frames starts on a whole pixel, then the rest of the group falls back from the next
        // pixel by an eighth of a pixel a frame.
        int within = frame % 7;
        return SignedSixteenSixteen.FromWholePixels(frame / 7 - negativeGroups) + GravityStep * (within == 0 ? 0 : 7 - within);
    }

    /// <summary>
    /// Shell gravity step <paramref name="frame"/>: the bounded curve's 38 records, then the steps the upper fragments
    /// read past it before the ground test deletes them, from <paramref name="pastCurve"/>. Negative frames throw.
    /// </summary>
    /// <param name="frame">The frames the fragment has fallen.</param>
    /// <param name="pastCurve">The installed steps a fragment falls by past the curve.</param>
    public static SignedSixteenSixteen FragmentY(int frame, ShellFragmentFallPastCurve pastCurve) =>
        frame >= FragmentCurveFrames
            ? Ensure.NotNull(pastCurve).Step(frame - FragmentCurveFrames)
            : Curve(frame, FragmentCurveFrames, 2);

    /// <summary>Returns one shell fragment's final world-space spawn position.</summary>
    /// <remarks>
    /// Issues #625 and #975: the fragment spawn reads six X/Y
    /// origin pairs, then adds 16 and 59
    /// respectively. The authored data
    /// gives source pairs (92,88), (99,88), (89,93),
    /// (96,91), (102,94), and (99,96); all twelve values match this
    /// selector after the biases. The spawn step creates each fragment once,
    /// and an unknown fragment throws. The irregular positions compose the
    /// shell artwork, so retaining six explicit pairs is clearer than a
    /// generated sequence or polynomial encoding.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fragment"/> is not a piece of the shell.</exception>
    public static Position16<ScreenPixels> FragmentInitialPosition(IntroEggShellFragment fragment) =>
        FragmentInitialPositions.TryGetValue(fragment, out Position16<ScreenPixels> position)
            ? position : throw new ArgumentOutOfRangeException(nameof(fragment));

    /// <summary>Six signed 1/65536-pixel shell-fragment horizontal velocities.</summary>
    /// <remarks>
    /// Issues #625 and #976: the authored data
    /// match all six velocities, in pixels per frame:
    /// -0.75, 0.25, -0.5, -0.875, 0.5
    /// and 0.125, in spawn order. The fragment motion selects them by the fragment,
    /// which never changes; the caller adds the subpixels before the
    /// signed whole pixels and carries across. The directions and fractional
    /// speeds vary irregularly by shell piece, so retain the six authored
    /// launch velocities. An unknown fragment throws.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fragment"/> is not a piece of the shell.</exception>
    public static SignedSixteenSixteen FragmentX(IntroEggShellFragment fragment) =>
        FragmentXVelocities.TryGetValue(fragment, out SignedSixteenSixteen velocity)
            ? velocity : throw new ArgumentOutOfRangeException(nameof(fragment));

    /// <summary>Five authored horizontal slime records; four actors are normally spawned.</summary>
    /// <remarks>
    /// Issues #625 and #978: the authored data
    /// match all five signed 1/65536-pixel speeds, in pixels per frame:
    /// -1, -0.5, 1, 0.5, -0.5.
    /// The slime motion indexes them by the actor timer's count within its
    /// 256-frame cycle; the retail spawn path creates indices 0..3, while index 4 is
    /// the fifth authored entry. The first four form opposing full-
    /// and half-pixel speeds, but the fifth repeats -0.5. Retain the five
    /// explicit launch choices rather than hide this authored extra entry
    /// behind a four-item symmetry rule. Unknown indices throw.
    /// </remarks>
    public static SignedSixteenSixteen SlimeX(int index) => Ensure.Element(SlimeXVelocities, index);
}
