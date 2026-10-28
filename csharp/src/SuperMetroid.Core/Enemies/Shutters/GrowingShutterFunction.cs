namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing shutter's routines: four initialization waits and two growth directions.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum GrowingShutterFunction
{
    /// <summary><c>$A2:EABD Function_ShutterGrowing_Initial_Upwards_WaitForTimer</c>: checks parameter 1 before decrementing; zero starts upward growth.</summary>
    WaitToGrowUpForTimer = 1,
    /// <summary><c>$A2:EAD1 Func_ShutterGrowing_Initial_Upwards_WaitForSamusToGetNear</c>: starts upward growth when Samus's absolute horizontal distance is strictly below parameter 1.</summary>
    WaitToGrowUpForProximity,
    /// <summary><c>$A2:EAE7 Func_ShutterGrowing_Initial_Downwards_WaitForSamusToGetNear</c>: starts downward growth at the strict parameter-1 horizontal proximity threshold.</summary>
    WaitToGrowDownForProximity,
    /// <summary><c>$A2:EAFD Function_ShutterGrowing_Initial_Downwards_WaitForTimer</c>: checks parameter 1 before decrementing; zero starts downward growth.</summary>
    WaitToGrowDownForTimer,
    /// <summary><c>$A2:EB11 Function_ShutterGrowing_Growing_Downwards</c>: advances four growth sections with positive fixed-point speed, then remains at growth level 4.</summary>
    GrowDown,
    /// <summary><c>$A2:EC13 Function_ShutterGrowing_Growing_Upwards</c>: subtracts fixed-point growth speed and carries a riding Samus by the whole-pixel upward delta.</summary>
    GrowUp,
}
