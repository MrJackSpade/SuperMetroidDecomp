
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>The travel axis and alternation that select a wave shape.</summary>
/// <param name="Axis">The axis.</param>
/// <param name="Alternate">The alternate.</param>
internal readonly record struct WaveShapeKey(WaveTravelAxis Axis, bool Alternate);
