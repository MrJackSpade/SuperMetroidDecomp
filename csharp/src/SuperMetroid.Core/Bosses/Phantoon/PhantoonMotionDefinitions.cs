namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>NTSC figure-eight and swoop limits; definition data, not mutable enemy speed.</summary>
public static class PhantoonMotionDefinitions
{
    /// <summary>
    /// MovePhantoonInSwoopingPattern: the signed 8.8 horizontal swoop velocity, in 256ths of a pixel per
    /// update, below which the leftward pull stops (the NTSC value, about -8 pixels per update; PAL differs).
    /// </summary>
    public const int SwoopNegativeXThreshold = -2047;
    /// <summary>The swoop target's far X position: once the target reaches it, the target counts back toward zero.</summary>
    public const int SwoopTargetTurnX = 256;
    /// <summary>The leftmost swoop X position, 64 pixels left of the room's origin.</summary>
    public const int SwoopMinimumX = -64;
    /// <summary>The rightmost swoop X position.</summary>
    public const int SwoopMaximumX = 448;
    /// <summary>
    /// MovePhantoonInSwoopingPattern: the signed 8.8 vertical swoop velocity, in 256ths of a pixel per update,
    /// below which the upward pull stops (the NTSC value, about -6 pixels per update; PAL differs).
    /// </summary>
    public const int SwoopNegativeYThreshold = -1535;
    /// <summary>Phantoon_Figure8_SubAcceleration_SlowStage and its reverse clone, fractional 16.16 acceleration.</summary>
    public const int SlowFraction = 1536;
    /// <summary>Slow-stage whole acceleration, zero in both directions.</summary>
    public const int SlowWhole = 0;
    /// <summary>Phantoon_Figure8_SubAcceleration_FastStages and its reverse clone.</summary>
    public const int FastFraction = 4096;
    /// <summary>Fast-stage whole acceleration, zero in both directions.</summary>
    public const int FastWhole = 0;
    /// <summary>Phantoon_Figure8_SpeedCaps_Stage0Max.</summary>
    public const int ForwardSlowCap = 2;
    /// <summary>Phantoon_Figure8_SpeedCaps_Stage1Max, NTSC value seven.</summary>
    public const int ForwardFastCap = 7;
    /// <summary>Phantoon_Figure8_SpeedCaps_Stage2Min.</summary>
    public const int ForwardMinimum = 0;
    /// <summary>Phantoon_ReverseFigure8_SpeedCaps_Stage0Max, the slow stage's signed whole-speed cap.</summary>
    public const int ReverseSlowCap = -2;
    /// <summary>Phantoon_ReverseFigure8_SpeedCaps_Stage1Max, the fast stages' signed whole-speed cap (NTSC).</summary>
    public const int ReverseFastCap = -7;
    /// <summary>Phantoon_ReverseFigure8_SpeedCaps_Stage2Min.</summary>
    public const int ReverseMaximum = 0;
}
