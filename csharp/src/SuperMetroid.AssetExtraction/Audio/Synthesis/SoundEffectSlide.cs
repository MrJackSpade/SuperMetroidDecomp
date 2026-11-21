namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// How far a sound-effect channel's pitch slide has gone (#2560): one choice where the driver keeps an enable mark and a
/// legato mark that only means something while the slide runs.
/// </summary>
internal enum SoundEffectSlide
{
    /// <summary>No slide runs; each note keys on and releases.</summary>
    Off,
    /// <summary>Each tick moves the note toward its target, still keying notes on and releasing them.</summary>
    Sliding,
    /// <summary>A legato slide has taken hold: notes continue without retriggering or releasing the voice.</summary>
    SlidingLegato,
}
