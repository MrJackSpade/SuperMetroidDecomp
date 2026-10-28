namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Control words in a top-level music track stream. A word with a nonzero high byte is a
/// phrase pointer, zero ends the track, and every other low value is a repeat count.
/// </summary>
internal enum MusicTrackControl
{
    /// <summary>$0080: process phrases without waiting for note timers.</summary>
    FastForwardOn = 0x80,
    /// <summary>$0081: return to ordinary timed processing.</summary>
    FastForwardOff = 0x81,
}
