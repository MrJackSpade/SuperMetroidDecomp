namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's head and attack callback, held in its variable F. Members start at one, as for <see cref="BotwoonEnemyFunction"/>.</summary>
public enum BotwoonHeadFunction
{
    /// <summary><c>$B3:9DC0 Function_Botwoon_Head_MovingAround</c>: derives orientation from delayed head positions and selects hidden/tangible drawing according to hole state.</summary>
    AnimateFromMovement = 1,
    /// <summary><c>$B3:9E7D Function_Botwoon_Head_Spitting_SetAngleAndShow</c>: aims at Samus, installs the spit list, and immediately selects the five-shot animation wait or three-shot volley.</summary>
    AimAtSamus,
    /// <summary><c>$B3:9EE0 Function_Botwoon_Head_Spitting_Spawn5SpitProjectiles</c>: waits for the head bytecode's spit-frame flag, then emits five projectiles spaced sixteen angle units apart.</summary>
    WaitForSpitFrame,
    /// <summary><c>$B3:9F34 Function_Botwoon_Head_Spitting_Spawn3SpitProjectiles</c>: emits three aimed projectiles immediately and selects the attack cooldown.</summary>
    SpawnImmediateVolley,
    /// <summary><c>$B3:9F7A Function_Botwoon_Head_Spitting_Cooldown</c>: decrements the attack timer through zero to signed underflow, pins it to zero, and restores movement-based animation.</summary>
    AttackCooldown,
}
