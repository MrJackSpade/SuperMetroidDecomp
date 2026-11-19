namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's head and attack behaviour: what his head does each update. Each member names that behaviour.</summary>
public enum BotwoonHeadAction
{
    /// <summary>Derives orientation from delayed head positions and selects hidden/tangible drawing according to hole state.</summary>
    AnimateFromMovement,
    /// <summary>Aims at Samus, starts the spit animation, and immediately selects the five-shot animation wait or three-shot volley.</summary>
    AimAtSamus,
    /// <summary>Waits until his spitting animation reaches its spit, which turns the head to <see cref="SpitAimedVolley"/>.</summary>
    WaitForSpitFrame,
    /// <summary>His spitting animation reached its spit: spits five, a sixteenth of a turn apart, and cools down.</summary>
    SpitAimedVolley,
    /// <summary>Spits three at once, aimed at Samus, and cools down.</summary>
    SpawnImmediateVolley,
    /// <summary>Steps the spit cooldown and, once it runs out, returns the head to animating from his movement.</summary>
    AttackCooldown,
}
