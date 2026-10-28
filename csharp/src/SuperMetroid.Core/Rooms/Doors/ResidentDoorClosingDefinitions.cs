using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Fixed secondary-list metadata selected by <c>$82:E8EB Spawn_Door_Closing_PLM</c>
/// for resident grey/coloured/eye doors and the Mother Brain escape gate.
/// </summary>
/// <remarks>
/// These dispatcher identities select bounded compiled timer, sound, draw and branch
/// programs. Neither selecting nor executing a list reads an executable cartridge header.
/// </remarks>
public static class ResidentDoorClosingDefinitions
{
    /// <summary>
    /// The closing start of each resident closer that keeps one by identity: the original bank-84 header+4 word consumed
    /// by bank-82 E91C for the escape gate and the eye door bodies. Typed grey and coloured caps close through their own
    /// animation.
    /// </summary>
    internal static IReadOnlyDictionary<PlmHeaderId, PlmProgramStart> Closings { get; } = new Dictionary<PlmHeaderId, PlmProgramStart>
    {
        [PlmHeaderId.MotherBrainEscapeRoomGate] = PlmProgramStart.Typed(ScriptedRoomPlmProgram.MotherBrainEscapeGateClosing),
        // $84:DB4C/DB5A carry $84:AAE3 as their secondary list: $82:E91C deletes a resident
        // eye door on entry instead of animating a closing cap.
        [PlmHeaderId.EyeDoorFacingRight] = PlmProgramStart.Delete,
        [PlmHeaderId.EyeDoorFacingLeft] = PlmProgramStart.Delete,
    };
}
