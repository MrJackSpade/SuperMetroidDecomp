namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>How a music track's top-level record stream behaves when played to completion (#1305).</summary>
internal enum MusicTrackShape
{
    /// <summary>An infinite repeat returns the sequencer to a previous state; the track has a loop.</summary>
    Loops,

    /// <summary>The stream reaches its $0000 terminator and returns the music port to zero.</summary>
    Ends,

    /// <summary>No phrase state repeated within the search bound; the track has no finite loop period found.</summary>
    NoPeriodFound,
}
