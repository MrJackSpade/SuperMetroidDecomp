using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>
/// Checks the block-parameter carry-over (#1722) exactly, for every pair of readings: a behaviour written as one collision
/// kind's reading, read under every other kind, is what the import codec decodes from the byte the game writes for that
/// reading (the first byte holding it), and every byte the codec decodes is one of the kept behaviours with its own
/// readings.
/// </summary>
internal static class BlockParameterCarryOverVerification
{
    /// <summary>Every reading a behaviour can be written as, by name, with the reading it takes of a behaviour.</summary>
    private static readonly (string Name, Func<RoomBlockBehavior, object> Read, Func<object, RoomBlockBehavior> Write)[] Readings =
    [
        (nameof(RoomBlockBehavior.Shootable), behavior => behavior.Shootable, reading => RoomBlockBehavior.Reacting((ShootableReaction)reading)),
        (nameof(RoomBlockBehavior.Bombable), behavior => behavior.Bombable, reading => RoomBlockBehavior.Reacting((BombableReaction)reading)),
        (nameof(RoomBlockBehavior.Special), behavior => behavior.Special, reading => RoomBlockBehavior.Reacting((SpecialBlockReaction)reading)),
        (nameof(RoomBlockBehavior.SpecialAir), behavior => behavior.SpecialAir, reading => RoomBlockBehavior.Reacting((SpecialAirReaction)reading)),
        (nameof(RoomBlockBehavior.Spike), behavior => behavior.Spike, reading => RoomBlockBehavior.Reacting((SpikeReaction)reading)),
        (nameof(RoomBlockBehavior.Grapple), behavior => behavior.Grapple, reading => RoomBlockBehavior.Reacting((GrappleReaction)reading)),
        (nameof(RoomBlockBehavior.Slope), behavior => behavior.Slope, reading => RoomBlockBehavior.Sloping((SlopeGeometry)reading)),
        (nameof(RoomBlockBehavior.ExtensionOffset), behavior => behavior.ExtensionOffset, reading => RoomBlockBehavior.Extending((int)reading)),
        (nameof(RoomBlockBehavior.DoorLink), behavior => behavior.DoorLink, reading => RoomBlockBehavior.LinkingDoor((int)reading)),
    ];

    /// <summary>Compares every written reading's every other reading with the codec's decoding of the game's byte.</summary>
    internal static void VerifyBlockParameterCarryOver()
    {
        var decoded = new RoomBlockBehavior[RoomBlockBehaviorCodec.ByteCount];
        for (int value = 0; value < decoded.Length; value++)
        {
            decoded[value] = RoomBlockBehaviorCodec.Decode(value);
            AssertEqual(RoomBlockBehaviorCodec.ReadingsOf(value), decoded[value].AllReadings, $"BTS ${value:X2} is the kept behaviour of its readings");
            AssertEqual(value, RoomBlockBehaviorCodec.Encode(decoded[value]), $"BTS ${value:X2} encodes back to itself");
        }
        AssertEqual(decoded.Length, decoded.Distinct().Count(), "every byte is its own kept behaviour");
        AssertEqual(decoded[0], RoomBlockBehavior.None, "the empty behaviour is the zero byte's");
        AssertEqual(decoded[0], default, "a block's default behaviour is the empty one");

        int pairs = 0;
        foreach ((string writtenName, Func<RoomBlockBehavior, object> written, Func<object, RoomBlockBehavior> write) in Readings)
        {
            // The game writes each reading as the first byte holding it.
            var firsts = new Dictionary<object, int>();
            for (int value = 0; value < decoded.Length; value++)
                firsts.TryAdd(written(decoded[value]), value);
            foreach ((object reading, int first) in firsts)
            {
                RoomBlockBehavior carried = write(reading);
                foreach ((string readName, Func<RoomBlockBehavior, object> read, _) in Readings)
                {
                    AssertEqual(read(decoded[first]), read(carried),
                        $"{writtenName} {reading} written, read as {readName}: the byte ${first:X2}'s reading");
                    pairs++;
                }
                AssertEqual(decoded[first], carried, $"{writtenName} {reading} keeps the byte ${first:X2}");
            }
        }

        // Every reaction a family names is held by some behaviour, so the installation can name each one.
        foreach (BlockReactionFamily family in Enum.GetValues<BlockReactionFamily>())
        foreach (Enum reaction in BlockReactionFamilies.Reactions(family))
            AssertTrue(decoded.Any(behavior => behavior.ReactionIn(family).Equals(reaction)),
                $"{family} reaction {reaction} is held by a byte");

        Console.WriteLine($"Block-parameter carry-over: {decoded.Length} kept behaviours; {pairs} written-reading and read-reading pairs match the codec.");
    }
}
