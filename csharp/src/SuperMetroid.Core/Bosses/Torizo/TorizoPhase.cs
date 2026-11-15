namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The main phase a Bomb or Golden Torizo runs (#1413: the values are arbitrary).</summary>
public enum TorizoPhase
{
    /// <summary>Dormant: nothing runs.</summary>
    Idle,

    /// <summary>
    /// Settles under gravity while it wakes, turns or sits, dropping drool once its health is low; both statues fall
    /// alike (#1931).
    /// </summary>
    Falling,

    /// <summary>Bomb Torizo stays a solid statue until its Chozo statue's hand crumbles, then wakes.</summary>
    WaitForHandTrigger,

    /// <summary>
    /// Bomb Torizo fights: its gut and then its face blow up as its health falls past their thresholds, and otherwise
    /// its body moves by its motion.
    /// </summary>
    Active,

    /// <summary>Golden Torizo stays seated until Samus comes below and to the right of it, then wakes.</summary>
    GoldenWaitForSamus,

    /// <summary>Golden Torizo fights: its body moves by its motion, dropping drool once its health is low (#1931).</summary>
    GoldenActive,
}
