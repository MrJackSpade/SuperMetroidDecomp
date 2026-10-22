using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.World.Artwork;

/// <summary>
/// The uploads each enemy's encounter makes over the OBJ characters after its room loads (#1392), by the enemy whose
/// graphics set loads the encounter. Pre-coloured frames drawn over them have a picture for each. The catalogue lives
/// above the encounters' modules, which each own their uploads.
/// </summary>
internal static class RuntimeCharacterUploadDefinitions
{
    /// <summary>
    /// Each enemy's encounter's uploads: Draygon's opening uploads the Evirs she releases, the gunship's takeoff its five
    /// liftoff pictures, Crocomire's death his skeleton and a Torizo's instruction lists the characters of the poses it
    /// shows next.
    /// </summary>
    internal static RuntimeCharacterUploadCatalog Catalog { get; } = new(new Dictionary<EnemyDefinitionId, IReadOnlyList<RuntimeCharacterUpload>>
    {
        [EnemyDefinitionId.DraygonBody] = [DraygonIntroPresentationDefinitions.EvirUpload],
        [EnemyDefinitionId.ShipTop] = GunshipLiftoffTransferDefinitions.Uploads,
        [EnemyDefinitionId.Crocomire] = CrocomireSkeletonTransferDefinitions.Uploads,
        [EnemyDefinitionId.BombTorizo] = TorizoInstructionVramTransferDefinitions.Uploads,
        [EnemyDefinitionId.GoldenTorizo] = TorizoInstructionVramTransferDefinitions.Uploads,
    });
}
