namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Spawns the shards Mother Brain's breaking glass throws, among the room's enemy projectiles.</summary>
internal interface IMotherBrainGlassShardSpawner
{
    /// <summary>Spawns the shard <paramref name="request"/> names at the glass's block.</summary>
    void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request);
}
