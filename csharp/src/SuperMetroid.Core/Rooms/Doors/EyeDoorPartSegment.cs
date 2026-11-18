namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One run of an eye door's body's or bottom's poses (<see cref="EyeDoorSequenceDefinitions.PartSegments"/>, #2480, #2481),
/// shared by both parts: the part and its door's facing choose what each pose draws.
/// </summary>
internal enum EyeDoorPartSegment
{
    /// <summary>The still body or bottom: its resting pose for eight updates before it looks for Samus again.</summary>
    Still,

    /// <summary>The left-facing body or bottom pulsing while Samus is near: its poses there and back, eight updates each.</summary>
    LeftPulse,

    /// <summary>The right-facing body or bottom pulsing while Samus is near: its poses there and back, six updates each.</summary>
    RightPulse,
}
