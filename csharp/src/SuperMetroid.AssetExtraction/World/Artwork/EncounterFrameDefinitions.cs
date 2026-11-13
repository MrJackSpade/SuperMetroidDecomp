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
        .. RidleySupplementalVisualDefinitions.TailTipFrames.InTurnOrder,
        .. RidleySupplementalVisualDefinitions.SegmentFrames.Select(entry => entry.Value),
        .. RidleySupplementalVisualDefinitions.WingFrames.Select(entry => entry.Value),
    ];

    /// <summary>
    /// The encounter frames whose draws OR their encounter's palette index into every part: Ridley's tail and wings, drawn
    /// with the spritemap palette index his initialization stores ($A6:A138, read at $A6:DAEE). The Ceres Baby's draw
    /// passes none ($A6:BF2C), so its parts show their own rows.
    /// </summary>
    internal static IReadOnlySet<EnemyFrameId> DrawnInEncounterRow { get; } = new HashSet<EnemyFrameId>(RidleySupplements);

    /// <summary>
    /// The encounter frames whose draws OR another enemy's row into every part: the door overlay Ceres Ridley's scene draws
    /// in the row his room's set loads the Ceres door's colours into ($A6:A314).
    /// </summary>
    internal static IReadOnlyDictionary<EnemyFrameId, EnemyDefinitionId> DrawnInRowOf { get; } = new Dictionary<EnemyFrameId, EnemyDefinitionId>
    {
        [CeresDoorFrameDefinitions.RidleyPrivateOverlayFrame] = EnemyDefinitionId.CeresDoor,
    };

    /// <summary>The frames each enemy's encounter draws itself.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>> ByEnemy { get; } =
        new Dictionary<EnemyDefinitionId, IReadOnlyList<EnemyFrameId>>
        {
            [EnemyDefinitionId.Ridley] = RidleySupplements,
            [EnemyDefinitionId.CorpseTorizo] = [DeadTorizoArtworkDefinitions.SandHeapFrame],
            [EnemyDefinitionId.RidleyCeres] =
            [
                .. RidleySupplements,
                .. CeresBabyFrameDefinitions.ShownFrames.Select(entry => entry.Value),
                CeresDoorFrameDefinitions.RidleyPrivateOverlayFrame,
            ],
        };
}
