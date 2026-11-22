using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>What a graphics-set record loads (#2560): a closed choice of an enemy's artwork or artwork for no enemy.</summary>
internal abstract partial record RoomEnemyGraphicsSetSource
{
    /// <summary>The source's name in diagnostics.</summary>
    internal abstract string Name { get; }

    /// <summary>The bytes of the tile set the source uploads, or null when the import does not catalogue it.</summary>
    internal abstract int? TileByteCount { get; }

    /// <summary>The fixed staging block the source uploads its tiles to; null when they follow the previous record's.</summary>
    internal abstract int? FixedBlock { get; }

    /// <summary>The installed tiles the source uploads, from <paramref name="artwork"/>.</summary>
    internal abstract ReadOnlyMemory<byte> TilesIn(EnemyTileSourceArtwork artwork);

    /// <summary>The installed colours the source loads, from <paramref name="artwork"/>.</summary>
    internal abstract EnemyPaletteSheet PaletteIn(EnemyTileSourceArtwork artwork);
}
