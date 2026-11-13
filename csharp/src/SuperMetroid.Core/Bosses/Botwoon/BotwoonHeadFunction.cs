namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's head and attack callback: what his head does each update. Members start at one, as for <see cref="BotwoonEnemyFunction"/>.</summary>
public enum BotwoonHeadFunction
{
    /// <summary>Derives orientation from delayed head positions and selects hidden/tangible drawing according to hole state.</summary>
    AnimateFromMovement = 1,
    /// <summary>Aims at Samus, installs the spit list, and immediately selects the five-shot animation wait or three-shot volley.</summary>
    AimAtSamus,
    /// <summary>Waits for the head animation's spit-frame flag, then emits five projectiles spaced sixteen angle units apart.</summary>
    WaitForSpitFrame,
    /// <summary>Emits three aimed projectiles immediately and selects the attack cooldown.</summary>
    SpawnImmediateVolley,
    /// <summary>Decrements the attack timer through zero to signed underflow, pins it to zero, and restores movement-based animation.</summary>
    AttackCooldown,
}
