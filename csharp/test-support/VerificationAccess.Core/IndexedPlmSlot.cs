using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core;

/// <summary>A PLM slot and its place in the table's processing order.</summary>
/// <param name="Slot">The slot.</param>
/// <param name="Index">The slot's processing position, zero first.</param>
internal readonly record struct IndexedPlmSlot(PlmSlot Slot, int Index);
