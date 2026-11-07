namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The library-two sounds Spore Spawn queues.</summary>
internal static class SporeSpawnSoundDefinitions
{
    /// <summary>The opening sound, queued as the head opens.</summary>
    internal static readonly SoundEffectId Opening = Library2Sound.ShotSporeSpawn;
    /// <summary>The hardening dust sound, queued with each hardening dust cloud.</summary>
    internal static readonly SoundEffectId HardeningDust = Library2Sound.HeavyRumble;
    /// <summary>The dying explosion sound, queued with each dying explosion.</summary>
    internal static readonly SoundEffectId DyingExplosion = Library2Sound.BigExplosion;
}
