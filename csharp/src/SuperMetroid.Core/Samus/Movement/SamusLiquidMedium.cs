namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The medium whose physics Samus moves by, as her movement last found it. Its values are arbitrary.</summary>
public enum SamusLiquidMedium
{
    /// <summary>No liquid physics are active at Samus's movement sample point.</summary>
    Air,
    /// <summary>Water physics apply, including gravity and acceleration changes unless Gravity Suit overrides them.</summary>
    Water,
    /// <summary>Lava or acid physics apply; damage distinguishes the room's actual liquid separately.</summary>
    LavaOrAcid,
}
