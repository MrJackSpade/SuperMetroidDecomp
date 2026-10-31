namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Fixed NTSC launch magnitudes for the three separately dispatched Rio families.</summary>
public static class RioLaunchDefinitions
{
    /// <summary>$A2:BBBB, Rio initial downward velocity, in 8.8 pixels/frame.</summary>
    public const ushort RioYVelocity = 1408;
    /// <summary>$A2:BBBF, Rio initial horizontal magnitude; caller supplies direction.</summary>
    public const ushort RioXVelocity = 384;
    /// <summary>$A2:C1C5, Norfair Rio horizontal magnitude.</summary>
    public const ushort NorfairXVelocity = 256;
    /// <summary>$A2:C6CA, lower Norfair Rio initial downward velocity.</summary>
    public const ushort LowerNorfairYVelocity = 1792;
    /// <summary>$A2:C6CE, lower Norfair Rio horizontal magnitude.</summary>
    public const ushort LowerNorfairXVelocity = 256;

    /// <summary>$A2:C1C1: the faster of the two adjacent Norfair Rio downward velocities, chosen when RNG bit two is clear.</summary>
    internal const ushort NorfairFastYVelocity = 1792;
    /// <summary>$A2:C1C1: the slower Norfair Rio downward velocity, chosen when RNG bit two is set.</summary>
    internal const ushort NorfairSlowYVelocity = 1472;

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
