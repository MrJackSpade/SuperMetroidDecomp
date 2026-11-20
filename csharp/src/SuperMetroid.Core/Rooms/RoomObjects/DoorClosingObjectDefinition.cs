using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room object a door transition places to close the door behind Samus when nothing at the door closes: a blue cap
/// closing with its facing, or Mother Brain's escape gate closing.
/// </summary>
internal abstract record DoorClosingObjectDefinition
{
    /// <summary>Only the nested cases derive.</summary>
    private DoorClosingObjectDefinition()
    {
    }

    /// <summary>Mother Brain's escape gate closing, whatever the door's direction.</summary>
    internal static DoorClosingObjectDefinition EscapeGate { get; } = new EscapeGateClosing();

    /// <summary>A blue cap that closes facing <paramref name="Facing"/>.</summary>
    /// <param name="Facing">The cap's facing.</param>
    internal sealed record BlueCap(ColoredDoorOrientation Facing) : DoorClosingObjectDefinition;

    /// <summary>Mother Brain's escape gate closing.</summary>
    private sealed record EscapeGateClosing : DoorClosingObjectDefinition;
}
