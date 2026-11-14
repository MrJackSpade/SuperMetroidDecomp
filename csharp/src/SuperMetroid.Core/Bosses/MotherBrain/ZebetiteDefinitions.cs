using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Fixed generation, health-tier, and respawn definitions for Tourian's Zebetites.</summary>
internal static class ZebetiteDefinitions
{
    /// <summary>The steps of the Zebetites' shared pulse cycle before it restarts.</summary>
    public const int PaletteCycleLength = 8;

    /// <summary>The primary half's embedded population entry.</summary>
    internal static readonly RoomEnemyPlacement PrimarySpawn =
        new(EnemyDefinitionId.Zebetite, new(0, 0), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, CollidesByFrameHitboxes: false, new ZebetiteSpawnSettings(ZebetiteHalf.Primary));

    /// <summary>The linked half's embedded population entry.</summary>
    internal static readonly RoomEnemyPlacement LinkedSpawn =
        new(EnemyDefinitionId.Zebetite, new(0, 0), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, CollidesByFrameHitboxes: false, new ZebetiteSpawnSettings(ZebetiteHalf.Linked));

    /// <summary>The exact embedded population entry of each half.</summary>
    internal static readonly Dictionary<ZebetiteHalf, RoomEnemyPlacement> SpawnPopulations = new()
    {
        [ZebetiteHalf.Primary] = PrimarySpawn,
        [ZebetiteHalf.Linked] = LinkedSpawn,
    };
}
