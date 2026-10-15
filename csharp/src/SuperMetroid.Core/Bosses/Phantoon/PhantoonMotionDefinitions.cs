namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>NTSC figure-eight and swoop limits; definition data, not mutable enemy speed.</summary>
public static class PhantoonMotionDefinitions
{
    /// <summary>$A7:D31A, MovePhantoonInSwoopingPattern: NTSC signed X threshold stops subtraction at -8 pixels/frame. PAL uses $F671.</summary>
    public const ushort SwoopNegativeXThreshold = 0xf801;
    /// <summary>The swoop target word's returning bit: set once the target passes 256, after which it counts back down.</summary>
    public const ushort SwoopReturningBit = 0x8000;
    /// <summary>The swoop target word's X position, below its returning bit.</summary>
    public const ushort SwoopTargetMask = 0x7fff;
    /// <summary>The leftmost swoop X position, 64 pixels left of the room's origin ($A7:D2D1).</summary>
    public const ushort SwoopMinimumX = 0xffc0;
    /// <summary>The rightmost swoop X position ($A7:D2D1).</summary>
    public const ushort SwoopMaximumX = 448;
    /// <summary>$A7:D394, MovePhantoonInSwoopingPattern: NTSC signed Y threshold stops subtraction at -6 pixels/frame. PAL uses $F8D1.</summary>
    public const ushort SwoopNegativeYThreshold = 0xfa01;
    /// <summary>$A7:CD73/$CD81: Phantoon_Figure8_SubAcceleration_SlowStage and its reverse clone, fractional 16.16 acceleration.</summary>
    public const ushort SlowFraction = 0x600;
    /// <summary>$A7:CD75/$CD83: slow-stage whole acceleration, zero in both directions.</summary>
    public const ushort SlowWhole = 0;
    /// <summary>$A7:CD77/$CD85: Phantoon_Figure8_SubAcceleration_FastStages and its reverse clone.</summary>
    public const ushort FastFraction = 0x1000;
    /// <summary>$A7:CD79/$CD87: fast-stage whole acceleration, zero in both directions.</summary>
    public const ushort FastWhole = 0;
    /// <summary>$A7:CD7B: Phantoon_Figure8_SpeedCaps_Stage0Max.</summary>
    public const ushort ForwardSlowCap = 2;
    /// <summary>$A7:CD7D: Phantoon_Figure8_SpeedCaps_Stage1Max, NTSC value seven.</summary>
    public const ushort ForwardFastCap = 7;
    /// <summary>$A7:CD7F: Phantoon_Figure8_SpeedCaps_Stage2Min.</summary>
    public const ushort ForwardMinimum = 0;
    /// <summary>$A7:CD89: Phantoon_ReverseFigure8_SpeedCaps_Stage0Max, signed minus two.</summary>
    public const ushort ReverseSlowCap = 0xfffe;
    /// <summary>$A7:CD8B: Phantoon_ReverseFigure8_SpeedCaps_Stage1Max, NTSC signed minus seven.</summary>
    public const ushort ReverseFastCap = 0xfff9;
    /// <summary>$A7:CD8D: Phantoon_ReverseFigure8_SpeedCaps_Stage2Min.</summary>
    public const ushort ReverseMaximum = 0;
}
