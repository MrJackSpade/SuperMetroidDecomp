namespace SuperMetroid.Core.Samus.Movement;

/// <summary>State values stored in Samus's remembered liquid-physics word.</summary>
public enum SamusLiquidMedium : ushort
{
    /// <summary>No liquid physics are active at Samus's movement sample point.</summary>
    Air = 0,
    /// <summary>Water physics apply, including gravity and acceleration changes unless Gravity Suit overrides them.</summary>
    Water = 1,
    /// <summary>Lava or acid physics apply; damage handling distinguishes the actual room-FX type separately.</summary>
    LavaOrAcid = 2,
}
