using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>An encounter's characters placed where the cartridge shows them, as extraction's installed catalogue places them (#1406).</summary>
internal static class PlacedUploads
{
    /// <summary>The upload of <paramref name="source"/> where the cartridge shows it.</summary>
    internal static SpriteArtworkPlacement Of(SpriteArtworkSource source) => EncounterCharacterPlacements.Placed(source);

    /// <summary>The uploads of <paramref name="sources"/>, in order, where the cartridge shows each.</summary>
    internal static IReadOnlyList<SpriteArtworkPlacement> Of(IEnumerable<SpriteArtworkSource> sources) => [.. sources.Select(Of)];
}
