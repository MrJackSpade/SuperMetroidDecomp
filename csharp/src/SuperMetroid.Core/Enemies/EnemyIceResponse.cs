namespace SuperMetroid.Core.Enemies;

/// <summary>How an Ice-bearing hit from one weapon treats an enemy (#1472).</summary>
public enum EnemyIceResponse
{
    /// <summary>The hit damages it, and a hit that would kill it freezes it instead.</summary>
    FreezesOnLethalHit,

    /// <summary>The hit damages it, and a lethal hit kills it.</summary>
    DiesOnLethalHit,

    /// <summary>The hit freezes it without damaging it.</summary>
    FreezesWithoutDamage,
}
