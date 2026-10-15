namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One of the glass programs' cues (#1296): its kind, the hits a stage waits for, and a burst's four shards.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Hits">The hits <see cref="MotherBrainGlassPlmCueKind.RepeatUntilHits"/> waits for.</param>
/// <param name="Shard1">A burst's first shard.</param>
/// <param name="Shard2">A burst's second shard.</param>
/// <param name="Shard3">A burst's third shard.</param>
/// <param name="Shard4">A burst's fourth shard.</param>
internal readonly record struct MotherBrainGlassPlmCue(
    MotherBrainGlassPlmCueKind Kind,
    ushort Hits = 0,
    MotherBrainGlassShard Shard1 = default,
    MotherBrainGlassShard Shard2 = default,
    MotherBrainGlassShard Shard3 = default,
    MotherBrainGlassShard Shard4 = default)
{
    /// <summary>A cue of <paramref name="kind"/> without operands.</summary>
    internal static MotherBrainGlassPlmCue Of(MotherBrainGlassPlmCueKind kind) => new(kind);

    /// <summary>Repeats the stage until the glass has taken <paramref name="hits"/> hits.</summary>
    internal static MotherBrainGlassPlmCue RepeatUntil(ushort hits) => new(MotherBrainGlassPlmCueKind.RepeatUntilHits, Hits: hits);

    /// <summary>A burst of four shards.</summary>
    internal static MotherBrainGlassPlmCue Burst(MotherBrainGlassShard a, MotherBrainGlassShard b, MotherBrainGlassShard c, MotherBrainGlassShard d) =>
        new(MotherBrainGlassPlmCueKind.SpawnShards, Shard1: a, Shard2: b, Shard3: c, Shard4: d);

    /// <summary>A burst's four shards, in spawn order.</summary>
    internal MotherBrainGlassShard[] Shards => [Shard1, Shard2, Shard3, Shard4];
}
