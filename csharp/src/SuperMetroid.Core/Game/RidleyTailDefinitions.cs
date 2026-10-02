namespace SuperMetroid.Core.Game;

/// <summary>Live entries of the $A6:CB21 tail dispatcher and its pogo geometry.</summary>
internal static class RidleyTailDefinitions
{
    /// <summary>$A6:CB23 -> CBC0, neutral tail.</summary>
    internal const ushort Neutral = 1;
    /// <summary>$A6:CB25 -> CB33, immediate pogo setup.</summary>
    internal const ushort PogoSetup = 2;
    /// <summary>$A6:CB27 -> CB45, point tail down.</summary>
    internal const ushort PointDown = 3;
    /// <summary>$A6:CB29 -> CBC7, normal pogo tail.</summary>
    internal const ushort Pogo = 4;
    /// <summary>$A6:CB2B -> CBCE, descending stab setup.</summary>
    internal const ushort StabSetup = 5;
    /// <summary>$A6:CB2D -> CB4E, descending stab.</summary>
    internal const ushort Stab = 6;
    /// <summary>$A6:CB78/CB88, straight-down target angle.</summary>
    internal const ushort DownAngle = 0x4000;
    /// <summary>$A6:CD52/CDD8, pogo clockwise whip target.</summary>
    internal const ushort PogoWhipAngle = 0x3f00;
    /// <summary>$A6:CE08, descending stab extension request.</summary>
    internal const ushort StabDistance = 0x0a00;
    /// <summary>$A6:B76E, bounce extension request.</summary>
    internal const ushort BounceDistance = 0x0c00;
    /// <summary>$A6:CC12, SetRidleyTailAngleExtrema_minClockwiseAngle for left/front/right facing.</summary>
    internal static ushort MinimumClockwise(ushort facing) => facing == 2 ? (ushort)0x3fc0 : (ushort)0x3ff0;
    /// <summary>$A6:CC18, SetRidleyTailAngleExtrema_maxCounterClockwiseAngle for left/front/right facing.</summary>
    internal static ushort MaximumCounterClockwise(ushort facing) => facing == 2 ? (ushort)0x4010 : (ushort)0x4040;
    /// <summary>$A6:D37C, InitializeTailParts_distances.</summary>
    internal static ReadOnlySpan<ushort> RestDistances => [0x0200, 0x0800, 0x0800, 0x0800, 0x0800, 0x0800, 0x0500];
}
