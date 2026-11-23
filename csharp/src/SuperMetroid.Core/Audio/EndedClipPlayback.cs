namespace SuperMetroid.Core.Audio;

/// <summary>A sound played past its logical end (#2560): it sounds on but no longer holds its library or its priority.</summary>
/// <param name="Sound">The playing sound.</param>
/// <param name="Start">Absolute frame of the sound's frame zero.</param>
internal sealed partial record EndedClipPlayback(SoundEffectClipEntry Sound, long Start) : ClipLibraryPlayback(Sound, Start);
