namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>A tempo a music track sets at a sample.</summary>
/// <param name="Sample">The sample.</param>
/// <param name="Tempo">The tempo.</param>
public readonly partial record struct TempoChange(long Sample, int Tempo);
