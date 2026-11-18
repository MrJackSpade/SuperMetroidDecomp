namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One run of an eye door's pictures and actions (<see cref="EyeDoorSequenceDefinitions"/>, #2480), shared by both
/// facings: the door's facing and part choose what each picture draws.
/// </summary>
internal enum EyeDoorSegment
{
    /// <summary>The closed eye, for four updates before it looks for Samus again.</summary>
    Closed,

    /// <summary>The eye opening as Samus nears: half open for eight updates.</summary>
    Opening,

    /// <summary>
    /// The open eye watching Samus: it fires three shots, then blinks slowly, its eye closed for 96 updates. The game draws
    /// the closed eye for 48 updates and then again for 48, the second draw writing the same picture to the same blocks,
    /// which nothing else writes in between, so it is one picture of both lengths (#2480).
    /// </summary>
    Watching,

    /// <summary>The eye blinking while Samus stands right in front of it: closed for four updates.</summary>
    Blinking,

    /// <summary>The eye flinching from a hit short of the threshold: it smokes, recovers and sweats.</summary>
    Flinching,

    /// <summary>One flash of the destroyed eye: the cleared opening for three updates, then the blue cap for four.</summary>
    OpeningFlash,

    /// <summary>The still body or bottom: its resting pose for eight updates before it looks for Samus again.</summary>
    Still,

    /// <summary>The left-facing body or bottom pulsing while Samus is near: its poses there and back, eight updates each.</summary>
    LeftPulse,

    /// <summary>The right-facing body or bottom pulsing while Samus is near: its poses there and back, six updates each.</summary>
    RightPulse,
}
