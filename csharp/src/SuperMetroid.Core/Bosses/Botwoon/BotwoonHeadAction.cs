namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's head and attack behaviour: what his head does each update. Each member names that behaviour.</summary>
public enum BotwoonHeadAction
{
    /// <summary>Derives orientation from delayed head positions and selects hidden/tangible drawing according to hole state.</summary>
    AnimateFromMovement,
    /// <summary>Aims at Samus, starts the spit animation, and immediately selects the five-shot animation wait or three-shot volley.</summary>
    AimAtSamus,
    /// <summary>Waits for the head animation's spit-frame flag, then emits five projectiles spaced sixteen angle units apart.</summary>
    WaitForSpitFrame,
    /// <summary>Emits three aimed projectiles immediately and selects the attack cooldown.</summary>
    SpawnImmediateVolley,
    /// <summary>Steps the spit cooldown and, once it runs out, returns the head to animating from his movement.</summary>
    AttackCooldown,
}
