using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// The frames each enemy's encounter draws itself, outside the enemy list, at character index zero (#1392): Ridley's tail
/// and wings, in Ceres the Baby he carries and the door overlay his scene draws, and the dead Torizo's hook of sand. Each graphics set's character-zero
/// spawned-object sheet holds those of the enemies it draws.
/// </summary>
internal static class EncounterFrameDefinitions
{
    /// <summary>Ridley's tail tip, tail segments and wings.</summary>
    private static readonly EnemyFrameId[] RidleySupplements =
    [
        .. RidleySupplementalVisualDefinitions.TailTipFrames.Select(entry => entry.Value),
        .. RidleySupplementalVisualDefinitions.SegmentFrames.Select(entry => entry.Value),
        .. RidleySupplementalVisualDefinitions.WingFrames.Select(entry => entry.Value),
    ];

    /// <summary>The frames each enemy's encounter draws itself.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>> ByEnemy { get; } =
        new Dictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>>
        {
            [EnemyDefinitionId.Ridley] = RidleySupplements,
            [EnemyDefinitionId.CorpseTorizo] = [DeadTorizoArtworkDefinitions.HookFrame],
            [EnemyDefinitionId.RidleyCeres] =
            [
                .. RidleySupplements,
                .. CeresBabyFrameDefinitions.ShownFrames.Select(entry => entry.Value),
                CeresDoorFrameDefinitions.RidleyPrivateOverlayFrame,
            ],
        };
}
