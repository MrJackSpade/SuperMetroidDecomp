using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The background artwork Kraid's working tilemap was built for, and its tiles.</summary>
/// <param name="Art">The art.</param>
/// <param name="Tiles">The working map's tiles.</param>
internal readonly record struct KraidWorkingMap(KraidBackgroundArtwork Art, BackgroundTile[] Tiles);
