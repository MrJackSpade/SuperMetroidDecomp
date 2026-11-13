using SuperMetroid.Core.Audio;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>
/// One layer of a sound-effect clip as the clip manifest stores it (#1305): the layer the game plays.
/// </summary>
/// <param name="Clip">The rendered layer.</param>
/// <param name="LoopStart">For a looping channel, the first frame of its loop, repeated to the clip's end; null for a one-shot layer.</param>
/// <param name="NoteStarts">Clip frames at which the layer starts a note, in order; a clip-set switch interrupts the layer until its next note (#1725).</param>
public sealed record InstalledSoundEffectLayer(
    AudioClipFile Clip,
    long? LoopStart,
    IReadOnlyList<long> NoteStarts)
{
    /// <summary>The layer the game plays; a method, so the manifest does not store it.</summary>
    public SoundEffectClipLayer PlayedLayer() => new(Clip, LoopStart, NoteStarts);
}
