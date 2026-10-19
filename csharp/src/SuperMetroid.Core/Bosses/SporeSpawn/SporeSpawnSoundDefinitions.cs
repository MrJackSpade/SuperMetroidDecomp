namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The library-two sounds Spore Spawn queues.</summary>
internal static class SporeSpawnSoundDefinitions
{
    /// <summary>The opening sound $2C, queued as the head opens ($A5:E6E7).</summary>
    internal static readonly SoundEffectId Opening = new(SoundEffectLibrary.Library2, 0x2c);
    /// <summary>The hardening dust sound $29, queued with each hardening dust cloud.</summary>
    internal static readonly SoundEffectId HardeningDust = new(SoundEffectLibrary.Library2, 0x29);
    /// <summary>The dying explosion sound $25, queued with each dying explosion.</summary>
    internal static readonly SoundEffectId DyingExplosion = new(SoundEffectLibrary.Library2, 0x25);
}
