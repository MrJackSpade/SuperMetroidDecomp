namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What an eye door's body or bottom is doing (#2482). Its values are arbitrary.</summary>
internal enum EyeDoorPartPhase
{
    /// <summary>Just placed: on its next update the part takes its first look at its door, and leaves if it was opened for good.</summary>
    Placed,

    /// <summary>Standing still, looking for Samus each time its still image ends.</summary>
    Still,

    /// <summary>Pulsing while Samus stays near.</summary>
    Pulsing,

    /// <summary>The body closes behind Samus as she arrives through its door, leaving the room on its next update.</summary>
    ClosingBehindSamus,
}
