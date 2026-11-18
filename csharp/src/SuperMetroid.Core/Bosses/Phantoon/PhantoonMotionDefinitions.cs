namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's figure-eight and swoop limits; definition data, not mutable enemy speed.</summary>
public static class PhantoonMotionDefinitions
{
    /// <summary>The horizontal swoop velocity, about eight pixels an update leftward, below which the leftward pull stops.</summary>
    public static SignedEightEight SwoopNegativeXThreshold { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-2047);

    /// <summary>The horizontal swoop velocity, eight pixels an update, at which the rightward pull stops.</summary>
    public static SignedEightEight SwoopPositiveXLimit { get; } = SignedEightEight.FromPixels(8);

    /// <summary>The speed his figure-eight starts from along its path: one place an update.</summary>
    public static SignedSixteenSixteen FigureEightStartSpeed { get; } = SignedSixteenSixteen.FromParts(1, 0);

    /// <summary>The vertical swoop velocity, six pixels an update, at which the downward pull stops.</summary>
    public static SignedEightEight SwoopPositiveYLimit { get; } = SignedEightEight.FromPixels(6);

    /// <summary>The swoop's horizontal pull toward its target, in 256ths of a pixel an update, each update.</summary>
    public static SignedEightEight SwoopHorizontalPull { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32);

    /// <summary>The swoop's vertical pull toward its target, in 256ths of a pixel an update, each update.</summary>
    public static SignedEightEight SwoopVerticalPull { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);
    /// <summary>The swoop target's far X position: once the target reaches it, the target counts back toward zero.</summary>
    public const int SwoopTargetTurnX = 256;
    /// <summary>The leftmost swoop X position, 64 pixels left of the room's origin.</summary>
    public const int SwoopMinimumX = -64;
    /// <summary>The rightmost swoop X position.</summary>
    public const int SwoopMaximumX = 448;
    /// <summary>The vertical swoop velocity, about six pixels an update upward, below which the upward pull stops.</summary>
    public static SignedEightEight SwoopNegativeYThreshold { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1535);
    /// <summary>
    /// The figure-eight's speed change each update while it eases in, in path places per update, in either direction
    /// (#1936): six 256ths of a place.
    /// </summary>
    public static SignedSixteenSixteen SlowSpeedChange { get; } = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(6);
    /// <summary>
    /// The figure-eight's speed change each update while it surges and settles, in path places per update, in either
    /// direction (#1936): a sixteenth of a place.
    /// </summary>
    public static SignedSixteenSixteen FastSpeedChange { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(1);
    /// <summary>The whole speed, in path places per update, at which a forward figure-eight stops easing in and surges.</summary>
    public const int ForwardSlowCap = 2;
    /// <summary>The whole speed at which a forward figure-eight stops surging and settles: seven places per update.</summary>
    public const int ForwardFastCap = 7;
    /// <summary>The whole speed a forward figure-eight settles down to before it eases in again.</summary>
    public const int ForwardMinimum = 0;
    /// <summary>The whole speed at which a returning figure-eight stops easing in and surges: two places per update back.</summary>
    public const int ReverseSlowCap = -2;
    /// <summary>The whole speed at which a returning figure-eight stops surging and settles: seven places per update back.</summary>
    public const int ReverseFastCap = -7;
    /// <summary>The speed a forward figure-eight surges from once it has eased in: a place an update.</summary>
    public static SignedSixteenSixteen ForwardSurgeStart { get; } = SignedSixteenSixteen.FromParts(ForwardSlowCap - 1, 0);
    /// <summary>The speed a forward figure-eight eases in from again once it has settled: a place an update.</summary>
    public static SignedSixteenSixteen ForwardEaseStart { get; } = SignedSixteenSixteen.FromParts(ForwardMinimum + 1, 0);
    /// <summary>The speed a returning figure-eight surges from once it has eased in: at rest.</summary>
    public static SignedSixteenSixteen ReverseSurgeStart { get; } = SignedSixteenSixteen.FromParts(ReverseSlowCap + 2, 0);
    /// <summary>The speed a returning figure-eight settles from once it has surged: six places an update back.</summary>
    public static SignedSixteenSixteen ReverseSettleStart { get; } = SignedSixteenSixteen.FromParts(ReverseFastCap + 1, 0);
    /// <summary>The highest his swoop carries him.</summary>
    public const int SwoopMinimumY = 64;
    /// <summary>The lowest his swoop carries him.</summary>
    public const int SwoopMaximumY = 216;
    /// <summary>The whole speed a returning figure-eight settles back to before it eases in again: at rest.</summary>
    public const int ReverseMaximum = 0;
}
