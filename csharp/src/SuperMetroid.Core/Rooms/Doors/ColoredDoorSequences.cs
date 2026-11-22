using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The animations one colour and facing of coloured cap plays (#1554), besides the closing every cap shares.</summary>
/// <param name="Waiting">The closed image the cap shows as it starts to wait for its shot, before it rests.</param>
/// <param name="Flash">The flashes a hit short of opening the cap plays, before it rests again.</param>
/// <param name="Opening">The cap opening, after which it leaves the room.</param>
internal sealed partial record ColoredDoorSequences(
    Animation<DoorCapSegment> Waiting,
    Animation<DoorCapSegment> Flash,
    Animation<DoorCapSegment> Opening);
