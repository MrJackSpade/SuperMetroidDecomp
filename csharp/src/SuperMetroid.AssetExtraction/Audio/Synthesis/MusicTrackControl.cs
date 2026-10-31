namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Control words in a top-level music track stream. A word with a nonzero high byte is a
/// phrase pointer, zero ends the track, and every other low value is a repeat count;
/// <see cref="MusicTrackControls"/> owns the word that names each control.
/// </summary>
internal enum MusicTrackControl
{
    /// <summary>Process phrases without waiting for note timers.</summary>
    FastForwardOn,
    /// <summary>Return to ordinary timed processing.</summary>
    FastForwardOff,
}
