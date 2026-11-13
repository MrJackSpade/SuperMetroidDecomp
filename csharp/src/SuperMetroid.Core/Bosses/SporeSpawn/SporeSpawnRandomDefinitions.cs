namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The random choices of Spore Spawn's fight and death: its dust, explosions, drops and spore releases.</summary>
internal static class SporeSpawnRandomDefinitions
{
    /// <summary>An idle dust cloud's X across its 64-pixel strip from X 96: the sample modulo 64.</summary>
    internal static readonly RandomSampleField IdleDustX = new(0, 6);

    /// <summary>An idle dust cloud's Y down its 16-pixel band from Y 480: the sample divided by 256, modulo 16.</summary>
    internal static readonly RandomSampleField IdleDustY = new(8, 4);

    /// <summary>A hardening dust burst's X across 128 pixels centred on the body: the sample modulo 128.</summary>
    internal static readonly RandomSampleField HardeningDustX = new(0, 7);

    /// <summary>A hardening dust burst's Y down 128 pixels centred on the body: the sample divided by 256, modulo 128.</summary>
    internal static readonly RandomSampleField HardeningDustY = new(8, 7);

    /// <summary>A dying explosion's X across 128 pixels centred on the body: the sample modulo 128.</summary>
    internal static readonly RandomSampleField DyingExplosionX = new(0, 7);

    /// <summary>A dying explosion's Y down 64 pixels centred on the body: the sample divided by 256, modulo 64.</summary>
    internal static readonly RandomSampleField DyingExplosionY = new(8, 6);

    /// <summary>A death drop's X across its 128-pixel strip from X 64: the sample modulo 128.</summary>
    internal static readonly RandomSampleField DropX = new(0, 7);

    /// <summary>A death drop's Y down its 64-pixel band from Y 528: the sample divided by 256, modulo 64.</summary>
    internal static readonly RandomSampleField DropY = new(8, 6);

    /// <summary>A spore spawner's wait before its next release, 0 through 511 updates: the sample modulo 512.</summary>
    internal static readonly RandomSampleField SporeReleaseWait = new(0, 9);
}
