namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The sequences one colour and facing of coloured cap plays (#1554), besides the closing every cap shares.</summary>
/// <param name="Waiting">The closed image the cap shows as it starts to wait for its shot, before it rests.</param>
/// <param name="Flash">The flashes a hit short of opening the cap plays, before it rests again.</param>
/// <param name="Opening">The cap opening, after which it leaves the room.</param>
internal sealed record ColoredDoorSequences(
    IReadOnlyList<DoorCapStep> Waiting,
    IReadOnlyList<DoorCapStep> Flash,
    IReadOnlyList<DoorCapStep> Opening);
