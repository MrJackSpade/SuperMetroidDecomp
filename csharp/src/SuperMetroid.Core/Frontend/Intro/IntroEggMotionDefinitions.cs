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
    /// 38 shell gravity records. Fragments zero/one also consume three
    /// pairs of the bytes that follow the records before the signed ground
    /// test deletes them. Preserve this overread as data, not executable code.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #977: all 38 high-word-first signed 16.16 records match
    /// the original data. For
    /// frame f=0..13, group g=f/7 and remainder r=f%7 give whole=(g-2)
    /// and fraction=0 when r=0, otherwise 57344-8192*r. For f=14..37,
    /// the signed value is (f-14)*8192. The actor adds the fractional word
    /// before the whole word, with carry. Independent position walks
    /// first ground fragments 0/1 at frame 40, 2/3 at 37, 4 at 36, and 5
    /// at 35. Only 0/1 therefore read frames 38..40, whose three whole:fraction
    /// pairs come from the bytes that follow the records.
    /// Those six overread words also match the original data; they are explicit exceptions
    /// to the bounded curve, not an extension of its formula. Other frame
    /// indices, including 41 and negative values, throw.
    /// </remarks>
    [SuppressMessage("Design", "SME6447", Justification =
        "The unsigned fractional words of the overread records, instruction bytes read as a fraction of a pixel; no " +
        "natural-unit factory expresses them, and they are not memory locations.")]
    internal static readonly SignedSixteenSixteen[] FragmentOverreads =
    [
        SignedSixteenSixteen.FromParts(WordWrap.Signed(40365), 39195),
        SignedSixteenSixteen.FromParts(7037, 38829),
        SignedSixteenSixteen.FromParts(WordWrap.Signed(39194), 6781),
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
