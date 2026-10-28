namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemies whose initialization draws them from other characters, or in another palette, than their room's graphics
/// set assigns them, and so the characters and palette their pre-coloured sheets compose them in (#1392).
/// </summary>
public static class EnemyTileBaseDefinitions
{
    /// <summary>
    /// The character index each enemy that fixes one draws from: Ceres's doors and steam draw from characters loaded with
    /// the Ceres tileset, a Zebetite from those following its room's first 128 enemy tiles, the fragments Ridley's death
    /// scatters from the first characters, Mother Brain's body and head from the first characters, which their
    /// initialisation selects whatever their place in the room's graphics set, and the Baby of Mother Brain's fight from
    /// the characters its tile rows are transferred to (#1435).
    /// </summary>
    public static IReadOnlyDictionary<EnemyDefinitionId, int> Fixed { get; } = new Dictionary<EnemyDefinitionId, int>
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
    /// The characters Mother Brain's charging hand beam and the shards of her tank's glass draw from: those following her
    /// first 64, whatever their spawner's own (#1435).
    /// </summary>
    public const int MotherBrainObjectCharacters = 0x40;

    /// <summary>
    /// The character indexes, beside its own, each enemy's spawned objects draw from, so its spawned-object sheets are
    /// composed there too: Mother Brain's at <see cref="MotherBrainObjectCharacters"/> (#1435).
    /// </summary>
    public static IReadOnlyDictionary<EnemyDefinitionId, int[]> SpawnsAlsoAt { get; } = new Dictionary<EnemyDefinitionId, int[]>
    {
        [EnemyDefinitionId.MotherBrainBody] = [MotherBrainObjectCharacters],
    };

    /// <summary>
    /// The enemy whose character index each enemy that copies one draws at: the slot it follows. A gunship's bottom
    /// reads its top's, a Baby Turtle its Mama's, Ki-Hunter wings their body's, an Evir's projectile its Evir's and
    /// Phantoon's eye, tentacles and mouth her body's.
    /// </summary>
    public static IReadOnlyDictionary<EnemyDefinitionId, EnemyDefinitionId> DrawsAtTilesOf { get; } = new Dictionary<EnemyDefinitionId, EnemyDefinitionId>
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

    /// <summary>
    /// The enemy whose palette each enemy that copies one draws in: a Baby Turtle its Mama's, Ki-Hunter wings their
    /// body's, an Evir's projectile its Evir's and Phantoon's eye, tentacles and mouth her body's.
    /// </summary>
    public static IReadOnlyDictionary<EnemyDefinitionId, EnemyDefinitionId> DrawsInPaletteOf { get; } = new Dictionary<EnemyDefinitionId, EnemyDefinitionId>
    {
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
