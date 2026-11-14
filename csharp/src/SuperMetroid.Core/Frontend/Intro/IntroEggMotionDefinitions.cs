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
    /// The shell gravity curve's frame count: 38 records. Fragments zero and one fall for three frames past it, whose
    /// steps the installed <see cref="ShellFragmentFallPastCurve"/> holds (#1500).
    /// </summary>
    /// <remarks>
    /// Issues #625 and #977: all 38 signed 1/65536-pixel records match
    /// the authored data. For
    /// frame f=0..13, group g=f/7 and remainder r=f%7 give whole=(g-2)
    /// and fraction=0 when r=0, otherwise 57344-8192*r. For f=14..37,
    /// the signed value is (f-14)*8192. The actor adds the subpixels
    /// before the whole pixels, the fraction's overflow passing on. Independent position walks
    /// first ground fragments 0/1 at frame 40, 2/3 at 37, 4 at 36, and 5
    /// at 35. Only 0/1 therefore read frames 38..40, past the records.
    /// </remarks>
    internal const int FragmentCurveFrames = 38;

    /// <summary>The screen row a falling shell fragment is deleted at once its position reaches it.</summary>
    internal const int FragmentFloorY = 168;

    /// <summary>The gravity curves' step: an eighth of a pixel per frame.</summary>
    internal static SignedSixteenSixteen GravityStep { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(2);

    /// <summary>The values <see cref="IntroEggMotion.SlimeX"/> selects, in index order.</summary>
    internal static readonly SignedSixteenSixteen[] SlimeXVelocities =
    [
        SignedSixteenSixteen.FromSixteenthsOfPixel(-16),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-8),
        SignedSixteenSixteen.FromSixteenthsOfPixel(16),
        SignedSixteenSixteen.FromSixteenthsOfPixel(8),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-8),
    ];
}
