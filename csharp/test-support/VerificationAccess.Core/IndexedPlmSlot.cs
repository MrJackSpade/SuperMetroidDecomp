using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core;

/// <summary>A PLM slot and its index.</summary>
/// <param name="Slot">The slot.</param>
/// <param name="Index">The index.</param>
internal readonly record struct IndexedPlmSlot(PlmSlot Slot, int Index);
