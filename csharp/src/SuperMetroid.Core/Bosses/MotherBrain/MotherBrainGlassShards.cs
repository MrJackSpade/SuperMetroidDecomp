namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Places Mother Brain's glass shards at the emitter each one bursts from.</summary>
internal static class MotherBrainGlassShards
{
    /// <summary>
    /// One of three horizontal emitter positions on the glass: right (+8), left (-40), or centre (-16), all two blocks
    /// below the PLM origin. These signed offsets are added after scaling block coordinates by 16; the initializer then
    /// applies independent -8..7 jitter on each axis.
    /// </summary>
    internal static MotherBrainGlassShardPlacement Placement(MotherBrainGlassShard shard) =>
        MotherBrainGlassShardDefinitions.EmitterXs.TryGetValue(shard, out int x)
            ? new(Offset16<RoomPixels>.Of(x, MotherBrainGlassShardDefinitions.EmitterY))
            : throw new ArgumentOutOfRangeException(nameof(shard), shard, "Undefined Mother Brain glass emitter.");
}
