
namespace SuperMetroid.Core.Audio;

/// <summary>A sound effect command within its library.</summary>
/// <param name="Library">The library.</param>
/// <param name="Command">The command.</param>
public readonly record struct SoundEffectPolicyKey(SoundEffectLibrary Library, int Command);
