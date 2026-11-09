namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// A room object at a door that closes behind Samus when she arrives through that door (#1554): a coloured or grey cap,
/// an eye door's eye, or Mother Brain's escape gate. The door transition asks it to close after the room's objects are
/// placed and before any of them updates.
/// </summary>
internal interface IClosingDoor
{
    /// <summary>Closes the door behind Samus: the object's closing starts on its next update.</summary>
    void CloseBehindSamus();
}
