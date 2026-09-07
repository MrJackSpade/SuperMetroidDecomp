namespace SuperMetroid.Core.Game;

/// <summary>Normal special-air BTS entries in bank-$94's inside-block dispatcher.</summary>
internal static class ConveyorBlockRomData
{
    /// <summary>$94:98EA, BTS $08: powered/grounded rightward conveyor.</summary>
    public const byte GroundedRight = 8;
    /// <summary>$94:9910, BTS $09: powered/grounded leftward conveyor.</summary>
    public const byte GroundedLeft = 9;
    /// <summary>$94:9936, BTS $0A: unconditional rightward conveyor.</summary>
    public const byte UnconditionalRight = 10;
    /// <summary>$94:9946, BTS $0B: unconditional leftward conveyor.</summary>
    public const byte UnconditionalLeft = 11;
    /// <summary>Whole external X displacement written by $94:98EA/$9936.</summary>
    public const ushort RightDisplacement = 2;
    /// <summary>Two's-complement external X displacement written by $94:9910/$9946.</summary>
    public const ushort LeftDisplacement = unchecked((ushort)-2);
}
