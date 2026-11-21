namespace SuperMetroid.Core.Audio;

/// <summary>A sound effect that ends (#2560): it holds its library until its logical end and plays its clip once.</summary>
/// <param name="ActiveFrames">Frames until the library's logical end, when its priority resets.</param>
/// <param name="Clip">The rendered sound, or null for a silent effect, which still holds its library until its end.</param>
public sealed record OneShotSoundEffectClip(long ActiveFrames, AudioClipFile? Clip) : SoundEffectClipShape;
