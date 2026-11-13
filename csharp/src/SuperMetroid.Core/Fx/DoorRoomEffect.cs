using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>The room effect a room effect selection shows when Samus enters through one door (#1546, #1740).</summary>
/// <param name="Door">The entry door that selects the effect.</param>
/// <param name="Effect">The effect the room shows when entered through <paramref name="Door"/>.</param>
public sealed record DoorRoomEffect(DoorDefinition Door, RoomEffectDefinition Effect);
