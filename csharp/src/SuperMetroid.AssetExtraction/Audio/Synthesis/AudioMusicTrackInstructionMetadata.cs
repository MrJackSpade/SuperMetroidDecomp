namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One phrase-flow operation; repeat uses Target while other operations do not.</summary>
public sealed record AudioMusicTrackInstructionMetadata(
    string Operation,
    ushort Value,
    ushort? Target);
