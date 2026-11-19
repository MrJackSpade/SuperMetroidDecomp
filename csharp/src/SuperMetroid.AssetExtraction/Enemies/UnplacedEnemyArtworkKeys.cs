using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The key of each <see cref="UnplacedEnemyArtwork"/> (#2548) in the enemy-tile import data: the name its manifest entry
/// and its character page and colour document are filed under, written out so renaming a member never changes it.
/// </summary>
internal static class UnplacedEnemyArtworkKeys
{
    /// <summary>Each UnplacedEnemyArtwork member's key.</summary>
    internal static InstalledKeys<UnplacedEnemyArtwork> Installed { get; } = new(new Dictionary<UnplacedEnemyArtwork, string>
    {
        [UnplacedEnemyArtwork.Stoke] = "Stoke",
        [UnplacedEnemyArtwork.DeadSidehopperLivingTiles] = "DeadSidehopperLivingTiles",
        [UnplacedEnemyArtwork.TourianStatueGhosts] = "TourianStatueGhosts",
        [UnplacedEnemyArtwork.CeresDoors] = "CeresDoors",
    });
}
