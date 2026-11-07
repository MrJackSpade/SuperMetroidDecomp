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
    /// 62 high-word-first signed 16.16 records; all 124 words match
    /// the original data. For f=0..13, g=f/7 and r=f%7
    /// give whole=g-2 and fraction=0 for r=0, otherwise 57344-8192*r.
    /// For f=14..61 the signed value is (f-14)*8192, ending at 5.E000.
    /// The slime motion chooses this table when the actor timer's low bit is one,
    /// then indexes by its high byte; ordinary odd actors are IDs 1 and 3.
    /// In the 96-pixel start fixture, fractional starts 0, 1, 32767, and
    /// 65535 all ground at frame 51, inside the table. The method rejects
    /// frame 62 and negative frames instead of reading the adjacent even
    /// table; fractional addition and whole-word carry stay exact.
    ///
    /// Issues #625 and #980: the separate even-actor table
    /// has 69 high-word-first signed 16.16 records; all
    /// 138 words match the original data. For
    /// f=0..20, g=f/7 and r=f%7 give whole=g-3 and fraction=0 for r=0,
    /// otherwise 57344-8192*r. For f=21..68 the signed value is
    /// (f-21)*8192, ending at 5.E000. The timer's clear low bit selects
    /// this table for ordinary actor IDs 0 and 2, and its high byte indexes
    /// the records. The 96-pixel start fixture with fractions 0, 1, 32767,
    /// and 65535 grounds at frame 62, within the table. Frame 69 and
    /// negative frames throw instead of reading the adjacent routine.
    /// </remarks>
    public static SignedSixteenSixteen SlimeY(int frame, bool odd) => Curve(frame, odd ? 62 : 69, odd ? 2 : 3);

    /// <summary>Produces one bounded signed 16.16 gravity step for shell or parity-selected slime trajectories.</summary>
    private static SignedSixteenSixteen Curve(int frame, int count, int negativeGroups)
    {
        if ((uint)frame >= count)
            throw new ArgumentOutOfRangeException(nameof(frame));
        int prefix = negativeGroups * 7;
        if (frame >= prefix)
            return Split((frame - prefix) * GravityStep);
        int within = frame % 7;
        int fraction = within == 0 ? 0 : OnePixel - GravityStep - within * GravityStep;
        return Split((frame / 7 - negativeGroups) * OnePixel + fraction);
    }

    /// <summary>
    /// Shell gravity step <paramref name="frame"/>: the bounded curve's 38 records, then the steps fragments zero and one
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
    /// Issues #625 and #975: the fragment spawn indexes six interleaved 16-bit X/Y
    /// origin pairs with index*4, then adds 16 and 59
    /// respectively with sixteen-bit wrap. The original data
    /// gives source pairs (92,88), (99,88), (89,93),
    /// (96,91), (102,94), and (99,96); all twelve words match this
    /// selector after the biases. The spawn opcode creates exactly indices
    /// 0..5, and unknown indices throw. The irregular positions compose the
    /// shell artwork, so retaining six explicit pairs is clearer than a
    /// generated sequence or polynomial encoding.
    /// </remarks>
    public static Position16<ScreenPixels> FragmentInitialPosition(int index) => Ensure.Element(FragmentInitialPositions, index);

    /// <summary>Six signed 16.16 shell-fragment horizontal velocities.</summary>
    /// <remarks>
    /// Issues #625 and #976: the original data
    /// match all six high-word-first velocity pairs:
    /// FFFF:4000, 0000:4000, FFFF:8000, FFFF:2000, 0000:8000,
    /// 0000:2000. The fragment motion indexes them by the immutable fragment number
    /// 0..5, scaled by four bytes; the caller adds the fraction before the
    /// signed whole word and carries across. The directions and fractional
    /// speeds vary irregularly by shell piece, so retain the six authored
    /// launch velocities. Unknown indices throw rather than read adjacent data.
    /// </remarks>
    public static SignedSixteenSixteen FragmentX(int index) => Ensure.Element(FragmentXVelocities, index);

    /// <summary>Five authored horizontal slime records; four actors are normally spawned.</summary>
    /// <remarks>
    /// Issues #625 and #978: the original data
    /// match all five high-word-first signed 16.16 pairs:
    /// FFFF:0000, FFFF:8000, 0001:0000, 0000:8000, FFFF:8000.
    /// The slime motion indexes by the actor timer's low byte with a four-byte
    /// stride; the retail spawn path creates indices 0..3, while index 4 is
    /// the fifth physical table entry. The first four form opposing full-
    /// and half-pixel speeds, but the fifth repeats -0.5. Retain the five
    /// explicit launch choices rather than hide this authored extra entry
    /// behind a four-item symmetry rule. Unknown indices throw.
    /// </remarks>
    public static SignedSixteenSixteen SlimeX(int index) => Ensure.Element(SlimeXVelocities, index);
}
