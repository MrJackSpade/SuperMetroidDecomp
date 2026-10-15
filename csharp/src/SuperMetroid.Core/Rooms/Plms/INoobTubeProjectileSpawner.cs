namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The N00b tube's shard projectiles (#633).</summary>
public interface INoobTubeProjectileSpawner
{
    /// <summary>Spawns one N00b tube shard projectile.</summary>
    void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request);
}
