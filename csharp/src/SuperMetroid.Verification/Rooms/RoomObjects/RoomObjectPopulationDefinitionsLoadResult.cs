using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The count and slots of one room roomObject population definitions load result.</summary>
/// <param name="Count">The count.</param>
/// <param name="Slots">The slots.</param>
internal readonly record struct RoomObjectPopulationDefinitionsLoadResult(int Count, RoomObjectSlotSnapshot[] Slots);
