namespace SuperMetroid.Core.Audio;

/// <summary>
/// Supplies the samples of the clips a <see cref="ClipAudioLibrary"/> names. The host's content reader implements it
/// over the installed clips; the game asks for a clip only when it first plays it.
/// </summary>
public interface IAudioClipSource
{
    /// <summary>The interleaved left/right samples of <paramref name="clip"/> at the manifest's rate.</summary>
    /// <exception cref="InvalidDataException">The clip's content does not match what the manifest names.</exception>
    short[] ReadSamples(AudioClipFile clip);
}
