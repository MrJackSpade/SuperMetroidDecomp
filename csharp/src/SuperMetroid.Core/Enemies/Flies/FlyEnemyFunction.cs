namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>
/// The routine a Mellow-family enemy's main AI runs each frame.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum FlyEnemyFunction : ushort
{
    /// <summary>Native <c>Function_Flies_IdleMovement_ClockwiseCircle</c> at <c>$A2:B14E</c>; circles with increasing table offsets, switching direction on wrap or attacking when cooldown is zero and Samus is within 112 horizontal pixels.</summary>
    ClockwiseCircle = 1,
    /// <summary>Native <c>Function_Flies_IdleMovement_AntiClockwiseCircle</c> at <c>$A2:B17C</c>; circles with decreasing table offsets and the same proximity/cooldown admission as the clockwise state.</summary>
    AntiClockwiseCircle,
    /// <summary>Native <c>Function_Flies_AttackSamus</c> at <c>$A2:B1AA</c>; advances the sampled attack velocity until crossing the saved Samus Y, then reverses vertical velocity and retreats.</summary>
    AttackSamus,
    /// <summary>Native <c>Function_Flies_Retreat</c> at <c>$A2:B1D2</c>; continues with reversed Y velocity until the accumulated attack duration counts down through signed underflow, then starts a 24-update idle cooldown.</summary>
    Retreat,
}
