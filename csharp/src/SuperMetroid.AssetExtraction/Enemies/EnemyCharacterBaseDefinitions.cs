using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The enemies whose initialization draws them from other characters than their room's graphics set assigns them, and
/// so the characters their pre-coloured sheets compose them at (#1392, #1513). The game never reads a character index:
/// it draws each enemy from its own sheet, and the objects it spawns from its own spawned-object sheet.
/// </summary>
internal static class EnemyCharacterBaseDefinitions
{
    /// <summary>
    /// The character index each enemy that fixes one draws from: Ceres's doors and steam draw from characters loaded with
    /// the Ceres tileset, a Zebetite from those following its room's first 128 enemy tiles, the fragments Ridley's death
    /// scatters from the first characters, Mother Brain's body and head from the first characters, which their
    /// initialisation selects whatever their place in the room's graphics set, and the Baby of Mother Brain's fight from
    /// the characters its tile rows are transferred to (#1435).
    /// </summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, int> Fixed { get; } = new Dictionary<EnemyDefinitionId, int>
    {
        [EnemyDefinitionId.CeresDoor] = 0,
        [EnemyDefinitionId.Steam] = 0,
        [EnemyDefinitionId.Zebetite] = 128,
        [EnemyDefinitionId.RidleyExplosion] = 0,
        [EnemyDefinitionId.MotherBrainBody] = 0,
        [EnemyDefinitionId.MotherBrainHead] = 0,
        [EnemyDefinitionId.BabyMetroidCutscene] = 160,
    };

    /// <summary>
    /// The characters the shards of Mother Brain's tank glass and their sparkles draw from: those following her first 64,
    /// whatever their spawner's own (#1435).
    /// </summary>
    internal const int MotherBrainTankGlassCharacters = 64;

    /// <summary>
    /// The enemy whose character index each enemy that copies one draws at: the slot it follows. A gunship's bottom
    /// reads its top's, a Baby Turtle its Mama's, Ki-Hunter wings their body's, an Evir's projectile its Evir's and
    /// Phantoon's eye, tentacles and mouth her body's.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, EnemyDefinitionId> DrawsAtTilesOf { get; } = new Dictionary<EnemyDefinitionId, EnemyDefinitionId>
    {
        [EnemyDefinitionId.ShipBottomEntrance] = EnemyDefinitionId.ShipTop,
        [EnemyDefinitionId.BabyTurtle] = EnemyDefinitionId.MamaTurtle,
        [EnemyDefinitionId.KihunterGreenWings] = EnemyDefinitionId.KihunterGreen,
        [EnemyDefinitionId.KihunterYellowWings] = EnemyDefinitionId.KihunterYellow,
        [EnemyDefinitionId.KihunterRedWings] = EnemyDefinitionId.KihunterRed,
        [EnemyDefinitionId.EvirProjectile] = EnemyDefinitionId.Evir,
        [EnemyDefinitionId.PhantoonEye] = EnemyDefinitionId.PhantoonBody,
        [EnemyDefinitionId.PhantoonTentacles] = EnemyDefinitionId.PhantoonBody,
        [EnemyDefinitionId.PhantoonMouth] = EnemyDefinitionId.PhantoonBody,
    };
}
