namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The routine a Skree's main AI runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum SkreeEnemyFunction
{
    /// <summary><c>$A3:C6D5</c>: waits until Samus is within 48 horizontal pixels.</summary>
    Idling = 1,

    /// <summary><c>$A3:C6F7</c>: waits for the wind-up instruction to authorize the dive.</summary>
    PreparingAttack,

    /// <summary><c>$A3:C716</c>: descends six pixels and steers one pixel toward Samus until floor collision.</summary>
    Diving,

    /// <summary><c>$A3:C77F</c>: sinks for the retained 21-update lifetime, emits debris, then deletes the enemy.</summary>
    Burrowing,
}
