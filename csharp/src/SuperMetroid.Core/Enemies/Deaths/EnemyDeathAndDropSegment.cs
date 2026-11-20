namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The animation segments of the drops the generic enemy-death explosions become, and of the respawn pause (#641, #2554), in
/// authored order. Their steps are <see cref="EnemyDeathAndDropAnimations"/>.
/// </summary>
internal enum EnemyDeathAndDropSegment
{
    /// <summary>An unseen pause before the enemy it stands for respawns, after which it is removed.</summary>
    RespawnPause,
    /// <summary>A small energy drop glowing dark, ringed and bright, then scattering, repeating.</summary>
    SmallEnergy,
    /// <summary>A big energy drop glowing bright, ringed and dark, then scattering, repeating.</summary>
    BigEnergy,
    /// <summary>A missile drop flashing, repeating.</summary>
    Missiles,
    /// <summary>A super missile drop flashing, repeating.</summary>
    SuperMissiles,
    /// <summary>A power bomb drop fading from bright to dim and flashing, repeating.</summary>
    PowerBombs,
}
