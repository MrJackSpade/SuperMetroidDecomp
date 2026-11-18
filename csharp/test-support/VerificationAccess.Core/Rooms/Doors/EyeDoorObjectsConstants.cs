using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Cartridge values and helpers of <see cref="EyeDoorObjects"/> that only verification reads.</summary>
internal static class EyeDoorObjectsConstants
{
    /// <summary>Recognizes the eye, middle, and bottom definitions for either door orientation.</summary>
    internal static bool IsEyeDoorKind(RoomObjectKind kind) =>
        RoomObjectKindVariants.EyeDoorEyes.ContainsKey(kind) || RoomObjectKindVariants.EyeDoorParts.ContainsKey(kind);
}
