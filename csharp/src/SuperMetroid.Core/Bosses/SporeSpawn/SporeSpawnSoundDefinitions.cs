namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The enemy-library sounds Spore Spawn queues.</summary>
internal static class SporeSpawnSoundDefinitions
{
    /// <summary>The opening sound, queued as the head opens.</summary>
    internal static readonly SoundEffectId Opening = EnemySound.ShotSporeSpawn;
    /// <summary>The hardening dust sound, queued with each hardening dust cloud.</summary>
    internal static readonly SoundEffectId HardeningDust = EnemySound.HeavyRumble;
    /// <summary>The dying explosion sound, queued with each dying explosion.</summary>
    internal static readonly SoundEffectId DyingExplosion = EnemySound.BigExplosion;
}
