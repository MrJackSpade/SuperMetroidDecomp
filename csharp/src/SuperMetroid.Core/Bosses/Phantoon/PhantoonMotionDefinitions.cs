namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>NTSC figure-eight and swoop limits; definition data, not mutable enemy speed.</summary>
public static class PhantoonMotionDefinitions
{
    /// <summary>
    /// The horizontal swoop velocity below which the leftward pull stops (the NTSC
    /// value, about -8 pixels per update; PAL differs).
    /// </summary>
    public static SignedEightEight SwoopNegativeXThreshold { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-2047);

    /// <summary>The horizontal swoop velocity, eight pixels an update, at which the rightward pull stops.</summary>
    public static SignedEightEight SwoopPositiveXLimit { get; } = SignedEightEight.FromPixels(8);

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
    /// <summary>
    /// The vertical swoop velocity below which the upward pull stops (the NTSC value,
    /// about -6 pixels per update; PAL differs).
    /// </summary>
    public static SignedEightEight SwoopNegativeYThreshold { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1535);
    /// <summary>The figure-eight's slow-stage sub-pixel acceleration, shared by its reverse, fractional 1/65536-pixel acceleration.</summary>
    public const int SlowFraction = 1536;
    /// <summary>Slow-stage whole acceleration, zero in both directions.</summary>
    public const int SlowWhole = 0;
    /// <summary>The figure-eight's fast-stage sub-pixel acceleration, shared by its reverse.</summary>
    public const int FastFraction = 4096;
    /// <summary>Fast-stage whole acceleration, zero in both directions.</summary>
    public const int FastWhole = 0;
    /// <summary>The figure-eight's stage-zero maximum speed.</summary>
    public const int ForwardSlowCap = 2;
    /// <summary>The figure-eight's stage-one maximum speed, NTSC value seven.</summary>
    public const int ForwardFastCap = 7;
    /// <summary>The figure-eight's stage-two minimum speed.</summary>
    public const int ForwardMinimum = 0;
    /// <summary>The reverse figure-eight's stage-zero maximum, the slow stage's signed whole-speed cap.</summary>
    public const int ReverseSlowCap = -2;
    /// <summary>The reverse figure-eight's stage-one maximum, the fast stages' signed whole-speed cap (NTSC).</summary>
    public const int ReverseFastCap = -7;
    /// <summary>The reverse figure-eight's stage-two minimum.</summary>
    public const int ReverseMaximum = 0;
}
