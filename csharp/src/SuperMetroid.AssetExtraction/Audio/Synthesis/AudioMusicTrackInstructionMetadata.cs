using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// One phrase-flow instruction of a music track (#2548): a closed choice of playing a phrase, turning the driver's
/// fast-forward on or off, repeating from an earlier instruction, or ending the track. Each holds only its own operands,
/// and the installed manifest names each by its written-out operation.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "operation")]
[JsonDerivedType(typeof(AudioMusicPlayPhrase), "playPhrase")]
[JsonDerivedType(typeof(AudioMusicFastForwardOn), "fastForwardOn")]
[JsonDerivedType(typeof(AudioMusicFastForwardOff), "fastForwardOff")]
[JsonDerivedType(typeof(AudioMusicRepeat), "repeat")]
[JsonDerivedType(typeof(AudioMusicTrackEnd), "end")]
public abstract partial record AudioMusicTrackInstructionMetadata;
