using System.Buffers.Binary;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>
/// Checks that the level import refuses a block whose behaviour gives its collision kind no reaction (#1742): such a block
/// would make its kind's dispatch read past the end of its tables, and no retail block does, so the game holds no
/// reaction for it. A block the import keeps holds, once its room's area resolves it, what every reacting kind reads of
/// its byte in that area (#1805).
/// </summary>
internal static class LevelBlockReactionImportVerification
{
    /// <summary>The first behaviour byte of the area tables.</summary>
    private const int AreaTableBehaviors = 0x80;

    /// <summary>The area tables' entries whose meanings differ between areas.</summary>
    private const int AreaMeanings = 16;

    /// <summary>
    /// Decodes levels of every area for every collision kind and every behaviour byte: a reacting kind's block with no
    /// reaction for the byte, or whose reaction's first behaviour reacts otherwise, is refused; a kept block, resolved in its
    /// area, gives every reacting kind the reaction the byte gives it there, and a block of a kind that reads no reaction
    /// keeps every reading of its byte. Only the area tables' first sixteen entries mean different things in different
    /// areas, so every other byte is decoded in one area. The kept blocks of a kind and area share one level; a refusal is
    /// checked for one kind of each family, as every kind of a family reads the same reaction.
    /// </summary>
    internal static void VerifyLevelImportRefusesBlocksWithoutReaction()
    {
        int refused = 0;
        int kept = 0;
        foreach (AreaId area in AreaIds.All)
        foreach (RoomCollisionType kind in Enum.GetValues<RoomCollisionType>())
        {
            BlockReactionFamily? family = BlockReactionFamilies.Of(kind);
            bool checksRefusals = family is { } read &&
                Enum.GetValues<RoomCollisionType>().First(other => BlockReactionFamilies.Of(other) == read) == kind;
            var keptValues = new List<int>();
            for (int value = 0; value < RoomBlockBehaviorCodec.ByteCount; value++)
            {
                if (area != AreaId.Crateria && value is < AreaTableBehaviors or >= AreaTableBehaviors + AreaMeanings)
                    continue;
                RoomBlockBehavior behavior = RoomBlockBehaviorCodec.Decode(area, value);
                BlockReaction? reaction = family is { } reacting ? behavior.ReactionIn(reacting) : null;
                // A reacting block keeps the first behaviour holding its reaction, which must react as its own byte does.
                bool refusedHere = family is not null && (reaction is null ||
                    !Enum.GetValues<BlockReactionFamily>().All(other =>
                        Equals(reaction.Behavior.In(area).ReactionIn(other), behavior.ReactionIn(other))));
                if (!refusedHere)
                {
                    keptValues.Add(value);
                    continue;
                }
                if (!checksRefusals)
                    continue;
                AssertThrows<InvalidDataException>(
                    () => RoomLevelStreamDecoder.Decode(new RoomLevelId("OneBlock"), Level(kind, [value]), 1, [area]),
                    $"{kind} block with BTS ${value:X2} in {area}, which gives its kind no reaction or reacts unlike its reaction's first behaviour, is refused");
                refused++;
            }
            if (keptValues.Count == 0)
                continue;
            InstalledRoomLevel level = RoomLevelStreamDecoder.Decode(new RoomLevelId("KeptBlocks"), Level(kind, keptValues),
                keptValues.Count, [area]);
            IReadOnlyDictionary<Position16<RoomBlocks>, int> links = RoomLevelStreamDecoder.DoorLinks(new RoomLevelId("KeptBlocks"),
                Level(kind, keptValues), keptValues.Count);
            for (int index = 0; index < keptValues.Count; index++)
            {
                RoomBlockBehavior behavior = RoomBlockBehaviorCodec.Decode(area, keptValues[index]);
                RoomBlockBehavior installed = level.Behaviors[index].In(area);
                if (kind == RoomCollisionType.DoorBlock)
                {
                    // #2090: a door block holds no behaviour; the low seven bits of its byte are where in its room's door
                    // list it leads, which the import resolves through each room's list.
                    AssertEqual(RoomBlockBehavior.None, installed, $"door block with BTS ${keptValues[index]:X2} in {area} holds no behaviour");
                    AssertEqual(keptValues[index] & 0x7f, links[new Position16<RoomBlocks>(index, 0)],
                        $"door block with BTS ${keptValues[index]:X2} in {area} leads through its byte's door-list position");
                    kept++;
                    continue;
                }
                AssertEqual(0, links.Count, $"a {kind} block leads through no door");
                if (family is null)
                    AssertEqual(behavior, installed, $"{kind} block with BTS ${keptValues[index]:X2} in {area} keeps its behaviour");
                foreach (BlockReactionFamily other in Enum.GetValues<BlockReactionFamily>())
                    AssertEqual(behavior.ReactionIn(other), installed.ReactionIn(other),
                        $"{kind} block with BTS ${keptValues[index]:X2} in {area} keeps its {other} reaction");
                kept++;
            }
        }
        AssertTrue(refused > 0, "some behaviours give a reacting kind no reaction");
        Console.WriteLine($"Level block reaction import: {kept} kind, behaviour and area triples are kept and {refused} refused.");
    }

    /// <summary>A decompressed level of one row of <paramref name="kind"/> blocks: its size word, its level words and its behaviour bytes.</summary>
    private static byte[] Level(RoomCollisionType kind, List<int> behaviors)
    {
        int wordBytes = behaviors.Count * sizeof(ushort);
        var stream = new byte[sizeof(ushort) + wordBytes + behaviors.Count];
        BinaryPrimitives.WriteUInt16LittleEndian(stream, checked((ushort)wordBytes));
        ushort word = RoomLevelWordEncoder.Encode(RoomTerrainBlock.Of(0, BlockMirroring.None, kind));
        for (int index = 0; index < behaviors.Count; index++)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(stream.AsSpan(sizeof(ushort) + index * sizeof(ushort)), word);
            stream[sizeof(ushort) + wordBytes + index] = checked((byte)behaviors[index]);
        }
        return stream;
    }
}
