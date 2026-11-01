namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy AI-handler states. The game treats them as composable bits: hurt and frozen coexist, and
/// the enemy pass runs the handler of the lowest set bit (grapple, then hurt, then frozen, otherwise main AI).
/// </summary>
[Flags]
public enum EnemyAiStates
{
    /// <summary>No special handler: the enemy runs its main AI.</summary>
    None = 0,

    /// <summary>The grapple beam holds the enemy and its grapple AI runs.</summary>
    Grappled = 1,

    /// <summary>The enemy runs its hurt AI, set when a shot damages it; the hurt handler clears it when the flash ends.</summary>
    Hurt = 2,

    /// <summary>
    /// The enemy is frozen. The processing window admits frozen actors even outside the camera window, keeping frozen
    /// processing and collision-list membership active.
    /// </summary>
    Frozen = 4,
}
