using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// What Bomb and Golden Torizo's blow-ups and death leave and how long they flash (#2032): the gut's drool, the low-health
/// and death explosions, and the death drops over each Torizo's arena.
/// </summary>
internal static class TorizoDeathDefinitions
{
    /// <summary>The drool pieces Bomb Torizo's gut spills as it blows up.</summary>
    internal const int GutDroolPieces = 6;

    /// <summary>The explosions each burst of low-health explosions spawns at the gut or the face.</summary>
    internal const int LowHealthExplosions = 6;

    /// <summary>The updates a burst of low-health explosions flashes the Torizo, as long as its animation waits after it.</summary>
    internal const int LowHealthExplosionFlash = 40;

    /// <summary>The updates a death explosion flashes the Torizo, as long as its flicker waits after it.</summary>
    internal const int DeathExplosionFlash = 1;

    /// <summary>The drops a Torizo's death scatters.</summary>
    internal const int DeathDrops = 16;

    /// <summary>
    /// The chances both Torizos' death drops roll, Golden Torizo's as well as Bomb Torizo's (#2244): Bomb Torizo's own,
    /// never nothing.
    /// </summary>
    internal static readonly EnemyDropChances DeathDropChances = new(SmallEnergy: 50, BigEnergy: 50, Missile: 50, Nothing: 0, SuperMissile: 50, PowerBomb: 55);

    /// <summary>Bomb Torizo's arena: a narrow stretch of the upper room.</summary>
    internal static readonly TorizoDropArena BombTorizoArena =
        new(new(64, 96), EnemyDropScatterDefinitions.NarrowArenaHorizontalScatter, EnemyDropScatterDefinitions.ArenaBandVerticalScatter);

    /// <summary>Golden Torizo's arena: a wide strip much lower in its room.</summary>
    internal static readonly TorizoDropArena GoldenTorizoArena =
        new(new(128, 288), EnemyDropScatterDefinitions.WideArenaHorizontalScatter, EnemyDropScatterDefinitions.ArenaBandVerticalScatter);
}
