namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Fixed NTSC launch magnitudes for the three separately dispatched Rio families.</summary>
public static class RioLaunchDefinitions
{
    /// <summary>Rio initial downward velocity, in 8.8 pixels/frame.</summary>
    public const int RioYVelocity = 1408;
    /// <summary>Rio initial horizontal magnitude; caller supplies direction.</summary>
    public const int RioXVelocity = 384;
    /// <summary>Norfair Rio horizontal magnitude.</summary>
    public const int NorfairXVelocity = 256;
    /// <summary>Lower Norfair Rio initial downward velocity.</summary>
    public const int LowerNorfairYVelocity = 1792;
    /// <summary>Lower Norfair Rio horizontal magnitude.</summary>
    public const int LowerNorfairXVelocity = 256;

    /// <summary>The faster of the Norfair Rio's two downward launch velocities, chosen when <see cref="SlowLaunchBit"/> is clear.</summary>
    internal const int NorfairFastYVelocity = 1792;
    /// <summary>The slower Norfair Rio downward launch velocity, chosen when <see cref="SlowLaunchBit"/> is set.</summary>
    internal const int NorfairSlowYVelocity = 1472;

    /// <summary>The random bit, bit two, that sends a diving Norfair Rio down at its slower launch velocity.</summary>
    internal static readonly RandomSampleField SlowLaunchBit = new(2, 1);

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
