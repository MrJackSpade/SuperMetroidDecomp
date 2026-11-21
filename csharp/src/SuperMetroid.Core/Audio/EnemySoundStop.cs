namespace SuperMetroid.Core.Audio;

/// <summary>
/// Every stop of the enemies' library (<see cref="SoundEffectLibrary.Enemies"/>, #2580): a request that plays nothing and
/// cuts off the sound the library is playing. Each plays one note at zero volume, so it is a stop,
/// not a sound. It waits in the library's queue and holds the library until its end, as a sound does.
/// </summary>
/// <remarks>
/// Members are identities. Their values are arbitrary and nothing reads them; the import pipeline alone knows which
/// installed request each member is.
/// </remarks>
public enum EnemySoundStop
{
    /// <summary>Stops whichever sound the library plays; it interrupts any sound of the library.</summary>
    Any,
    /// <summary>
    /// Stops the library's sound sixteen updates after Samus enters the Maridia elevatube by either of its doors, as the
    /// tube's start requests it. The request was taken for the elevatube's own sound, but it plays one zero-volume note,
    /// as the library's other stop does: the tube has no sound of its own.
    /// </summary>
    Elevatube,
}
