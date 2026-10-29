using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed enemy-directory artwork, read once from one manifest and published as the catalogs each domain owns
/// (#1395): the ordinary enemies', the bosses' and the areas'.
/// </summary>
/// <param name="Enemies">The ordinary enemy sheets, palettes, spritemaps and enemy colors.</param>
/// <param name="Bosses">The bosses' melts, skeletons, BG2 frames, special pages and colors.</param>
/// <param name="Areas">The Ceres door and escape artwork and the Tourian statues' colors.</param>
public sealed record InstalledEnemyArtwork(EnemyTileSourceArtwork Enemies, BossSourceArtwork Bosses, AreaSourceArtwork Areas)
{
    /// <summary>The selected presentation of a whole enemy directory bound as the catalogs gameplay reads.</summary>
    /// <param name="enemies">The ordinary enemies' artwork.</param>
    /// <param name="bosses">The bosses' artwork.</param>
    /// <param name="areas">The areas' artwork.</param>
    public static string DirectoryIdentity(EnemyTileArtworkCatalog enemies, BossArtworkCatalog bosses, AreaArtworkBundle areas) =>
        SelectedPresentationHash.Create("enemy-directory-v1", content =>
        {
            content.AppendIdentity("enemies", enemies.ContentIdentity);
            content.AppendIdentity("bosses", bosses.ContentIdentity);
            content.AppendIdentity("areas", areas.ContentIdentity);
        });
}
