namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// Bank-$A2 dispatcher values used by enemy $D4FF. The names describe the four
/// initialization waits and two growth directions without discarding the literal ROM
/// addresses a debugger needs when comparing this port with enemy variable A in WRAM.
/// </summary>
public enum GrowingShutterFunction : ushort
{
    /// <summary><c>$A2:EABD Function_ShutterGrowing_Initial_Upwards_WaitForTimer</c>: checks parameter 1 before decrementing; zero starts upward growth.</summary>
    WaitToGrowUpForTimer = 0xeabd,
    /// <summary><c>$A2:EAD1 Func_ShutterGrowing_Initial_Upwards_WaitForSamusToGetNear</c>: starts upward growth when Samus's absolute horizontal distance is strictly below parameter 1.</summary>
    WaitToGrowUpForProximity = 0xead1,
    /// <summary><c>$A2:EAE7 Func_ShutterGrowing_Initial_Downwards_WaitForSamusToGetNear</c>: starts downward growth at the strict parameter-1 horizontal proximity threshold.</summary>
    WaitToGrowDownForProximity = 0xeae7,
    /// <summary><c>$A2:EAFD Function_ShutterGrowing_Initial_Downwards_WaitForTimer</c>: checks parameter 1 before decrementing; zero starts downward growth.</summary>
    WaitToGrowDownForTimer = 0xeafd,
    /// <summary><c>$A2:EB11 Function_ShutterGrowing_Growing_Downwards</c>: advances four growth sections with positive fixed-point speed, then remains at growth level 4.</summary>
    GrowDown = 0xeb11,
    /// <summary><c>$A2:EC13 Function_ShutterGrowing_Growing_Upwards</c>: subtracts fixed-point growth speed and carries a riding Samus by the whole-pixel upward delta.</summary>
    GrowUp = 0xec13,
}
