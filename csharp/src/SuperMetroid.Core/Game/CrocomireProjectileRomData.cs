namespace SuperMetroid.Core.Game;

/// <summary>Native definition tables for Crocomire's mouth volley.</summary>
public static class CrocomireProjectileRomData
{
    /// <summary>$86:9059, CrocomiresProjectile_Gradients, indexed by the body's volley counter.</summary>
    public const int Gradients = 0x869059;
    /// <summary>$A0:B443, SineCosineTables_8bitSine_SignExtended: $86:9095 reads the X component here.</summary>
    public const int XVelocitySine = 0xa0b443;
    /// <summary>$A0:B3C3, SineCosineTables_NegativeCosine_SignExtended: $86:90A1 reads the Y component here.</summary>
    public const int YVelocityNegativeCosine = 0xa0b3c3;
    /// <summary>$86:909B/$909C and $90A7/$90A8 shift each signed component left twice.</summary>
    public const int VelocityMultiplier = 4;
}
