using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Fixed generation, health-tier, and respawn definitions for Tourian's Zebetites.</summary>
internal static class ZebetiteDefinitions
{
    /// <summary>The two colours the Zebetites pulse: the second enemy store's colours twelve and thirteen.</summary>
    public static readonly SpriteColourRun PulseColours = new(SpriteColourStore.EnemiesSecond, 12, 2);

    /// <summary>The steps of the shared pulse cycle physical enemy slot zero counts through before it restarts.</summary>
    public const int PaletteCycleLength = 8;

    /// <summary>Embedded primary spawn record at <c>$A6:FCE1-$A6:FCF0</c>.</summary>
    internal static readonly RoomEnemyPopulationRecord PrimarySpawn =
        new(EnemyDefinitionId.Zebetite, new(0, 0), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new ZebetiteSpawnSettings(ZebetiteHalf.Primary));

    /// <summary>Embedded linked-half spawn record at <c>$A6:FCF9-$A6:FD08</c>.</summary>
    internal static readonly RoomEnemyPopulationRecord LinkedSpawn =
        new(EnemyDefinitionId.Zebetite, new(0, 0), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new ZebetiteSpawnSettings(ZebetiteHalf.Linked));

    /// <summary>The exact embedded enemy population record of each half.</summary>
    internal static readonly Dictionary<ZebetiteHalf, RoomEnemyPopulationRecord> SpawnPopulations = new()
    {
        [ZebetiteHalf.Primary] = PrimarySpawn,
        [ZebetiteHalf.Linked] = LinkedSpawn,
    };
}
