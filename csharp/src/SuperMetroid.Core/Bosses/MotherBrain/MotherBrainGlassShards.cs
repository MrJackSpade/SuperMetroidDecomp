namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Places Mother Brain's glass shards at the emitter each one bursts from.</summary>
internal static partial class MotherBrainGlassShards
{
    /// <summary>
    /// One of the horizontal emitter positions on the glass: right, left or centre, all below the glass's origin block;
    /// the setup then applies an independent jitter on each axis.
    /// </summary>
    internal static MotherBrainGlassShardPlacement Placement(MotherBrainGlassShard shard) =>
        MotherBrainGlassShardDefinitions.EmitterXs.TryGetValue(shard, out int x)
            ? new(Offset16<RoomPixels>.Of(x, MotherBrainGlassShardDefinitions.EmitterY))
            : throw new ArgumentOutOfRangeException(nameof(shard), shard, "Undefined Mother Brain glass emitter.");
}
