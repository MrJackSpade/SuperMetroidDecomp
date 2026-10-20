namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// Stoke's main-AI routine (WRAM <c>$0FB2,x</c>). The cartridge has separate moving-left ($A2:8A43) and moving-right
/// ($A2:8A5C) routines; which one runs always matches the direction word, so the direction is read from
/// <see cref="StokeEnemyState.Direction"/> alone.
/// </summary>
public enum StokeAiFunction
{
    /// <summary>$A2:8A75, RTS_A28A75: no-op main entry while the attack animation owns projectile emission and eventual walk resumption.</summary>
    IdleDuringAttack,

    /// <summary>
    /// $A2:8A43 or $A2:8A5C: walking with the 16.16 speed pair of the direction word's side, terrain-turn checks, and the
    /// per-update random attack choice toward that side.
    /// </summary>
    Moving,
}
