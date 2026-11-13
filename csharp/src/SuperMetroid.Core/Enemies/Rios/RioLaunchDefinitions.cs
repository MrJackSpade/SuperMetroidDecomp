namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Fixed NTSC launch magnitudes for the three separately dispatched Rio families.</summary>
public static class RioLaunchDefinitions
{
    /// <summary>Rio initial downward velocity, five and a half pixels per update.</summary>
    public static SignedEightEight RioYVelocity { get; } = SignedEightEight.FromParts(5, 128);
    /// <summary>Rio initial horizontal speed, one and a half pixels per update; caller supplies direction.</summary>
    public static SignedEightEight RioXVelocity { get; } = SignedEightEight.FromParts(1, 128);
    /// <summary>Norfair Rio horizontal speed, one pixel per update.</summary>
    public static SignedEightEight NorfairXVelocity { get; } = SignedEightEight.FromParts(1, 0);
    /// <summary>Lower Norfair Rio initial downward velocity, seven pixels per update.</summary>
    public static SignedEightEight LowerNorfairYVelocity { get; } = SignedEightEight.FromParts(7, 0);
    /// <summary>Lower Norfair Rio horizontal speed, one pixel per update.</summary>
    public static SignedEightEight LowerNorfairXVelocity { get; } = SignedEightEight.FromParts(1, 0);

    /// <summary>The faster of the Norfair Rio's two downward launch velocities, seven pixels per update, chosen when <see cref="SlowLaunchBit"/> is clear.</summary>
    internal static SignedEightEight NorfairFastYVelocity { get; } = SignedEightEight.FromParts(7, 0);
    /// <summary>The slower Norfair Rio downward launch velocity, five and three quarter pixels per update, chosen when <see cref="SlowLaunchBit"/> is set.</summary>
    internal static SignedEightEight NorfairSlowYVelocity { get; } = SignedEightEight.FromParts(5, 192);

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
