namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Whether an enemy is drawn, whether it still occupies its slot, and whether it returns after a kill. Its population
/// entry starts it and its behaviour toggles it; each is an independent fact.
/// </summary>
[Flags]
public enum EnemyPresence
{
    /// <summary>The enemy is drawn, occupies its slot and stays dead when killed.</summary>
    None = 0,

    /// <summary>Suppresses ordinary enemy drawing while leaving the enemy's AI and collision state resident.</summary>
    Hidden = 1 << 0,

    /// <summary>Marks the slot as deleted so the room's enemy passes no longer process it.</summary>
    Deleted = 1 << 1,

    /// <summary>
    /// Enemy death keeps the slot occupied by the respawn placeholder, and the death-projectile animation later rebuilds
    /// the enemy from its unchanged population entry. Rinka is the family that exercises this lifecycle.
    /// </summary>
    RespawnsIfKilled = 1 << 2,
}
