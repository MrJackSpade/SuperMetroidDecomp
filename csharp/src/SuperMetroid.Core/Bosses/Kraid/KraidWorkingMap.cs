using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The background artwork Kraid's working tilemap was built for, and its words.</summary>
/// <param name="Art">The art.</param>
/// <param name="Words">The words.</param>
internal readonly record struct KraidWorkingMap(KraidBackgroundArtwork Art, ushort[] Words);
