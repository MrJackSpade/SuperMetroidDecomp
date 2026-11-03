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
    /// <summary>The selected presentation of the whole enemy directory: an edit to any of its enemy, boss or area artwork changes it.</summary>
    public string DirectoryIdentity =>
        SelectedPresentationHash.Create("enemy-directory-v1", content =>
        {
            content.AppendIdentity("enemies", Enemies.ContentIdentity);
            content.AppendIdentity("bosses", Bosses.ContentIdentity);
            content.AppendIdentity("areas", Areas.ContentIdentity);
        });
}
