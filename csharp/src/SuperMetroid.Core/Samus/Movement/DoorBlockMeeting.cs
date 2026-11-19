namespace SuperMetroid.Core.Samus.Movement;

/// <summary>What a move's door blocks do to Samus beyond stopping her or letting her into the doorway (#2545).</summary>
public enum DoorBlockMeeting
{
    /// <summary>
    /// Her collision makes each door block's contact: she enters the door it leads through, or stands at or brushes the
    /// elevator's door (<see cref="Rooms.Doors.DoorContact"/>).
    /// </summary>
    Contacts,

    /// <summary>
    /// An elevator not departing carries her past its shaft's door blocks without contacting them. The destination's
    /// elevator arrives while gameplay already runs, so a stand at its door would send it straight back, and the shaft's
    /// door would start another transition.
    /// </summary>
    CarriedPastByElevator,
}
