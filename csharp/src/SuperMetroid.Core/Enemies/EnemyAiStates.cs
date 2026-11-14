namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy AI states. The game treats them as composable: hurt and frozen coexist, and the enemy pass runs
/// the AI of the first state set in the order grapple, hurt, frozen, otherwise the main AI.
/// </summary>
[Flags]
public enum EnemyAiStates
{
    /// <summary>No special state: the enemy runs its main AI.</summary>
    None = 0,

    /// <summary>The grapple beam holds the enemy and its grapple AI runs.</summary>
    Grappled = 1 << 0,

    /// <summary>The enemy runs its hurt AI, set when a shot damages it; the hurt AI clears it when the flash ends.</summary>
    Hurt = 1 << 1,

    /// <summary>
    /// The enemy is frozen. The processing window admits frozen actors even outside the camera window, keeping frozen
    /// processing and collision-list membership active.
    /// </summary>
    Frozen = 1 << 2,
}
