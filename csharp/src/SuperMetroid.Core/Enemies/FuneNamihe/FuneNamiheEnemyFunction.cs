namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// Cartridge function pointers stored in Fune/Namihe variable B (<c>$0FAA,x</c>).
/// The two one-byte RTS targets are retained separately because they are observable native
/// state: Fune and Namihe deliberately park on different addresses while mouth animation
/// bytecode owns the active portion of their attack cycle.
/// </summary>
public enum FuneNamiheEnemyFunction : ushort
{
    /// <summary>Function_Fune_WaitForTimer, $A8:9737: increments the wrapping cooldown before a signed comparison with parameter one's high-byte target; on expiry it clears the counter, selects active mouth bytecode, and parks at $978E.</summary>
    FuneWaitForCooldown = 0x9737,
    /// <summary>Function_Namihe_WaitForSamusToGetNear, $A8:975C: selects active mouth bytecode only when Samus's modular vertical distance is strictly below the whole-pixel parameter-two high-byte threshold, then parks at $978F without a cooldown.</summary>
    NamiheWaitForSamus = 0x975c,
    /// <summary>RTS_A8978E, $A8:978E: Fune's inert main-AI target while animation instructions own mouth motion, projectile/sound production, and the finish callback that restores its cooldown wait.</summary>
    FuneActivityNoOp = 0x978e,
    /// <summary>RTS_A8978F, $A8:978F: Namihe's separately observable inert main-AI target while animation bytecode owns the attack and eventually restores its Samus-proximity wait.</summary>
    NamiheActivityNoOp = 0x978f,
}
