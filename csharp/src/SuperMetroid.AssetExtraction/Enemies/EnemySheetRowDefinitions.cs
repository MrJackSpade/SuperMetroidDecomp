using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The sprite palette rows the import colours enemy sheets and their shades in (#1512): where a room load leaves the
/// shared colours, and which enemies' initialization draws them in another enemy's or a shared row than their graphics
/// set's. Gameplay names none of them.
/// </summary>
internal static class EnemySheetRowDefinitions
{
    /// <summary>The row of the common sprite colours a room load fills, which a hit flash shows.</summary>
    internal const int Common = 0;

    /// <summary>The row of the enemy projectiles' colours a room load fills, which an enemy its set does not load draws in.</summary>
    internal const int EnemyProjectiles = 5;

    /// <summary>The row Samus's beam colours fill, which a frozen enemy shows.</summary>
    internal const int Beams = 6;

    /// <summary>The row the Ninja Space Pirates' initialization loads the gold wall Pirate's colours into.</summary>
    internal const int NinjaGoldWall = 7;

    /// <summary>The row the doors of Ceres show their colours in once active, and the escape's falling tiles show.</summary>
    internal const int CeresDoorActive = 7;

    /// <summary>The row the Shitroid's encounter installs the drained corpses' colours in.</summary>
    internal const int DrainedCorpses = 7;

    /// <summary>The rows the Tourian statue scene's initialization installs its statue and base decoration colours in.</summary>
    internal const int TourianStatueBody = 2;

    /// <summary>The row of the Tourian statue scene's base decoration colours, whose colours nine to twelve are the eyes.</summary>
    internal const int TourianStatueBase = 7;

    /// <summary>The row Ridley's statue's colours lie in.</summary>
    internal const int TourianStatueRidley = 1;

    /// <summary>The first of the two rows the Chozo statues and the n00b tube's cracks install their 32 colours in.</summary>
    internal const int ChozoAndTubeFirst = 1;

    /// <summary>The colour within its row a dead Sidehopper's drain starts at; colour zero stays transparent.</summary>
    internal const int SidehopperDrainFirstColour = 1;

    /// <summary>The colour within its row the face block's and the Work Robot's glows start at.</summary>
    internal const int GlowFirstColour = 9;

    /// <summary>
    /// The enemy whose row each enemy that copies one draws in: a Baby Turtle its Mama's, Ki-Hunter wings their body's, an
    /// Evir's projectile its Evir's and Phantoon's eye, tentacles and mouth her body's.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, EnemyDefinitionId> DrawsInRowOf { get; } = new Dictionary<EnemyDefinitionId, EnemyDefinitionId>
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

    /// <summary>
    /// The shared row each enemy whose initialization selects one draws in for its whole life: the Tourian statues', the
    /// Chozo statues', the Torizos' and the escape Etecoons' common colours (each initialization clears the enemy's palette
    /// bits, so its parts show the rows their own bits name), Ceres's steam in the enemy projectiles' colours.
    /// </summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, int> FixedRows { get; } = new Dictionary<EnemyDefinitionId, int>
    {
        [EnemyDefinitionId.TourianStatue] = Common,
        [EnemyDefinitionId.Chozo] = Common,
        [EnemyDefinitionId.BombTorizo] = Common,
        [EnemyDefinitionId.GoldenTorizo] = Common,
        [EnemyDefinitionId.EtecoonEscape] = Common,
        [EnemyDefinitionId.Steam] = EnemyProjectiles,
    };
}
