namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>The two previous decoded samples a BRR loop resumes from.</summary>
/// <param name="Old">The old.</param>
/// <param name="Older">The older.</param>
internal readonly record struct BrrHistory(int Old, int Older);
