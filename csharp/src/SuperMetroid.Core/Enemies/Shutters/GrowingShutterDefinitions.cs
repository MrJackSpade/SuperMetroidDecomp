namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>Fixed growing-shutter dispatch ordering and 16.16 growth increments.</summary>
public static class GrowingShutterDefinitions
{
    /// <summary>$A2:EA4E, InitAI_ShutterGrowing.functionPointers: down timer/proximity, then up timer/proximity.</summary>
    internal static readonly GrowingShutterFunction[] InitialFunctions =
    [
        GrowingShutterFunction.WaitToGrowDownForTimer,
        GrowingShutterFunction.WaitToGrowDownForProximity,
        GrowingShutterFunction.WaitToGrowUpForTimer,
        GrowingShutterFunction.WaitToGrowUpForProximity,
    ];

    /// <summary>The authored growth speed records.</summary>
    internal const int SpeedCount = 24;

    /// <summary>How much faster each growth speed record is, in unsigned 16.16.</summary>
    internal const int SpeedStep = 4096;
}
