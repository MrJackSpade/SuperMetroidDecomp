using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>How the hatching egg's shell pieces and the Baby's slime drops fly, independent of their pictures.</summary>
internal static class IntroEggMotionDefinitions
{
    /// <summary>Where each piece of the shell starts, which <see cref="IntroEggMotion.FragmentInitialPosition"/> selects.</summary>
    internal static readonly IReadOnlyDictionary<IntroEggShellFragment, Offset16<ScreenPixels>> FragmentInitialPositions =
        new Dictionary<IntroEggShellFragment, Offset16<ScreenPixels>>
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
            [IntroEggShellFragment.UpperLeft] = SignedSixteenSixteen.FromPixelFraction(-3, 4),
            [IntroEggShellFragment.UpperRight] = SignedSixteenSixteen.FromPixelFraction(1, 4),
            [IntroEggShellFragment.Left] = SignedSixteenSixteen.FromPixelFraction(-1, 2),
            [IntroEggShellFragment.Centre] = SignedSixteenSixteen.FromPixelFraction(-7, 8),
            [IntroEggShellFragment.Right] = SignedSixteenSixteen.FromPixelFraction(1, 2),
            [IntroEggShellFragment.Lower] = SignedSixteenSixteen.FromPixelFraction(1, 8),
        };

    /// <summary>
    /// The updates <see cref="ShellFall"/> covers. The upper-left and upper-right pieces fly three updates past it before
    /// they land; the installed <see cref="ShellFragmentFallPastCurve"/> holds those steps (#1500).
    /// </summary>
    internal const int FragmentCurveFrames = 38;

    /// <summary>
    /// The screen row at which the hatched Baby, rising out of the egg, sheds its slime drops and first cries: it reaches the
    /// row exactly once on its way up.
    /// </summary>
    internal const int SlimeShedRow = 145;

    /// <summary>The screen row a falling shell piece is deleted at once its position reaches it.</summary>
    internal const int FragmentFloorY = 168;

    /// <summary>
    /// The rise of the shell pieces and the shallower slime drops, in sixteenths of a pixel an update: two spells of seven
    /// updates, two pixels an update upward then one, each spell's later updates slowing from three quarters of a pixel
    /// less than its first.
    /// </summary>
    private static ImmutableArray<SignedSixteenSixteen> TwoSpellRise { get; } =
        [.. new[] { -32, -20, -22, -24, -26, -28, -30, -16, -4, -6, -8, -10, -12, -14 }.Select(SignedSixteenSixteen.FromSixteenthsOfPixel)];

    /// <summary>
    /// The rise of the steeper slime drops, in sixteenths of a pixel an update: a spell three pixels an update upward
    /// before <see cref="TwoSpellRise"/>'s two.
    /// </summary>
    private static ImmutableArray<SignedSixteenSixteen> ThreeSpellRise { get; } =
        [.. new[] { -48, -36, -38, -40, -42, -44, -46 }.Select(SignedSixteenSixteen.FromSixteenthsOfPixel), .. TwoSpellRise];

    /// <summary>How every piece of the shell flies: two spells rising, then falling.</summary>
    internal static IntroEggFallCurve ShellFall { get; } = new(TwoSpellRise, Updates: FragmentCurveFrames);

    /// <summary>How the steeper slime drops fly: three spells rising, then falling.</summary>
    internal static IntroEggFallCurve SteepSlimeFall { get; } = new(ThreeSpellRise, Updates: 69);

    /// <summary>How the shallower slime drops fly: two spells rising, then falling.</summary>
    internal static IntroEggFallCurve ShallowSlimeFall { get; } = new(TwoSpellRise, Updates: 62);

    /// <summary>
    /// The four slime drops the hatched Baby sheds, in the order it sheds them: a steep drop a pixel an update to the left,
    /// a shallow one half a pixel to the left, a steep one a pixel to the right and a shallow one half a pixel to the right.
    /// </summary>
    internal static IReadOnlyList<IntroEggSlimeLaunch> SlimeLaunches { get; } =
    [
        new(SignedSixteenSixteen.FromWholePixels(-1), SteepSlimeFall),
        new(SignedSixteenSixteen.FromPixelFraction(-1, 2), ShallowSlimeFall),
        new(SignedSixteenSixteen.FromWholePixels(1), SteepSlimeFall),
        new(SignedSixteenSixteen.FromPixelFraction(1, 2), ShallowSlimeFall),
    ];
}
