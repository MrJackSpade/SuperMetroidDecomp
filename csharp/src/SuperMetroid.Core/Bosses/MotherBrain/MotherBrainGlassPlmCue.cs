using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One of the glass programs' cues (#1296): its kind, the hits a stage waits for, and a burst's four shards.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Hits">The hits <see cref="MotherBrainGlassPlmCueKind.RepeatUntilHits"/> waits for.</param>
/// <param name="Shards">A burst's four shards, in spawn order.</param>
internal readonly record struct MotherBrainGlassPlmCue(
    MotherBrainGlassPlmCueKind Kind,
    int Hits = 0,
    ImmutableArray<MotherBrainGlassShard> Shards = default)
{
    /// <summary>A cue of <paramref name="kind"/> without operands.</summary>
    internal static MotherBrainGlassPlmCue Of(MotherBrainGlassPlmCueKind kind) => new(kind);

    /// <summary>Repeats the stage until the glass has taken <paramref name="hits"/> hits.</summary>
    internal static MotherBrainGlassPlmCue RepeatUntil(int hits) => new(MotherBrainGlassPlmCueKind.RepeatUntilHits, Hits: hits);

    /// <summary>A burst of four shards.</summary>
    internal static MotherBrainGlassPlmCue Burst(MotherBrainGlassShard a, MotherBrainGlassShard b, MotherBrainGlassShard c, MotherBrainGlassShard d) =>
        new(MotherBrainGlassPlmCueKind.SpawnShards, Shards: [a, b, c, d]);

    /// <summary>Two cues are equal when their kind, hits and shards in spawn order match.</summary>
    public bool Equals(MotherBrainGlassPlmCue other) =>
        Kind == other.Kind && Hits == other.Hits && Shards.IsDefault == other.Shards.IsDefault &&
        (Shards.IsDefault || Shards.SequenceEqual(other.Shards));

    /// <summary>Combines the kind, hits and every shard in spawn order.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Kind);
        hash.Add(Hits);
        if (!Shards.IsDefault)
            foreach (MotherBrainGlassShard shard in Shards) hash.Add(shard);
        return hash.ToHashCode();
    }
}
