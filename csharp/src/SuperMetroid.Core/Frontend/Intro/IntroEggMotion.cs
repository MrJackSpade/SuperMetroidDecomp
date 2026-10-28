using static SuperMetroid.Core.Frontend.Intro.IntroEggMotionDefinitions;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Moves the intro egg's shell fragments and slime (<see cref="IntroEggMotionDefinitions"/>): the bounded gravity curves each
/// follows, and the shell's native overread past its curve.
/// </summary>
internal static class IntroEggMotion
{
    /// <summary>$8B:AB49/AC41: 62 odd / 69 even gravity records, selected by actor parity, not frame parity.</summary>
    /// <remarks>
    /// Issues #625 and #979: the odd-actor table at $8B:AB49..AC40 has
    /// 62 high-word-first signed 16.16 records; all 124 words match pinned
    /// NTSC J/U v1.0 ROM and bank_8B.asm. For f=0..13, g=f/7 and r=f%7
    /// give whole=g-2 and fraction=0 for r=0, otherwise $E000-$2000*r.
    /// For f=14..61 the signed value is (f-14)*$2000, ending at 5.E000.
    /// $8B:AAB3 chooses this table when the actor timer's low bit is one,
    /// then indexes by its high byte; ordinary odd actors are IDs 1 and 3.
    /// In the 96-pixel start fixture, fractional starts 0, 1, $7FFF, and
    /// $FFFF all ground at frame 51, inside the table. The method rejects
    /// frame 62 and negative frames instead of reading the adjacent even
    /// table; fractional addition and whole-word carry stay exact.
    ///
    /// Issues #625 and #980: the separate even-actor table at
    /// $8B:AC41..AD54 has 69 high-word-first signed 16.16 records; all
    /// 138 words match pinned NTSC J/U v1.0 ROM and bank_8B.asm. For
    /// f=0..20, g=f/7 and r=f%7 give whole=g-3 and fraction=0 for r=0,
    /// otherwise $E000-$2000*r. For f=21..68 the signed value is
    /// (f-21)*$2000, ending at 5.E000. The timer's clear low bit selects
    /// this table for ordinary actor IDs 0 and 2, and its high byte indexes
    /// the records. The 96-pixel start fixture with fractions 0, 1, $7FFF,
    /// and $FFFF grounds at frame 62, within the table. Frame 69 and
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
    /// Shell gravity record <paramref name="frame"/>: the bounded curve's 38 records, then the three words fragments zero
    /// and one overread before the ground test deletes them. Other frames throw.
    /// </summary>
    public static SignedSixteenSixteen FragmentY(int frame) =>
        frame >= FragmentCurveFrames && frame < FragmentCurveFrames + FragmentOverreads.Length
            ? FragmentOverreads[frame - FragmentCurveFrames]
            : Curve(frame, FragmentCurveFrames, 2);
}
