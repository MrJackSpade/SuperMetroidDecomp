using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The count and slots of one room plm population definitions load result.</summary>
/// <param name="Count">The count.</param>
/// <param name="Slots">The slots.</param>
internal readonly record struct RoomPlmPopulationDefinitionsLoadResult(int Count, RoomPlmSlotSnapshot[] Slots);
