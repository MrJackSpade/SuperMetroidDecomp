namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// The routine Kzan's top part runs each frame.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum KzanEnemyFunction
{
    /// <summary>Native <c>Function_Kzan_WaitingToFall</c> at <c>$A6:8BB4</c>; counts down the population-defined top wait before resetting fall acceleration.</summary>
    WaitingToFall = 1,
    /// <summary>Native <c>Function_Kzan_Falling</c> at <c>$A6:8BDC</c>; accelerates downward, carries a touching rider, and clamps to the lower target on landing.</summary>
    Falling,
    /// <summary>Native <c>Function_Kzan_WaitingToRise</c> at <c>$A6:8C4A</c>; waits at the lower target until the post-landing countdown expires.</summary>
    WaitingToRise,
    /// <summary>Native <c>Function_Kzan_Rising</c> at <c>$A6:8C5D</c>; returns upward at one-half pixel per NTSC actor update and resumes the top wait.</summary>
    Rising,
}
