namespace SuperMetroid.Core.Game;

/// <summary>Native definition tables for Crocomire's mouth volley.</summary>
public static class CrocomireProjectileRomData
{
    /// <summary>$86:9059, CrocomiresProjectile_Gradients, indexed by the body's volley counter.</summary>
    public const int Gradients = 0x869059;
    /// <summary>$86:909B/$909C and $90A7/$90A8 shift each signed component left twice.</summary>
    public const int VelocityMultiplier = 4;
}
