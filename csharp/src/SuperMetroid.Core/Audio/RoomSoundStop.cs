namespace SuperMetroid.Core.Audio;

/// <summary>
/// Every stop of the room's library (<see cref="SoundEffectLibrary.Room"/>, #2580): a request that plays nothing and cuts
/// off the sound the library is playing. Each plays one note at zero volume, so it is a stop,
/// not a sound. It waits in the library's queue and holds the library until its end, as a sound does.
/// </summary>
/// <remarks>
/// Members are identities. Their values are arbitrary and nothing reads them; the import pipeline alone knows which
/// installed request each member is.
/// </remarks>
public enum RoomSoundStop
{
    /// <summary>
    /// Stops whichever sound the library plays, the low-health beep included, and ends low-health mode; it interrupts any
    /// sound of the library.
    /// </summary>
    Any,
    /// <summary>
    /// Stops the library's looping Speed Booster or elevator sound (#1842): Samus's Speed Booster ending, or an elevator
    /// arriving.
    /// </summary>
    SpeedBoosterOrElevator,
}
