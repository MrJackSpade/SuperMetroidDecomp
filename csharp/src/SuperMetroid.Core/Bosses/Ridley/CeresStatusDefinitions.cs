namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Named values of the native <c>ceres_status</c> word (<see cref="RidleyEncounter.CeresStatus"/>).</summary>
internal static class CeresStatusDefinitions
{
    /// <summary>
    /// $8000, published by the Ceres door's destruction routine at $A6:F7DC once the door has rumbled apart after
    /// Ridley's escape, so the room's Mode-7 controller begins its rotation.
    /// </summary>
    internal const ushort DoorEscaped = 0x8000;
}
