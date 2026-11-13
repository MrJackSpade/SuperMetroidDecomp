using System.Buffers.Binary;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>
/// Checks that the level import refuses a block whose behaviour gives its collision kind no reaction (#1742): such a block
/// would make its kind's dispatch read past the end of its tables, and no retail block does, so the game holds no
/// reaction for it.
/// </summary>
internal static class LevelBlockReactionImportVerification
{
    /// <summary>
    /// Decodes a one-block level for every reacting collision kind and every behaviour byte: the decode keeps the block
    /// exactly when its kind has a reaction for the byte, and otherwise refuses the level; a kind that reads no reaction
    /// keeps every byte.
    /// </summary>
    internal static void VerifyLevelImportRefusesBlocksWithoutReaction()
    {
        int refused = 0;
        foreach (RoomCollisionType kind in Enum.GetValues<RoomCollisionType>())
        for (int value = 0; value < RoomBlockBehaviorCodec.ByteCount; value++)
        {
            RoomBlockBehavior behavior = RoomBlockBehaviorCodec.Decode(value);
            bool reacts = BlockReactionFamilies.Of(kind) is not { } family || behavior.ReactionIn(family) is not null;
            byte[] stream = OneBlockLevel(RoomTerrainBlock.Of(0, BlockMirroring.None, kind), value);
            if (reacts)
            {
                InstalledRoomLevel level = RoomLevelStreamDecoder.Decode(new RoomLevelId("OneBlock"), stream, 1);
                AssertEqual(behavior, level.Behaviors[0], $"{kind} block with BTS ${value:X2} keeps its behaviour");
                continue;
            }
            AssertThrows<InvalidDataException>(() => RoomLevelStreamDecoder.Decode(new RoomLevelId("OneBlock"), stream, 1),
                $"{kind} block with BTS ${value:X2}, which gives its kind no reaction, is refused");
            refused++;
        }
        AssertTrue(refused > 0, "some behaviours give a reacting kind no reaction");
        Console.WriteLine($"Level block reaction import: {refused} kind and behaviour pairs with no reaction are refused.");
    }

    /// <summary>A decompressed level of one block: its size word, its level word and its behaviour byte.</summary>
    private static byte[] OneBlockLevel(RoomTerrainBlock block, int behavior)
    {
        var stream = new byte[sizeof(ushort) + sizeof(ushort) + 1];
        BinaryPrimitives.WriteUInt16LittleEndian(stream, sizeof(ushort));
        BinaryPrimitives.WriteUInt16LittleEndian(stream.AsSpan(sizeof(ushort)), RoomLevelWordEncoder.Encode(block));
        stream[^1] = checked((byte)behavior);
        return stream;
    }
}
