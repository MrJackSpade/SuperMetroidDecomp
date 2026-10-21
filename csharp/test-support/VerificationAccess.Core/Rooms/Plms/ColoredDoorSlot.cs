using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A PLM slot and its coloured door state, if it is one.</summary>
/// <param name="Slot">The slot.</param>
/// <param name="Door">The door.</param>
internal readonly record struct ColoredDoorSlot(PlmSlot Slot, ColoredDoorPlmState? Door);
