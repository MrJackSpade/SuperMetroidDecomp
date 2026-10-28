namespace SuperMetroid.Core.Enemies.Rios;

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

    /// <summary>$A2:C1C1: the faster of the two adjacent Norfair Rio downward velocities, chosen when RNG bit two is clear.</summary>
    internal const ushort NorfairFastYVelocity = 0x700;
    /// <summary>$A2:C1C1: the slower Norfair Rio downward velocity, chosen when RNG bit two is set.</summary>
    internal const ushort NorfairSlowYVelocity = 0x5c0;

    /// <summary>The two random bits ($0101) of which at least one must be set for a Norfair or Lower Norfair Rio to attack.</summary>
    internal const ushort AttackRandomBits = 0x0101;
}
