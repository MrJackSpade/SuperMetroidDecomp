namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>A voice's base pan volume and the next.</summary>
/// <param name="Base">The base.</param>
/// <param name="Next">The next.</param>
internal readonly record struct PanVolumePair(int Base, int Next);
