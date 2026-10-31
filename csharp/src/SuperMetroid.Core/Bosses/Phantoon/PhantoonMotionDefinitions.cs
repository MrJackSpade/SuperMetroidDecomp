namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>NTSC figure-eight and swoop limits; definition data, not mutable enemy speed.</summary>
public static class PhantoonMotionDefinitions
{
    /// <summary>
    /// $A7:D31A, MovePhantoonInSwoopingPattern: the signed 8.8 horizontal swoop velocity, in 256ths of a pixel per
    /// update, below which the leftward pull stops (the NTSC value, about -8 pixels per update; PAL differs).
    /// </summary>
    public const short SwoopNegativeXThreshold = -2047;
    /// <summary>The swoop target's far X position: once the target reaches it, the target counts back toward zero.</summary>
    public const int SwoopTargetTurnX = 256;
    /// <summary>The leftmost swoop X position, 64 pixels left of the room's origin ($A7:D2D1).</summary>
    public const short SwoopMinimumX = -64;
    /// <summary>The rightmost swoop X position ($A7:D2D1).</summary>
    public const ushort SwoopMaximumX = 448;
    /// <summary>
    /// $A7:D394, MovePhantoonInSwoopingPattern: the signed 8.8 vertical swoop velocity, in 256ths of a pixel per update,
    /// below which the upward pull stops (the NTSC value, about -6 pixels per update; PAL differs).
    /// </summary>
    public const short SwoopNegativeYThreshold = -1535;
    /// <summary>$A7:CD73/$CD81: Phantoon_Figure8_SubAcceleration_SlowStage and its reverse clone, fractional 16.16 acceleration.</summary>
    public const ushort SlowFraction = 1536;
    /// <summary>$A7:CD75/$CD83: slow-stage whole acceleration, zero in both directions.</summary>
    public const ushort SlowWhole = 0;
    /// <summary>$A7:CD77/$CD85: Phantoon_Figure8_SubAcceleration_FastStages and its reverse clone.</summary>
    public const ushort FastFraction = 4096;
    /// <summary>$A7:CD79/$CD87: fast-stage whole acceleration, zero in both directions.</summary>
    public const ushort FastWhole = 0;
    /// <summary>$A7:CD7B: Phantoon_Figure8_SpeedCaps_Stage0Max.</summary>
    public const ushort ForwardSlowCap = 2;
    /// <summary>$A7:CD7D: Phantoon_Figure8_SpeedCaps_Stage1Max, NTSC value seven.</summary>
    public const ushort ForwardFastCap = 7;
    /// <summary>$A7:CD7F: Phantoon_Figure8_SpeedCaps_Stage2Min.</summary>
    public const ushort ForwardMinimum = 0;
    /// <summary>$A7:CD89: Phantoon_ReverseFigure8_SpeedCaps_Stage0Max, the slow stage's signed whole-speed cap.</summary>
    public const short ReverseSlowCap = -2;
    /// <summary>$A7:CD8B: Phantoon_ReverseFigure8_SpeedCaps_Stage1Max, the fast stages' signed whole-speed cap (NTSC).</summary>
    public const short ReverseFastCap = -7;
    /// <summary>$A7:CD8D: Phantoon_ReverseFigure8_SpeedCaps_Stage2Min.</summary>
    public const ushort ReverseMaximum = 0;
}
