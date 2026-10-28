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

    /// <summary>Returns one shell fragment's final world-space spawn position.</summary>
    /// <remarks>
    /// Issues #625 and #975: $8B:A958 indexes six interleaved 16-bit X/Y
    /// origin pairs at $8B:A97C..A993 with index*4, then adds $10 and $3B
    /// respectively with native 16-bit wrap. Pinned NTSC J/U v1.0 ROM and
    /// bank_8B.asm give source pairs ($5C,$58), ($63,$58), ($59,$5D),
    /// ($60,$5B), ($66,$5E), and ($63,$60); all twelve words match this
    /// selector after the biases. The spawn opcode creates exactly indices
    /// 0..5, and unknown indices throw. The irregular positions compose the
    /// shell artwork, so retaining six explicit pairs is clearer than a
    /// generated sequence or polynomial encoding.
    /// </remarks>
    public static Position16<ScreenPixels> FragmentInitialPosition(int index) => Ensure.Element(FragmentInitialPositions, index);

    /// <summary>$8B:A9EA: six signed 16.16 shell-fragment horizontal velocities.</summary>
    /// <remarks>
    /// Issues #625 and #976: pinned NTSC J/U v1.0 ROM and bank_8B.asm
    /// match all six high-word-first velocity pairs at $8B:A9EA..AA01:
    /// FFFF:4000, 0000:4000, FFFF:8000, FFFF:2000, 0000:8000,
    /// 0000:2000. $8B:A994 indexes them by the immutable fragment number
    /// 0..5, scaled by four bytes; the caller adds the fraction before the
    /// signed whole word and carries across. The directions and fractional
    /// speeds vary irregularly by shell piece, so retain the six authored
    /// launch velocities. Unknown indices throw rather than read adjacent data.
    /// </remarks>
    public static SignedSixteenSixteen FragmentX(int index) => Ensure.Element(FragmentXVelocities, index);

    /// <summary>$8B:AB35: five authored horizontal slime records; four actors are normally spawned.</summary>
    /// <remarks>
    /// Issues #625 and #978: pinned NTSC J/U v1.0 ROM and bank_8B.asm
    /// match all five high-word-first signed 16.16 pairs at $8B:AB35..AB48:
    /// FFFF:0000, FFFF:8000, 0001:0000, 0000:8000, FFFF:8000.
    /// $8B:AAB3 indexes by the actor timer's low byte with a four-byte
    /// stride; the retail spawn path creates indices 0..3, while index 4 is
    /// the fifth physical table entry. The first four form opposing full-
    /// and half-pixel speeds, but the fifth repeats -0.5. Retain the five
    /// explicit launch choices rather than hide this authored extra entry
    /// behind a four-item symmetry rule. Unknown indices throw.
    /// </remarks>
    public static SignedSixteenSixteen SlimeX(int index) => Ensure.Element(SlimeXVelocities, index);
}
