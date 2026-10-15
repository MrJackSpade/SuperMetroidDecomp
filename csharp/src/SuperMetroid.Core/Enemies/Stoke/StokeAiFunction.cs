namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>
/// Indirect main-AI pointers stored at Stoke WRAM <c>$0FB2,x</c>. The values are the actual
/// bank-$A2 routine offsets so a debugger watch can be compared directly with the ROM.
/// </summary>
public enum StokeAiFunction : ushort
{
    /// <summary>$A2:8A75, RTS_A28A75: no-op main entry while the attack animation owns projectile emission and eventual walk resumption.</summary>
    IdleDuringAttack = 0x8a75,
    /// <summary>$A2:8A43, Function_Stoke_MovingLeft: walking with the negative 16.16 speed pair, terrain-turn checks, and the per-update random left-attack choice.</summary>
    MovingLeft = 0x8a43,
    /// <summary>$A2:8A5C, Function_Stoke_MovingRight: walking with the positive 16.16 speed pair, terrain-turn checks, and the per-update random right-attack choice.</summary>
    MovingRight = 0x8a5c,
}
