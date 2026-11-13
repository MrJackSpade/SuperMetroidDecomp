using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Fixed generation, health-tier, and respawn definitions for Tourian's Zebetites.</summary>
internal static class ZebetiteDefinitions
{
    /// <summary>The steps of the Zebetites' shared pulse cycle before it restarts.</summary>
    public const int PaletteCycleLength = 8;

    /// <summary>Embedded primary spawn record.</summary>
    internal static readonly RoomEnemyPopulationRecord PrimarySpawn =
        new(EnemyDefinitionId.Zebetite, new(0, 0), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, CollidesByFrameHitboxes: false, new ZebetiteSpawnSettings(ZebetiteHalf.Primary));

    /// <summary>Embedded linked-half spawn record.</summary>
    internal static readonly RoomEnemyPopulationRecord LinkedSpawn =
        new(EnemyDefinitionId.Zebetite, new(0, 0), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, CollidesByFrameHitboxes: false, new ZebetiteSpawnSettings(ZebetiteHalf.Linked));

    /// <summary>The exact embedded enemy population record of each half.</summary>
    internal static readonly Dictionary<ZebetiteHalf, RoomEnemyPopulationRecord> SpawnPopulations = new()
    {
        [ZebetiteHalf.Primary] = PrimarySpawn,
        [ZebetiteHalf.Linked] = LinkedSpawn,
    };
}
