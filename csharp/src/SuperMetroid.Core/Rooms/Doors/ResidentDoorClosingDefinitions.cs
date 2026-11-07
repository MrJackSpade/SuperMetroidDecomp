using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Fixed secondary-list metadata the door-closing spawn selects
/// for resident grey/coloured/eye doors and the Mother Brain escape gate.
/// </summary>
/// <remarks>
/// These dispatcher identities select bounded compiled timer, sound, draw and branch
/// programs. Neither selecting nor executing a list reads an executable header.
/// </remarks>
public static class ResidentDoorClosingDefinitions
{
    /// <summary>
    /// The closing start of each resident closer that keeps one by identity: the closing script the door-closing spawn
    /// selects for the escape gate and the eye door bodies. Typed grey and coloured caps close through their own
    /// animation. The eye doors' closing script deletes a resident eye
    /// door on entry instead of animating a closing cap.
    /// </summary>
    internal static IReadOnlyDictionary<RoomObjectHeaderId, RoomObjectProgramStart> Closings { get; } = new Dictionary<RoomObjectHeaderId, RoomObjectProgramStart>
    {
        [RoomObjectHeaderId.MotherBrainEscapeRoomGate] = RoomObjectProgramStart.Typed(ScriptedRoomObjectProgram.MotherBrainEscapeGateClosing),
        [RoomObjectHeaderId.EyeDoorFacingRight] = RoomObjectProgramStart.Delete,
        [RoomObjectHeaderId.EyeDoorFacingLeft] = RoomObjectProgramStart.Delete,
    };
}
