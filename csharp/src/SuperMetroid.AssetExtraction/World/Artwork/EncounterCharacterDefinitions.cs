using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// The characters each enemy's encounter shows after its room loads (#1392, #1406), by the enemy whose graphics set loads
/// the encounter. Pre-coloured frames drawn over them have a picture for each. Where each shows among the sprite
/// characters is extraction's: the installed catalogue pairs each with its place. The list lives above the encounters'
/// modules, which each own what they show.
/// </summary>
internal static class EncounterCharacterDefinitions
{
    /// <summary>
    /// Each enemy's encounter's uploads: Draygon's opening uploads the Evirs she releases, the gunship's takeoff its five
    /// liftoff pictures, Crocomire's death his skeleton, a Torizo's instruction lists the characters of the poses it
    /// shows next, Ceres Ridley's getaway the escape timer and its warning, and the Ceres door's second variant its characters.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, IReadOnlyList<SpriteArtworkSource>> Encounters { get; } =
        new Dictionary<EnemyDefinitionId, IReadOnlyList<SpriteArtworkSource>>
        {
            [EnemyDefinitionId.DraygonBody] = [DraygonIntroPresentationDefinitions.EvirTiles],
            [EnemyDefinitionId.ShipTop] = GunshipLiftoffTransferDefinitions.Pages,
            [EnemyDefinitionId.Crocomire] = CrocomireSkeletonTransferDefinitions.Sources,
            [EnemyDefinitionId.BombTorizo] = TorizoArtworkSwapDefinitions.Sources,
            [EnemyDefinitionId.GoldenTorizo] = TorizoArtworkSwapDefinitions.Sources,
            [EnemyDefinitionId.RidleyCeres] = CeresEscapeObjectPages.ObjectPages,
            [EnemyDefinitionId.CeresDoor] = [CeresDoorCharacters.Shown],
            [EnemyDefinitionId.MotherBrainHead] = MotherBrainCharacterSources.All,
        };
}
