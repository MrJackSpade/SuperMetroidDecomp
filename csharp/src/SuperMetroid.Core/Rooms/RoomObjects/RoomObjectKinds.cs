namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The room-object kinds the game's room populations place; every other kind is rejected from a population.</summary>
internal static class RoomObjectKinds
{
    /// <summary>Returns <paramref name="kind"/> when one of the game's room populations places it, and rejects it otherwise.</summary>
    /// <exception cref="InvalidDataException">No room population of the game places <paramref name="kind"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="kind"/> names no room object.</exception>
    internal static RoomObjectKind RequirePlaced(RoomObjectKind kind) =>
        !Enum.IsDefined(kind) ? throw new InvalidOperationException($"Undefined room-object kind {kind}.")
        : RoomObjectKindDefinitions.Placed.Contains(kind)
            ? kind
            : throw new InvalidDataException($"No room population of the game places room-object kind {kind}.");
}
