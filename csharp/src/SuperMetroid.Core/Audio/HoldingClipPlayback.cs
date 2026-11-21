namespace SuperMetroid.Core.Audio;

/// <summary>A sound that still holds its library (#2560): it has not reached its logical end.</summary>
/// <param name="Sound">The playing sound.</param>
/// <param name="Start">Absolute frame of the sound's frame zero.</param>
internal sealed record HoldingClipPlayback(SoundEffectClipEntry Sound, long Start) : ClipLibraryPlayback(Sound, Start);
