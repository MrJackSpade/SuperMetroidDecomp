namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>
/// The routine Puyo's main AI runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum PuyoEnemyFunction : ushort
{
    /// <summary>$A2:9B65, <c>Function_Puyo_Grounded</c>: decrements the hop cooldown and selects an airborne hop when the countdown becomes negative.</summary>
    Grounded = 1,
    /// <summary>$A2:9B81, <c>Function_Puyo_Airborne</c>: dispatches the current hop or collision-recovery routine through the airborne function word.</summary>
    Airborne,
}
