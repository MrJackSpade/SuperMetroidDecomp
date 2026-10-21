namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>The voices keyed on at a sample.</summary>
/// <param name="Sample">The sample.</param>
/// <param name="Voices">The voices.</param>
internal readonly record struct KeyOnSample(long Sample, int Voices);
