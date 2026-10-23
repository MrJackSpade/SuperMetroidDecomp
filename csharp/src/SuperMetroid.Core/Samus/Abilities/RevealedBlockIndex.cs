using System.Buffers.Binary;
using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Every way the X-ray reveal lays out a room's block definitions, by the four tile words it writes for each (#1392): a
/// copied block as the level shows it, a reveal metatile as defined, an overlay with its rows swapped. A buffer's block is
/// found from its words; definitions laid out to the same words show the same picture.
/// </summary>
public sealed class RevealedBlockIndex
{
    /// <summary>The layouts by their four tile words.</summary>
    private readonly Dictionary<ExpandedBlockTiles, RevealedBlock> layouts = [];

    /// <summary>Lays out each of <paramref name="definitions"/> every way, the layouts as defined taking precedence.</summary>
    public RevealedBlockIndex(ImmutableArray<byte> definitions)
    {
        Definitions = definitions;
        int count = definitions.Length / RoomLevelPlane.DefinitionBytes;
        foreach (RevealedBlockArrangement arrangement in Enum.GetValues<RevealedBlockArrangement>())
        {
            for (int definition = 0; definition < count; definition++)
                layouts.TryAdd(Words(definitions.AsSpan(), definition, arrangement), new RevealedBlock(definition, arrangement));
        }
    }

    /// <summary>The definitions laid out.</summary>
    public ImmutableArray<byte> Definitions { get; }

    /// <summary>
    /// The block whose layout writes the four tile words, top left to bottom right; null for a block the copy took from
    /// beyond the room, where BG1 shows nothing and the copy writes zero words that lay out no definition.
    /// </summary>
    /// <exception cref="InvalidDataException">No definition laid out any of the reveal's ways writes those words.</exception>
    public RevealedBlock? Find(ushort topLeft, ushort topRight, ushort bottomLeft, ushort bottomRight)
    {
        if (layouts.TryGetValue(new ExpandedBlockTiles(new(topLeft), new(topRight), new(bottomLeft), new(bottomRight)), out RevealedBlock block))
            return block;
        if ((topLeft | topRight | bottomLeft | bottomRight) == 0)
            return null;
        throw new InvalidDataException(
            $"The X-ray reveal writes ${topLeft:X4} ${topRight:X4} ${bottomLeft:X4} ${bottomRight:X4}, which lays out no block definition.");
    }

    /// <summary>The four tile words <paramref name="arrangement"/> lays definition <paramref name="definition"/> out as.</summary>
    private static ExpandedBlockTiles Words(ReadOnlySpan<byte> definitions, int definition, RevealedBlockArrangement arrangement)
    {
        if (arrangement == RevealedBlockArrangement.RowsSwapped)
        {
            // $91:D0A6 swaps the two tile rows without toggling any tile's flips.
            ReadOnlySpan<byte> words = definitions.Slice(definition * RoomLevelPlane.DefinitionBytes, RoomLevelPlane.DefinitionBytes);
            return new(new(BinaryPrimitives.ReadUInt16LittleEndian(words[4..])), new(BinaryPrimitives.ReadUInt16LittleEndian(words[6..])),
                new(BinaryPrimitives.ReadUInt16LittleEndian(words)), new(BinaryPrimitives.ReadUInt16LittleEndian(words[2..])));
        }
        LevelBlockFlipFlags flips = arrangement switch
        {
            RevealedBlockArrangement.AsDefined => LevelBlockFlipFlags.None,
            RevealedBlockArrangement.MirroredHorizontally => LevelBlockFlipFlags.Horizontal,
            RevealedBlockArrangement.MirroredVertically => LevelBlockFlipFlags.Vertical,
            RevealedBlockArrangement.MirroredBoth => LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical,
            RevealedBlockArrangement.RowsSwapped => throw new InvalidOperationException("Swapped rows were laid out above."),
            _ => throw new InvalidDataException($"Unknown revealed block arrangement {arrangement}."),
        };
        return LevelBlockTilemapExpander.Expand(RoomLevelWord.Create(checked((ushort)definition), flips, RoomCollisionType.Air), definitions);
    }
}
