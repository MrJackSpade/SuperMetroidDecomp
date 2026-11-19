namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Plays one phrase: routes the eight channels to the programs the phrase names.</summary>
/// <param name="Phrase">The identity of the phrase played, one of its bank's phrases.</param>
public sealed record AudioMusicPlayPhrase(string Phrase) : AudioMusicTrackInstructionMetadata;
