namespace SuperMetroid.Core.Game;

/// <summary>Fixed NTSC launch magnitudes for the three separately dispatched Rio families.</summary>
public static class RioLaunchDefinitions
{
    /// <summary>$A2:BBBB, Rio initial downward velocity, in 8.8 pixels/frame.</summary>
    public const ushort RioYVelocity = 0x580;
    /// <summary>$A2:BBBF, Rio initial horizontal magnitude; caller supplies direction.</summary>
    public const ushort RioXVelocity = 0x180;
    /// <summary>$A2:C1C5, Norfair Rio horizontal magnitude.</summary>
    public const ushort NorfairXVelocity = 0x100;
    /// <summary>$A2:C6CA, lower Norfair Rio initial downward velocity.</summary>
    public const ushort LowerNorfairYVelocity = 0x700;
    /// <summary>$A2:C6CE, lower Norfair Rio horizontal magnitude.</summary>
    public const ushort LowerNorfairXVelocity = 0x100;

    /// <summary>$A2:C1C1: RNG bit two selects one of two adjacent downward velocities.</summary>
    public static ushort NorfairYVelocity(ushort random) => (random & 4) == 0 ? (ushort)0x700 : (ushort)0x5c0;
}
