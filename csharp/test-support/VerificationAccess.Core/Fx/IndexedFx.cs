
namespace SuperMetroid.Core.Fx;

/// <summary>A compiled FX record, the header that holds it, and the door word its header's selection stores for it.</summary>
/// <param name="Fx">The header that holds the record.</param>
/// <param name="Record">The record.</param>
/// <param name="DoorPointer">The record's native door word: its entry door's pointer, or zero for a default or a Mother Brain record.</param>
internal readonly record struct IndexedFx(RoomFxSelection Fx, RoomEffectDefinition Record, ushort DoorPointer);
