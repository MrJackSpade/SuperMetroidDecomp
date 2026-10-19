namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy AI-handler states ($0F8E,x). The cartridge treats them as composable bits: hurt and frozen coexist, and
/// EnemyMain's `$A0:9044` runs the handler of the lowest set bit (grapple, then hurt, then frozen, otherwise main AI).
/// </summary>
[Flags]
public enum EnemyAiStates : ushort
{
    /// <summary>No special handler: the enemy runs its main AI.</summary>
    None = 0,

    /// <summary>Bit $0001: the grapple beam holds the enemy and its grapple AI runs.</summary>
    Grappled = 1,

    /// <summary>Bit $0002: the enemy runs its hurt AI, set when a shot damages it; `$A0:9128` clears it when the flash ends.</summary>
    Hurt = 2,

    /// <summary>
    /// Bit $0004: the enemy is frozen. `$A0:8EB6` admits frozen actors even outside the camera window, keeping frozen
    /// processing and collision-list membership active.
    /// </summary>
    Frozen = 4,
}
