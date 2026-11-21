namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Library three's request mode (#2560), which its low-health and cancellation commands set.</summary>
internal enum SoundLibraryMode
{
    /// <summary>Requests pass the library's priority rule.</summary>
    Normal,
    /// <summary>The low-health beep holds the library, which rejects every request except its clearing one.</summary>
    LowHealth,
}
