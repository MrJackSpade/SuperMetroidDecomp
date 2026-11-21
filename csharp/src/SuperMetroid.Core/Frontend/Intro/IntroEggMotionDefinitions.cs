using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>How the hatching egg's shell pieces and the Baby's slime drops fly, independent of their pictures.</summary>
internal static class IntroEggMotionDefinitions
{
    /// <summary>
    /// Each piece of the shell: where it starts and its horizontal launch speed, which
    /// <see cref="IntroEggMotion.FragmentInitialPosition"/> and <see cref="IntroEggMotion.FragmentX"/> select.
    /// </summary>
    internal static readonly IReadOnlyDictionary<IntroEggShellFragment, IntroEggShellPiece> ShellPieces =
        new Dictionary<IntroEggShellFragment, IntroEggShellPiece>
        {
            [IntroEggShellFragment.UpperLeft] = new(new(108, 147), SignedSixteenSixteen.FromPixelFraction(-3, 4)),
            [IntroEggShellFragment.UpperRight] = new(new(115, 147), SignedSixteenSixteen.FromPixelFraction(1, 4)),
            [IntroEggShellFragment.Left] = new(new(105, 152), SignedSixteenSixteen.FromPixelFraction(-1, 2)),
            [IntroEggShellFragment.Centre] = new(new(112, 150), SignedSixteenSixteen.FromPixelFraction(-7, 8)),
            [IntroEggShellFragment.Right] = new(new(118, 153), SignedSixteenSixteen.FromPixelFraction(1, 2)),
            [IntroEggShellFragment.Lower] = new(new(115, 155), SignedSixteenSixteen.FromPixelFraction(1, 8)),
        };

    /// <summary>The definition of <paramref name="fragment"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="fragment"/> is not a piece of the shell.</exception>
    internal static IntroEggShellPiece ShellPiece(IntroEggShellFragment fragment) => Ensure.Listed(ShellPieces, fragment);

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
    /// The rise of the shell pieces and the shallower slime drops, each update's speed: two spells of seven updates, two
    /// pixels an update upward then one, each spell's later updates three quarters of a pixel slower than its first and
    /// then an eighth of a pixel faster each update.
    /// </summary>
    private static ImmutableArray<SignedSixteenSixteen> TwoSpellRise { get; } =
    [
        SignedSixteenSixteen.FromWholePixels(-2), SignedSixteenSixteen.FromPixelFraction(-5, 4), SignedSixteenSixteen.FromPixelFraction(-11, 8),
        SignedSixteenSixteen.FromPixelFraction(-3, 2), SignedSixteenSixteen.FromPixelFraction(-13, 8), SignedSixteenSixteen.FromPixelFraction(-7, 4),
        SignedSixteenSixteen.FromPixelFraction(-15, 8),
        SignedSixteenSixteen.FromWholePixels(-1), SignedSixteenSixteen.FromPixelFraction(-1, 4), SignedSixteenSixteen.FromPixelFraction(-3, 8),
        SignedSixteenSixteen.FromPixelFraction(-1, 2), SignedSixteenSixteen.FromPixelFraction(-5, 8), SignedSixteenSixteen.FromPixelFraction(-3, 4),
        SignedSixteenSixteen.FromPixelFraction(-7, 8),
    ];

    /// <summary>
    /// The rise of the steeper slime drops, each update's speed: a spell three pixels an update upward, shaped as
    /// <see cref="TwoSpellRise"/>'s are, before its two.
    /// </summary>
    private static ImmutableArray<SignedSixteenSixteen> ThreeSpellRise { get; } =
    [
        SignedSixteenSixteen.FromWholePixels(-3), SignedSixteenSixteen.FromPixelFraction(-9, 4), SignedSixteenSixteen.FromPixelFraction(-19, 8),
        SignedSixteenSixteen.FromPixelFraction(-5, 2), SignedSixteenSixteen.FromPixelFraction(-21, 8), SignedSixteenSixteen.FromPixelFraction(-11, 4),
        SignedSixteenSixteen.FromPixelFraction(-23, 8),
        .. TwoSpellRise,
    ];

    /// <summary>How every piece of the shell flies: two spells rising, then falling.</summary>
    internal static IntroEggFallCurve ShellFall { get; } = new(TwoSpellRise, Updates: FragmentCurveFrames);

    /// <summary>How the steeper slime drops fly: three spells rising, then falling.</summary>
    internal static IntroEggFallCurve SteepSlimeFall { get; } = new(ThreeSpellRise, Updates: 69);

    /// <summary>How the shallower slime drops fly: two spells rising, then falling.</summary>
    internal static IntroEggFallCurve ShallowSlimeFall { get; } = new(TwoSpellRise, Updates: 62);

    /// <summary>
    /// The slime drops the hatched Baby sheds, in the order it sheds them: a steep drop and a shallow one to the left, then
    /// a steep one and a shallow one to the right.
    /// </summary>
    internal static IReadOnlyList<IntroEggSlimeLaunch> SlimeLaunches { get; } =
    [
        new(SignedSixteenSixteen.FromWholePixels(-1), SteepSlimeFall),
        new(SignedSixteenSixteen.FromPixelFraction(-1, 2), ShallowSlimeFall),
        new(SignedSixteenSixteen.FromWholePixels(1), SteepSlimeFall),
        new(SignedSixteenSixteen.FromPixelFraction(1, 2), ShallowSlimeFall),
    ];


    /// <summary>How much faster a piece of the egg falls each update once it has stopped rising: an eighth of a pixel an update.</summary>
    internal static readonly SignedSixteenSixteen FallGravity = SignedSixteenSixteen.FromPixelFraction(1, 8);
}
