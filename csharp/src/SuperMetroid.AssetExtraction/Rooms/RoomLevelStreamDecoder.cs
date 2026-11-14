using System.Buffers.Binary;
using System.Collections.Immutable;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Decodes one decompressed cartridge room level into typed terrain (#1469): the grid's terrain blocks, behaviours and
/// background appearances, and the terrain past the grid. The level is decoded once, at installation.
/// </summary>
/// <remarks>
/// The decompressed level is a size word (the grid's foreground bytes), the foreground level words, one behaviour byte
/// per block, then the background level words. The cartridge fills the whole level space with
/// <see cref="RoomLevelSpaceDefinitions.PrefillWord"/> before decompressing over its beginning, so background words the
/// level omits read as that fill, and the level space past the foreground grid holds the rest of the decompressed data,
/// read as level words, and then the fill.
/// </remarks>
internal static class RoomLevelStreamDecoder
{
    /// <summary>Bytes in the leading size word.</summary>
    private const int SizeWordBytes = sizeof(ushort);

    /// <summary>Decodes one level.</summary>
    /// <param name="level">The level, for messages.</param>
    /// <param name="stream">The complete decompressed level.</param>
    /// <param name="widthInBlocks">The width every room state using the level gives it.</param>
    /// <param name="areas">The areas of the rooms using the level, which give the area tables' reactions their meaning.</param>
    /// <exception cref="InvalidDataException">
    /// The level's size word, foreground, behaviours or background are malformed, a reacting block's behaviour gives its
    /// collision kind no reaction (its kind's dispatch would read past the end of its tables, which no retail block does,
    /// #1742) or different reactions in the level's areas (#1805), or the cartridge's fill is not the game's edge-solid block.
    /// </exception>
    internal static InstalledRoomLevel Decode(RoomLevelId level, ReadOnlySpan<byte> stream, int widthInBlocks,
        IReadOnlyCollection<AreaId> areas)
    {
        if (areas.Count == 0)
            throw new ArgumentException($"Room level {level} belongs to no area.", nameof(areas));
        if (stream.Length < SizeWordBytes)
            throw new InvalidDataException($"Room level {level} has no size word.");
        int layerBytes = BinaryPrimitives.ReadUInt16LittleEndian(stream);
        int blockCount = layerBytes / sizeof(ushort);
        int behaviorOffset = SizeWordBytes + layerBytes;
        int backgroundOffset = behaviorOffset + blockCount;
        if ((layerBytes & 1) != 0 || blockCount == 0 || blockCount % widthInBlocks != 0 || stream.Length < backgroundOffset)
            throw new InvalidDataException($"Room level {level} has an invalid foreground or behaviour layer.");
        int availableBackgroundBytes = Math.Min(layerBytes, stream.Length - backgroundOffset);
        if ((availableBackgroundBytes & 1) != 0)
            throw new InvalidDataException($"Room level {level} ends inside a background level word.");

        // The game fills the space past the installed data with its edge-solid block, so the cartridge's fill must be it.
        if (RoomLevelWordCodec.Decode(RoomLevelSpaceDefinitions.PrefillWord) != RoomTerrainBlocks.EdgeSolid)
            throw new InvalidDataException("The cartridge's level-space fill is not the game's edge-solid block.");
        ReadOnlySpan<byte> payload = stream[SizeWordBytes..];
        ushort[] space = LevelSpace(payload);
        var foreground = ImmutableArray.CreateBuilder<RoomTerrainBlock>(blockCount);
        var behaviors = ImmutableArray.CreateBuilder<RoomBlockBehavior>(blockCount);
        var background = ImmutableArray.CreateBuilder<RoomBlockAppearance>(blockCount);
        for (int index = 0; index < blockCount; index++)
        {
            RoomTerrainBlock block = RoomLevelWordCodec.Decode(space[index]);
            RoomBlockBehavior behavior = InstalledBehavior(level, index, block.Collision, stream[behaviorOffset + index], areas);
            foreground.Add(block);
            behaviors.Add(behavior);
            int backgroundByte = index * sizeof(ushort);
            background.Add(RoomLevelWordCodec.DecodeAppearance(backgroundByte < availableBackgroundBytes
                ? BinaryPrimitives.ReadUInt16LittleEndian(stream[(backgroundOffset + backgroundByte)..])
                : RoomLevelSpaceDefinitions.PrefillWord));
        }

        // The data past the grid is every level-space word the decompressed level reaches into, the last possibly half
        // decompressed and half fill; the rest of the level space is the fill alone, which the game reads as edge solid.
        int dataWords = (payload.Length + 1) / sizeof(ushort);
        var following = ImmutableArray.CreateBuilder<RoomTerrainBlock>(Math.Max(0, dataWords - blockCount));
        for (int index = blockCount; index < dataWords; index++)
            following.Add(RoomLevelWordCodec.Decode(space[index]));
        return new InstalledRoomLevel(
            new Extent16<RoomBlocks>(checked((ushort)widthInBlocks), checked((ushort)(blockCount / widthInBlocks))),
            foreground.MoveToImmutable(), behaviors.MoveToImmutable(), background.MoveToImmutable(),
            following.ToImmutable(), space.Length - blockCount);
    }

    /// <summary>
    /// A block's behaviour as the installation holds it (#1805). A block of a kind that reads no reaction holds the byte's
    /// behaviour with the areas' meanings left unresolved, which a room's load resolves in its area. A reacting block holds
    /// the reaction its kind reads in the level's areas, which must agree, as the first behaviour holding that reaction:
    /// where an area's table gives several entries one meaning (Maridia's quicksand surface), the block keeps the first
    /// entry's readings, which give every reacting kind what its own entry gives and differ only as a slope, extension or
    /// door, kinds nothing changes a reacting retail block to without writing its behaviour.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The block's kind has no reaction for the byte in an area, the areas give it different reactions, or the first
    /// behaviour holding its reaction gives another reacting kind something else.
    /// </exception>
    private static RoomBlockBehavior InstalledBehavior(RoomLevelId level, int index, RoomCollisionType kind, int value,
        IReadOnlyCollection<AreaId> areas)
    {
        if (BlockReactionFamilies.Of(kind) is not { } family)
            return RoomBlockBehaviorCodec.Decode(value);
        RoomBlockBehavior? installed = null;
        foreach (AreaId area in areas)
        {
            RoomBlockBehavior resolved = RoomBlockBehaviorCodec.Decode(area, value);
            Enum reaction = resolved.ReactionIn(family) ?? throw new InvalidDataException(
                $"Room level {level} block {index} is {kind} with behaviour {resolved}, which gives its kind no reaction in {area}.");
            RoomBlockBehavior first = RoomBlockBehavior.ReactingAs(reaction);
            if (!GiveSameReactions(first.In(area), resolved))
                throw new InvalidDataException(
                    $"Room level {level} block {index} is {kind} with behaviour {resolved}, whose reaction's first behaviour {first} reacts otherwise.");
            if (installed is { } earlier && earlier != first)
                throw new InvalidDataException(
                    $"Room level {level} block {index} is {kind} with a behaviour its areas give different reactions, {earlier} and {first}.");
            installed = first;
        }
        return installed ?? throw new InvalidOperationException($"Room level {level} belongs to no area.");
    }

    /// <summary>Whether two behaviours give every reacting kind the same reaction.</summary>
    private static bool GiveSameReactions(RoomBlockBehavior left, RoomBlockBehavior right) =>
        Enum.GetValues<BlockReactionFamily>().All(family => Equals(left.ReactionIn(family), right.ReactionIn(family)));

    /// <summary>
    /// The level space as level words after the fill and the decompressed level's overwrite: at least the filled words,
    /// and every word the decompressed level reaches.
    /// </summary>
    private static ushort[] LevelSpace(ReadOnlySpan<byte> payload)
    {
        int prefilledBytes = RoomLevelSpaceDefinitions.PrefilledWordCount * sizeof(ushort);
        int spaceBytes = Math.Max(prefilledBytes, checked((payload.Length + 1) & ~1));
        var bytes = new byte[spaceBytes];
        for (int index = 0; index < bytes.Length; index += sizeof(ushort))
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index), RoomLevelSpaceDefinitions.PrefillWord);
        payload.CopyTo(bytes);
        var words = new ushort[spaceBytes / sizeof(ushort)];
        for (int index = 0; index < words.Length; index++)
            words[index] = BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(index * sizeof(ushort)));
        return words;
    }
}
