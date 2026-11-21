using SuperMetroid.Core.Doors;

namespace SuperMetroid.Verification.Doors;

/// <summary>The doors the checks compare.</summary>
internal static class DoorChecks
{
    /// <summary>Whether a new door is locked.</summary>
    internal static bool NewDoorIsLocked => new Door().IsLocked;
}
