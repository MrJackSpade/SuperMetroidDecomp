namespace SuperMetroid.Core.Audio;

/// <summary>One channel of a never-ending sound effect (#1305, #2560).</summary>
/// <param name="Clip">The rendered channel.</param>
/// <param name="LoopStart">The first frame of its loop, repeated to the clip's end.</param>
public sealed record LoopingSoundEffectLayer(AudioClipFile Clip, long LoopStart);
