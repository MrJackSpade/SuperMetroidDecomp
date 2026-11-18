using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The save station an action came from (#2383, #2481): the station itself, which the save's answer and its completion
/// return to, the load station it saves at, and its block.
/// </summary>
/// <param name="Station">The save station the action came from.</param>
/// <param name="SavesAt">The load station the save station saves at.</param>
/// <param name="Block">The station's room block, rather than a side-access trigger's.</param>
internal readonly record struct SaveStationActivation(SaveStationObject Station, LoadStation SavesAt, Position16<RoomBlocks> Block);
