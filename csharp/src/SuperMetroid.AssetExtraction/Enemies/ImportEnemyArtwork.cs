namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The enemy directory as the import reads it, from one manifest (#1395), with its characters from the component's
/// import data (#1636), published as the catalogs each domain owns: the ordinary enemies', the bosses' and the areas'. The
/// game reads none of it; it reads <see cref="InstalledEnemyArtwork"/> and draws the sheets composed from this.
/// </summary>
/// <param name="Enemies">The ordinary enemy sheets, palettes, spritemaps and enemy colors.</param>
/// <param name="Bosses">The bosses' melts, skeletons, BG2 frames, special pages and colors.</param>
/// <param name="Areas">The Ceres door and escape artwork and the Tourian statues' colors.</param>
public sealed record ImportEnemyArtwork(EnemyTileSourceArtwork Enemies, BossSourceArtwork Bosses, AreaSourceArtwork Areas);
