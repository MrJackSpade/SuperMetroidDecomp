namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Low-three-bit projectile classes consumed by `$A9:B58E`.</summary>
public enum MotherBrainProjectileType
{
    /// <summary>Low-three-bit class zero: an ordinary beam projectile for damage lookup.</summary>
    Beam = 0,
    /// <summary>Low-three-bit class one: an ordinary missile projectile for damage lookup.</summary>
    Missile = 1,
    /// <summary>Low-three-bit class two: a super missile projectile for damage lookup.</summary>
    SuperMissile = 2,
    /// <summary>Low-three-bit class three: the Power Bomb family.</summary>
    PowerBomb = 3,
    /// <summary>Low-three-bit class four: the projectile family nibble $4, whose producer is not translated.</summary>
    ClassFour = 4,
    /// <summary>Low-three-bit class five: the bomb family.</summary>
    Bomb = 5,
    /// <summary>Low-three-bit class six: the projectile family nibble $6, whose producer is not translated.</summary>
    ClassSix = 6,
    /// <summary>Low-three-bit class seven: the beam-explosion family.</summary>
    BeamExplosion = 7,
}
