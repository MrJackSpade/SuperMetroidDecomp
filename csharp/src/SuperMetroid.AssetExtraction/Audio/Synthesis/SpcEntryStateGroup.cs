namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The groups of SPC driver state that a track start and an upload leave in place, so a track's
/// output can depend on what played before it (#1305 Findings 7).
/// </summary>
internal enum SpcEntryStateGroup
{
    /// <summary>Echo enable, volumes, delay, feedback, write-disable and FIR, with their fades.</summary>
    Echo,

    /// <summary>The noise-enable voice mask and the global noise rate.</summary>
    Noise,

    /// <summary>Every music-voice field, including the ones a track start does not reset.</summary>
    Channels,
}
