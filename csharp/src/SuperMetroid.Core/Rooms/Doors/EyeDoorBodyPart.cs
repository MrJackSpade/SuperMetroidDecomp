namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The two parts of an eye door's body below its eye (#2481), each a room object of its own that pulses while Samus is
/// near and leaves once its door has been opened for good. Its values are arbitrary.
/// </summary>
internal enum EyeDoorBodyPart
{
    /// <summary>The door's middle, the main body, which closes behind Samus.</summary>
    Middle,

    /// <summary>The door's bottom, which no door closes.</summary>
    Bottom,
}
