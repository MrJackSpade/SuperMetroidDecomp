namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed egg-shell and slime trajectories, independent of cinematic artwork.</summary>
internal static class IntroEggMotionDefinitions
{

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

    /// <summary>The values <see cref="FragmentInitialPosition"/> selects, in index order.</summary>
    private static readonly Position16<ScreenPixels>[] FragmentInitialPositions =
    [
        new(0x006c, 0x0093),
        new(0x0073, 0x0093),
        new(0x0069, 0x0098),
        new(0x0070, 0x0096),
        new(0x0076, 0x0099),
        new(0x0073, 0x009b),
    ];

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

    /// <summary>The values <see cref="FragmentX"/> selects, in index order.</summary>
    private static readonly SignedSixteenSixteen[] FragmentXVelocities =
    [
        Split(-0xc000),
        Split(0x4000),
        Split(-0x8000),
        Split(-0xe000),
        Split(0x8000),
        Split(0x2000),
    ];

    /// <summary>
    /// $8B:AA02: 38 shell gravity records. Fragments zero/one also consume three
    /// adjacent instruction-byte pairs at $AA9A-$AAA5 before the signed ground
    /// test deletes them. Preserve this native overread as data, not executable code.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #977: all 38 high-word-first signed 16.16 records at
    /// $8B:AA02..AA99 match pinned NTSC J/U v1.0 ROM and bank_8B.asm. For
    /// frame f=0..13, group g=f/7 and remainder r=f%7 give whole=(g-2)
    /// and fraction=0 when r=0, otherwise $E000-$2000*r. For f=14..37,
    /// the signed value is (f-14)*$2000. The actor adds the fractional word
    /// before the whole word, with carry. Independent native-position walks
    /// first ground fragments 0/1 at frame 40, 2/3 at 37, 4 at 36, and 5
    /// at 35. Only 0/1 therefore read frames 38..40: $9DAD:$991B,
    /// $1B7D:$97AD, and $991A:$1A7D from adjacent initializer bytes.
    /// Those six overread words also match ROM; they are explicit exceptions
    /// to the bounded curve, not an extension of its formula. Other frame
    /// indices, including 41 and negative values, throw.
    /// </remarks>
    internal static readonly SignedSixteenSixteen[] FragmentOverreads =
    [
        SignedSixteenSixteen.FromParts(unchecked((short)0x9dad), 0x991b),
        SignedSixteenSixteen.FromParts(0x1b7d, 0x97ad),
        SignedSixteenSixteen.FromParts(unchecked((short)0x991a), 0x1a7d),
    ];

    /// <summary>The shell gravity records before the overread: the bounded curve's frame count.</summary>
    internal const int FragmentCurveFrames = 38;

    /// <summary>The gravity curves' step, an eighth of a pixel per frame in 16.16.</summary>
    internal const int GravityStep = 0x2000;

    /// <summary>One whole pixel in 16.16.</summary>
    internal const int OnePixel = 0x10000;

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

    /// <summary>The values <see cref="SlimeX"/> selects, in index order.</summary>
    private static readonly SignedSixteenSixteen[] SlimeXVelocities =
    [
        Split(-0x10000),
        Split(-0x8000),
        Split(0x10000),
        Split(0x8000),
        Split(-0x8000),
    ];

    /// <summary>One signed fixed-point step.</summary>
    internal static SignedSixteenSixteen Split(int value) => SignedSixteenSixteen.FromRaw(value);
}
