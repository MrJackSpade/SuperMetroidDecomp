namespace SuperMetroid.Core.Audio;

/// <summary>
/// The sound a library plays and from which frame (#1305, #2560): a closed choice of a sound that still holds the library
/// and one played past its logical end. A silent library has no playback.
/// </summary>
/// <param name="Sound">The playing sound.</param>
/// <param name="Start">Absolute frame of the sound's frame zero (the request check that accepted it).</param>
internal abstract record ClipLibraryPlayback(SoundEffectClipEntry Sound, long Start);
