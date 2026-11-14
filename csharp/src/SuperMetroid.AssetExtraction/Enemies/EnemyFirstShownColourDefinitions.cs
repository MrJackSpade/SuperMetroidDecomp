using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Chozo.Artwork;
using SuperMetroid.Core.Enemies.Corpses.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The colourings each enemy whose encounter's code sets colours, rather than its graphics set's load, leaves before its
/// frames first show (#1513): the shades its encounter lays, in the order they are laid, up to the moment its frames
/// first show, by enemy or, for an enemy whose initialization chooses its colours by how its room places it, by placement
/// (#1642). Its sheets, and the spawned-object sheets holding its objects, are coloured in what those shades leave in the
/// rows they write, so a frame that shows before any later shade does not show the zero colours its header palette or
/// the room load leaves there.
/// </summary>
internal static class EnemyFirstShownColourDefinitions
{
    /// <summary>
    /// The shades laid before each such enemy's frames first show, in laying order. The Baby's cutscene initialization
    /// lays its arrival colours before its first frame. Both Ridleys' initializations load the door's, container's and Baby's
    /// rows (the Ceres Baby and its container show in them from the room's start) and clear his own row, then the eye fade
    /// ends on its brightest shade and the body fade on its full body colours, over his own row and his shadow's, before
    /// either encounter shows him; his row's last colour stays cleared. His Norfair breakup's fragments draw in his row
    /// (<see cref="EnemySheetRowDefinitions.FixedRows"/>) as his fight left it. The Tourian statue scene lays its entrance colours
    /// over the statue and base rows before the scene shows. Each Torizo's initialization loads the orb-and-flame and
    /// statue-shading rows and his first body colouring (Bomb Torizo's statue, Golden Torizo's colours before his first
    /// hurt), which the statue's fragments, his orbs and his frames first show in. Spore Spawn's initialization loads his
    /// spores' colours into their row. Mother Brain's body first shows after her room entry's glass-and-tube load and her
    /// ascent's phase-two load of her attack and rear-leg rows; the objects that show before the ascent take their own
    /// colourings (<see cref="FrameColouringDefinitions"/>). The Shitroid's initialization and its victim Sidehopper's draw
    /// both in the Shitroid encounter's target colours from the room's start (#1513): their room's graphics set loads all its
    /// records into one row and none into the Shitroid's draw row, so neither is ever drawn in the colours that load leaves.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, PictureShade[]> Shades { get; } = new Dictionary<EnemyDefinitionId, PictureShade[]>
    {
        [EnemyDefinitionId.BabyMetroidCutscene] = [MotherBrainShades.BabyArrives],
        [EnemyDefinitionId.BabyMetroid] = [TourianCorpseShades.ShitroidTargets],
        [EnemyDefinitionId.CorpseSidehopper] = [TourianCorpseShades.ShitroidTargets],
        [EnemyDefinitionId.RidleyCeres] =
        [
            RidleyShades.CeresStart,
            RidleyShades.CeresHidden,
            RidleyShades.EyeFade(0),
            RidleyShades.BodyFade(CeresRidleyShadeDefinitions.BodyFadeShadeCount - 1),
        ],
        [EnemyDefinitionId.Ridley] =
        [
            RidleyShades.NorfairStart,
            RidleyShades.NorfairHidden,
            RidleyShades.EyeFade(0),
            RidleyShades.BodyFade(CeresRidleyShadeDefinitions.BodyFadeShadeCount - 1),
        ],
        [EnemyDefinitionId.RidleyExplosion] =
        [
            RidleyShades.NorfairStart,
            RidleyShades.NorfairHidden,
            RidleyShades.EyeFade(0),
            RidleyShades.BodyFade(CeresRidleyShadeDefinitions.BodyFadeShadeCount - 1),
        ],
        [EnemyDefinitionId.TourianStatue] = [TourianStatueShades.Entrance],
        [EnemyDefinitionId.BombTorizo] = [TorizoShades.Shared, TorizoShades.Body(new TorizoLook(TorizoLookKind.BombStatue))],
        [EnemyDefinitionId.GoldenTorizo] = [TorizoShades.Shared, TorizoShades.Body(new TorizoLook(TorizoLookKind.GoldenInitial))],
        [EnemyDefinitionId.SporeSpawn] = [SporeSpawnShades.Spores],
        [EnemyDefinitionId.MotherBrainBody] = [MotherBrainShades.RoomEntry, MotherBrainShades.PhaseTwo],
    };

    /// <summary>
    /// The shades laid before an enemy's frames first show, by the placement its initialization chooses its colours by
    /// (#1642); no enemy named by <see cref="Shades"/> is placed so. Each Chozo statue's initialization clears its palette
    /// bits ($AA:E74C) and loads its site's statue colours into the two rows its parts name, before its first frame: the
    /// Wrecked Ship statue's for the first site ($AA:E75E to $AA:E771), the Lower Norfair statue's for the second ($AA:E784
    /// to $AA:E797). The statue shows them for its whole life, its body in the first row and its walking back leg in the
    /// second, so its sheets show them, never the colours its graphics set's load leaves in those rows.
    /// </summary>
    internal static IReadOnlyDictionary<EnemySpawnSettings, PictureShade[]> ByPlacement { get; } = new Dictionary<EnemySpawnSettings, PictureShade[]>
    {
        [new ChozoStatueSpawnSettings(Site: ChozoStatueSite.WreckedShip)] = [ChozoAndTubeShades.WreckedShipStatue],
        [new ChozoStatueSpawnSettings(Site: ChozoStatueSite.LowerNorfair)] = [ChozoAndTubeShades.LowerNorfairStatue],
    };
}
