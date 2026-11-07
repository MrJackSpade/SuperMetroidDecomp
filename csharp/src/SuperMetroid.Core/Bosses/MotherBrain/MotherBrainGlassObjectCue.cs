using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One of the glass programs' cues (#1296).</summary>
internal abstract record MotherBrainGlassObjectCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainGlassObjectCue()
    {
    }

    /// <summary>Repeats the current stage while the glass has taken fewer than <see cref="Hits"/> hits.</summary>
    /// <param name="Hits">The hits the stage waits for.</param>
    internal sealed record RepeatUntilHits(int Hits) : MotherBrainGlassObjectCue;

    /// <summary>Spawns four shard projectiles and queues the shatter sound.</summary>
    /// <param name="Shards">The burst's four shards, in spawn order.</param>
    internal sealed record SpawnShards(ImmutableArray<MotherBrainGlassShard> Shards) : MotherBrainGlassObjectCue
    {
        /// <summary>Two bursts are equal when their shards match in spawn order.</summary>
        public bool Equals(SpawnShards? other) => other is not null && Shards.SequenceEqual(other.Shards);

        /// <summary>Combines every shard in spawn order.</summary>
        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (MotherBrainGlassShard shard in Shards)
                hash.Add(shard);
            return hash.ToHashCode();
        }
    }

    /// <summary>A cue that carries no operand.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Command(MotherBrainGlassObjectCueKind Kind) : MotherBrainGlassObjectCue;

    /// <summary>A cue of <paramref name="kind"/> without operands.</summary>
    internal static MotherBrainGlassObjectCue Of(MotherBrainGlassObjectCueKind kind) => new Command(kind);

    /// <summary>Repeats the stage until the glass has taken <paramref name="hits"/> hits.</summary>
    internal static MotherBrainGlassObjectCue RepeatUntil(int hits) => new RepeatUntilHits(hits);

    /// <summary>A burst of four shards.</summary>
    internal static MotherBrainGlassObjectCue Burst(MotherBrainGlassShard a, MotherBrainGlassShard b, MotherBrainGlassShard c, MotherBrainGlassShard d) =>
        new SpawnShards([a, b, c, d]);
}
