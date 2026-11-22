namespace SuperMetroid.Core.Audio;

/// <summary>A never-ending sound effect (#2560): one looping layer per channel, which loop independently and play together.</summary>
/// <param name="Layers">The channels' layers.</param>
public sealed partial record LoopingSoundEffectClip(IReadOnlyList<LoopingSoundEffectLayer> Layers) : SoundEffectClipShape;
