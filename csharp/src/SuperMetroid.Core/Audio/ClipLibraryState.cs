namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound library's clip playback (#1305): the sound it plays, from which frame, whether it
/// still holds the library (before its logical end), the library's priority and low-health mode,
/// and which layers an upload has silenced until their next key-on.
/// </summary>
internal sealed class ClipLibraryState
{
    /// <summary>The playing sound, or null.</summary>
    internal SoundEffectClipEntry? Sound;

    /// <summary>Absolute frame of the sound's frame zero (the accepting poll).</summary>
    internal long Start;

    /// <summary>Whether the sound still holds the library: it has not reached its logical end.</summary>
    internal bool Active;

    /// <summary>The library priority: zero lets any request replace the sound, one admits only exempt requests.</summary>
    internal byte Priority;

    /// <summary>Library 3's low-health mode: two rejects every request except the clearing one.</summary>
    internal byte LowHealthMode;

    /// <summary>Per layer, the clip frame before which an upload has silenced it; zero when audible.</summary>
    internal long[] SilencedUntil = [];
}
