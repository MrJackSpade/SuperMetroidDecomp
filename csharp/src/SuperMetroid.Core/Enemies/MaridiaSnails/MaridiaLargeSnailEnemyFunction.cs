namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// The behaviour routine Oum's main AI runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum MaridiaLargeSnailEnemyFunction
{
    /// <summary>Function_Oum_Idle, $A2:CDE6: completes settling bounces, then selects an idle facing and enters rolling when Samus is strictly within 24 horizontal pixels.</summary>
    Idle = 1,
    /// <summary>Function_Oum_Rolling, $A2:CE2B: applies bounce/floor and horizontal collision movement; the preceding update's stop flag also suppresses the signed-underflow attack countdown.</summary>
    Rolling,
    /// <summary>Function_Oum_Attacking, $A2:CF40: waits for instruction $CCB3's animation-finished flag, consumes it, and installs the corresponding rolling list without moving during the wait.</summary>
    Attacking,
}
