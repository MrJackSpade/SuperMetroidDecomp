namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The random choices of Spore Spawn's fight and death: its dust, explosions, drops and spore releases.</summary>
internal static partial class SporeSpawnRandomDefinitions
{
    /// <summary>An idle dust cloud's X across its 64-pixel strip from X 96.</summary>
    internal static readonly RandomSampleField IdleDustX = RandomSampleField.OneOf(64);

    /// <summary>An idle dust cloud's Y down its 16-pixel band from Y 480.</summary>
    internal static readonly RandomSampleField IdleDustY = RandomSampleField.SecondOf(16);

    /// <summary>A hardening dust burst's X across 128 pixels centred on the body.</summary>
    internal static readonly RandomSampleField HardeningDustX = RandomSampleField.OneOf(128);

    /// <summary>A hardening dust burst's Y down 128 pixels centred on the body.</summary>
    internal static readonly RandomSampleField HardeningDustY = RandomSampleField.SecondOf(128);

    /// <summary>A dying explosion's X across 128 pixels centred on the body.</summary>
    internal static readonly RandomSampleField DyingExplosionX = RandomSampleField.OneOf(128);

    /// <summary>A dying explosion's Y down 64 pixels centred on the body.</summary>
    internal static readonly RandomSampleField DyingExplosionY = RandomSampleField.SecondOf(64);

    /// <summary>A death drop's X across its 128-pixel strip from X 64.</summary>
    internal static readonly RandomSampleField DropX = RandomSampleField.OneOf(128);

    /// <summary>A death drop's Y down its 64-pixel band from Y 528.</summary>
    internal static readonly RandomSampleField DropY = RandomSampleField.SecondOf(64);

    /// <summary>A spore spawner's wait before its next release, 0 through 511 updates.</summary>
    internal static readonly RandomSampleField SporeReleaseWait = RandomSampleField.OneOf(512);
}
