namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// The routine Fune's and Namihe's main AI runs each frame. The two idle returns stay separate members
/// because Fune and Namihe park on different ones while their mouth animation owns the active part of the attack cycle.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum FuneNamiheEnemyFunction
{
    /// <summary>Function_Fune_WaitForTimer, $A8:9737: increments the wrapping cooldown before a signed comparison with parameter one's high-byte target; on expiry it clears the counter, selects active mouth bytecode, and parks at $978E.</summary>
    FuneWaitForCooldown = 1,
    /// <summary>Function_Namihe_WaitForSamusToGetNear, $A8:975C: selects active mouth bytecode only when Samus's modular vertical distance is strictly below the whole-pixel parameter-two high-byte threshold, then parks at $978F without a cooldown.</summary>
    NamiheWaitForSamus,
    /// <summary>RTS_A8978E, $A8:978E: Fune's inert main-AI target while animation instructions own mouth motion, projectile/sound production, and the finish callback that restores its cooldown wait.</summary>
    FuneActivityNoOp,
    /// <summary>RTS_A8978F, $A8:978F: Namihe's separately observable inert main-AI target while animation bytecode owns the attack and eventually restores its Samus-proximity wait.</summary>
    NamiheActivityNoOp,
}
