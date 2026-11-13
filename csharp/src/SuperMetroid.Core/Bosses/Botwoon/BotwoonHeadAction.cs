namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's head and attack behaviour: what his head does each update. Members start at one, as for <see cref="BotwoonPhase"/>.</summary>
public enum BotwoonHeadAction
{
    /// <summary>Derives orientation from delayed head positions and selects hidden/tangible drawing according to hole state.</summary>
    AnimateFromMovement = 1,
    /// <summary>Aims at Samus, starts the spit animation, and immediately selects the five-shot animation wait or three-shot volley.</summary>
    AimAtSamus,
    /// <summary>Waits for the head animation's spit-frame flag, then emits five projectiles spaced sixteen angle units apart.</summary>
    WaitForSpitFrame,
    /// <summary>Emits three aimed projectiles immediately and selects the attack cooldown.</summary>
    SpawnImmediateVolley,
    /// <summary>Counts the attack timer down until it passes zero, pins it to zero, and restores movement-based animation.</summary>
    AttackCooldown,
}
