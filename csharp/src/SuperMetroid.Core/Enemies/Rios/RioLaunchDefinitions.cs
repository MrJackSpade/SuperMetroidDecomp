namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Fixed NTSC launch magnitudes for the three separately dispatched Rio families.</summary>
public static class RioLaunchDefinitions
{
    /// <summary>$A2:BBBB, Rio initial downward velocity, in 8.8 pixels/frame.</summary>
    public const int RioYVelocity = 0x580;
    /// <summary>$A2:BBBF, Rio initial horizontal magnitude; caller supplies direction.</summary>
    public const int RioXVelocity = 0x180;
    /// <summary>$A2:C1C5, Norfair Rio horizontal magnitude.</summary>
    public const int NorfairXVelocity = 0x100;
    /// <summary>$A2:C6CA, lower Norfair Rio initial downward velocity.</summary>
    public const int LowerNorfairYVelocity = 0x700;
    /// <summary>$A2:C6CE, lower Norfair Rio horizontal magnitude.</summary>
    public const int LowerNorfairXVelocity = 0x100;

    /// <summary>$A2:C1C1: the faster of the two adjacent Norfair Rio downward velocities, chosen when RNG bit two is clear.</summary>
    internal const int NorfairFastYVelocity = 0x700;
    /// <summary>$A2:C1C1: the slower Norfair Rio downward velocity, chosen when RNG bit two is set.</summary>
    internal const int NorfairSlowYVelocity = 0x5c0;

    /// <summary>
    /// One of the two random bits, bit zero, of which at least one must be set for a Norfair or Lower Norfair Rio to
    /// attack.
    /// </summary>
    internal static readonly RandomSampleField AttackChanceLowBit = new(0, 1);

    /// <summary>
    /// The other of the two random bits, bit eight, of which at least one must be set for a Norfair or Lower Norfair
    /// Rio to attack.
    /// </summary>
    internal static readonly RandomSampleField AttackChanceHighBit = new(8, 1);
}
