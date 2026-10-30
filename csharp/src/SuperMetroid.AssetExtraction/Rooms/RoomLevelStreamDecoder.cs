using System.Buffers.Binary;
using System.Collections.Immutable;
using SuperMetroid.Core;
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
    /// <exception cref="InvalidDataException">
    /// The level's size word, foreground, behaviours or background are malformed, or the cartridge's fill is not the
    /// game's edge-solid block.
    /// </exception>
    internal static InstalledRoomLevel Decode(RoomLevelId level, ReadOnlySpan<byte> stream, int widthInBlocks)
    {
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
            foreground.Add(RoomLevelWordCodec.Decode(space[index]));
            behaviors.Add(new RoomBlockBehavior(stream[behaviorOffset + index]));
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
