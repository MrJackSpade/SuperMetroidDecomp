namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// The routine a Skultera's main AI runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum SkulteraEnemyFunction : ushort
{
    /// <summary>$A3:9132, Function_Skultera_SwimmingLeft: applies signed 16.16 leftward displacement and whole-pixel sine-offset changes; either axis collision starts a right turn.</summary>
    SwimmingLeft = 1,
    /// <summary>$A3:91AB, Function_Skultera_SwimmingRight: mirrors leftward swimming with the rightward speed; unobstructed movement advances the low-byte angle, and either axis collision starts a left turn.</summary>
    SwimmingRight,
    /// <summary>$A3:9224, Function_Skultera_TurningRight: waits without swimming until animation sets the turn-finished flag, then clears it, negates the angle delta, and installs rightward swimming.</summary>
    TurningRight,
    /// <summary>$A3:9256, Function_Skultera_TurningLeft: waits for the animation's turn-finished flag, then clears it, negates the angle delta, and installs leftward swimming.</summary>
    TurningLeft,
}
