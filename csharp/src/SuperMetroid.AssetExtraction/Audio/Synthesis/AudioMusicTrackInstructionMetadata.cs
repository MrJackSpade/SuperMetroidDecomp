using System.Text.Json.Serialization;
using SuperMetroid.Core;

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
public abstract record AudioMusicTrackInstructionMetadata;

/// <summary>Plays one phrase: routes the eight channels to the programs the phrase names.</summary>
/// <param name="Phrase">The identity of the phrase played, one of its bank's phrases.</param>
public sealed record AudioMusicPlayPhrase(string Phrase) : AudioMusicTrackInstructionMetadata;

/// <summary>Turns the driver's fast-forward mode on.</summary>
public sealed record AudioMusicFastForwardOn : AudioMusicTrackInstructionMetadata;

/// <summary>Turns the driver's fast-forward mode off.</summary>
public sealed record AudioMusicFastForwardOff : AudioMusicTrackInstructionMetadata;

/// <summary>
/// Repeats the track from one of its own instructions, <paramref name="Target"/>, as the driver counts
/// <paramref name="Count"/>.
/// </summary>
/// <param name="Count">The repeat count, 1 through 255, neither of the fast-forward control words.</param>
/// <param name="Target">The place in the track's instruction list of the instruction it repeats from.</param>
public sealed record AudioMusicRepeat(int Count, int Target) : AudioMusicTrackInstructionMetadata
{
    /// <summary>The place in the track's instruction list of the instruction it repeats from, zero or more.</summary>
    public int Target { get; init; } = Ensure.AtLeastZero(Target);
}

/// <summary>Ends the track; no instruction follows it.</summary>
public sealed record AudioMusicTrackEnd : AudioMusicTrackInstructionMetadata;
