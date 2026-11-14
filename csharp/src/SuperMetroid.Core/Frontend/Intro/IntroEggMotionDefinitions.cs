namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>How the hatching egg's shell pieces and the Baby's slime drops fly, independent of their pictures.</summary>
internal static class IntroEggMotionDefinitions
{
    /// <summary>Where each piece of the shell starts, which <see cref="IntroEggMotion.FragmentInitialPosition"/> selects.</summary>
    internal static readonly IReadOnlyDictionary<IntroEggShellFragment, Position16<ScreenPixels>> FragmentInitialPositions =
        new Dictionary<IntroEggShellFragment, Position16<ScreenPixels>>
        {
            [IntroEggShellFragment.UpperLeft] = new(108, 147),
            [IntroEggShellFragment.UpperRight] = new(115, 147),
            [IntroEggShellFragment.Left] = new(105, 152),
            [IntroEggShellFragment.Centre] = new(112, 150),
            [IntroEggShellFragment.Right] = new(118, 153),
            [IntroEggShellFragment.Lower] = new(115, 155),
        };

    /// <summary>Each piece of the shell's horizontal launch speed, which <see cref="IntroEggMotion.FragmentX"/> selects.</summary>
    internal static readonly IReadOnlyDictionary<IntroEggShellFragment, SignedSixteenSixteen> FragmentXVelocities =
        new Dictionary<IntroEggShellFragment, SignedSixteenSixteen>
        {
            [IntroEggShellFragment.UpperLeft] = SignedSixteenSixteen.FromSixteenthsOfPixel(-12),
            [IntroEggShellFragment.UpperRight] = SignedSixteenSixteen.FromSixteenthsOfPixel(4),
            [IntroEggShellFragment.Left] = SignedSixteenSixteen.FromSixteenthsOfPixel(-8),
            [IntroEggShellFragment.Centre] = SignedSixteenSixteen.FromSixteenthsOfPixel(-14),
            [IntroEggShellFragment.Right] = SignedSixteenSixteen.FromSixteenthsOfPixel(8),
            [IntroEggShellFragment.Lower] = SignedSixteenSixteen.FromSixteenthsOfPixel(2),
        };

    /// <summary>
    /// The updates <see cref="ShellFall"/> covers. The upper-left and upper-right pieces fly three updates past it before
    /// they land; the installed <see cref="ShellFragmentFallPastCurve"/> holds those steps (#1500).
    /// </summary>
    internal const int FragmentCurveFrames = 38;

    /// <summary>The screen row a falling shell piece is deleted at once its position reaches it.</summary>
    internal const int FragmentFloorY = 168;

    /// <summary>How every piece of the shell flies: two spells rising, then falling.</summary>
    internal static IntroEggFallCurve ShellFall { get; } = new(RisingSpells: 2, Updates: FragmentCurveFrames);

    /// <summary>How the steeper slime drops fly: three spells rising, then falling.</summary>
    internal static IntroEggFallCurve SteepSlimeFall { get; } = new(RisingSpells: 3, Updates: 69);

    /// <summary>How the shallower slime drops fly: two spells rising, then falling.</summary>
    internal static IntroEggFallCurve ShallowSlimeFall { get; } = new(RisingSpells: 2, Updates: 62);

    /// <summary>
    /// The four slime drops the hatched Baby sheds, in the order it sheds them: a steep drop a pixel an update to the left,
    /// a shallow one half a pixel to the left, a steep one a pixel to the right and a shallow one half a pixel to the right.
    /// </summary>
    internal static IReadOnlyList<IntroEggSlimeLaunch> SlimeLaunches { get; } =
    [
        new(SignedSixteenSixteen.FromSixteenthsOfPixel(-16), SteepSlimeFall),
        new(SignedSixteenSixteen.FromSixteenthsOfPixel(-8), ShallowSlimeFall),
        new(SignedSixteenSixteen.FromSixteenthsOfPixel(16), SteepSlimeFall),
        new(SignedSixteenSixteen.FromSixteenthsOfPixel(8), ShallowSlimeFall),
    ];
}
