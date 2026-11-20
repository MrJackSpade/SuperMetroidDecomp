namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One numbered music track: its top-level phrase-flow program, or none for an inactive track, which plays nothing.</summary>
/// <param name="Track">The track's number.</param>
/// <param name="Id">The track's identity.</param>
/// <param name="Program">The track's program, or null for an inactive track.</param>
public sealed record AudioMusicTrackMetadata(int Track, string Id, AudioMusicTrackProgram? Program);
