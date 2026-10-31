using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Fixed egg-shell and slime trajectories, independent of cinematic artwork.</summary>
internal static class IntroEggMotionDefinitions
{

    /// <summary>The values <see cref="IntroEggMotion.FragmentInitialPosition"/> selects, in index order.</summary>
    internal static readonly Position16<ScreenPixels>[] FragmentInitialPositions =
    [
        new(108, 147),
        new(115, 147),
        new(105, 152),
        new(112, 150),
        new(118, 153),
        new(115, 155),
    ];

    /// <summary>The values <see cref="IntroEggMotion.FragmentX"/> selects, in index order.</summary>
    internal static readonly SignedSixteenSixteen[] FragmentXVelocities =
    [
        SignedSixteenSixteen.FromSixteenthsOfPixel(-12),
        SignedSixteenSixteen.FromSixteenthsOfPixel(4),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-8),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-14),
        SignedSixteenSixteen.FromSixteenthsOfPixel(8),
        SignedSixteenSixteen.FromSixteenthsOfPixel(2),
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
    [SuppressMessage("Design", "SME6447", Justification =
        "The unsigned fractional words of the overread records, instruction bytes read as a fraction of a pixel; no " +
        "natural-unit factory expresses them, and they are not cartridge addresses.")]
    internal static readonly SignedSixteenSixteen[] FragmentOverreads =
    [
        SignedSixteenSixteen.FromParts(unchecked((short)40365), 39195),
        SignedSixteenSixteen.FromParts(7037, 38829),
        SignedSixteenSixteen.FromParts(unchecked((short)39194), 6781),
    ];

    /// <summary>The shell gravity records before the overread: the bounded curve's frame count.</summary>
    internal const int FragmentCurveFrames = 38;

    /// <summary>The gravity curves' step, an eighth of a pixel per frame in 16.16.</summary>
    internal const int GravityStep = 8192;

    /// <summary>One whole pixel in 16.16.</summary>
    internal const int OnePixel = 65536;

    /// <summary>The values <see cref="IntroEggMotion.SlimeX"/> selects, in index order.</summary>
    internal static readonly SignedSixteenSixteen[] SlimeXVelocities =
    [
        SignedSixteenSixteen.FromSixteenthsOfPixel(-16),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-8),
        SignedSixteenSixteen.FromSixteenthsOfPixel(16),
        SignedSixteenSixteen.FromSixteenthsOfPixel(8),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-8),
    ];

    /// <summary>One signed fixed-point step.</summary>
    internal static SignedSixteenSixteen Split(int value) => SignedSixteenSixteen.FromRaw(value);
}
