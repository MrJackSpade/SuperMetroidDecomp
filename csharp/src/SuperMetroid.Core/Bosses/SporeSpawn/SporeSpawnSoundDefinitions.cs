namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The library-two sounds Spore Spawn queues.</summary>
internal static class SporeSpawnSoundDefinitions
{
    /// <summary>The opening sound $2C, queued as the head opens ($A5:E6E7).</summary>
    internal const ushort Opening = 0x002c;
    /// <summary>The hardening dust sound $29, queued with each hardening dust cloud.</summary>
    internal const ushort HardeningDust = 0x0029;
    /// <summary>The dying explosion sound $25, queued with each dying explosion.</summary>
    internal const ushort DyingExplosion = 0x0025;
}
