namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's head and attack behaviour: what his head does each update. Each member names that behaviour.</summary>
public enum BotwoonHeadAction
{
    /// <summary>Derives orientation from delayed head positions and selects hidden/tangible drawing according to hole state.</summary>
    AnimateFromMovement,
    /// <summary>Aims at Samus, starts the spit animation, and immediately selects the aimed volley's animation wait or the immediate volley.</summary>
    AimAtSamus,
    /// <summary>Waits until his spitting animation reaches its spit, which turns the head to <see cref="SpitAimedVolley"/>.</summary>
    WaitForSpitFrame,
    /// <summary>His spitting animation reached its spit: spits the aimed volley (<see cref="BotwoonFightDefinitions.AimedVolley"/>) and cools down.</summary>
    SpitAimedVolley,
    /// <summary>Spits the immediate volley (<see cref="BotwoonFightDefinitions.ImmediateVolley"/>) at once, aimed at Samus, and cools down.</summary>
    SpawnImmediateVolley,
    /// <summary>Steps the spit cooldown and, once it runs out, returns the head to animating from his movement.</summary>
    AttackCooldown,
}
