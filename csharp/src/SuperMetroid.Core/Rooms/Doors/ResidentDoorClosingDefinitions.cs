using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// How a resident eye door's eye closes behind Samus: it leaves the room on its first update. Every other door that
/// closes is a typed room object (<see cref="IClosingDoor"/>).
/// </summary>
public static class ResidentDoorClosingDefinitions
{
    /// <summary>The closing start of each resident closer that keeps one by identity: the eye doors' eyes, which delete on entry.</summary>
    internal static IReadOnlyDictionary<RoomObjectHeaderId, RoomObjectProgramStart> Closings { get; } = new Dictionary<RoomObjectHeaderId, RoomObjectProgramStart>
    {
        [RoomObjectHeaderId.EyeDoorFacingRight] = RoomObjectProgramStart.Delete,
        [RoomObjectHeaderId.EyeDoorFacingLeft] = RoomObjectProgramStart.Delete,
    };
}
