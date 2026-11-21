namespace SuperMetroid.Core.Audio;

/// <summary>
/// The closed choice of a sound effect's shape (#2560): a one-shot that reaches its logical end, or a never-ending effect
/// whose channels each loop. Each case carries only its own clips, so a one-shot cannot carry a loop and a loop cannot
/// carry an end.
/// </summary>
public abstract record SoundEffectClipShape;
