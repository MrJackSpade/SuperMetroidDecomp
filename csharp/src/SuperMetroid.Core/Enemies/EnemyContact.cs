namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How an enemy meets Samus and her shots: whether her touch reaches it, whether it is solid to her, and whether it stops
/// a Plasma Beam shot. Its population entry starts it and its behaviour toggles it; each is an independent fact.
/// </summary>
[Flags]
public enum EnemyContact
{
    /// <summary>The enemy touches Samus, is not solid to her and lets a Plasma Beam shot pass through after a hit.</summary>
    None = 0,

    /// <summary>Excludes the enemy from the ordinary touch-collision pass against Samus.</summary>
    IgnoresSamus = 1 << 0,

    /// <summary>
    /// Makes the enemy take part in the solid-enemy collision pass even while unfrozen.
    /// Frozen enemies enter that pass independently of this flag.
    /// </summary>
    SolidToSamus = 1 << 1,

    /// <summary>Prevents Plasma Beam projectiles from passing through the enemy after a hit.</summary>
    BlocksPlasmaBeam = 1 << 2,
}
