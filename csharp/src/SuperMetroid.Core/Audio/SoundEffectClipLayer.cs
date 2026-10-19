namespace SuperMetroid.Core.Audio;

/// <summary>
/// One layer of a sound-effect clip (#1305): the whole sound for a one-shot effect, or one channel
/// of a never-ending effect, whose channels loop independently and play together.
/// </summary>
/// <param name="Clip">The rendered layer.</param>
/// <param name="LoopStart">For a looping channel, the first frame of its loop, repeated to the clip's end; null for a one-shot layer.</param>
/// <param name="Voices">The hardware voices this layer sounds on.</param>
/// <param name="KeyOnFrames">Clip-relative frames at which the layer keys a note on; an upload silences the layer until its next key-on.</param>
public sealed record SoundEffectClipLayer(
    AudioClipFile Clip,
    long? LoopStart,
    SoundEffectVoices Voices,
    IReadOnlyList<long> KeyOnFrames);
