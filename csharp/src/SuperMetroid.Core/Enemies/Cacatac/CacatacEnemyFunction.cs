namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The routine Cacatac's main AI runs each frame. Initialization selects one of the first three by the
/// low byte of parameter one (0 moves left, 1 moves right, 2 stops while an attack animation owns the actor); other
/// selectors are outside the authored table.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum CacatacEnemyFunction
{
    /// <summary>$A2:9FBA, <c>Function_Cacatac_MovingLeft</c>: advances the split 16.16 left velocity, turns right below the minimum X bound, then evaluates the random attack gate.</summary>
    MovingLeft = 1,
    /// <summary>$A2:9FEC, <c>Function_Cacatac_MovingRight</c>: advances the split 16.16 right velocity, turns left at or beyond the maximum X bound, then evaluates the random attack gate.</summary>
    MovingRight,
    /// <summary>$A2:A01B, <c>RTS_A2A01B</c>: performs no patrol update while attack animation commands own the actor; also the initial target for population direction selector two.</summary>
    Stopped,
}
