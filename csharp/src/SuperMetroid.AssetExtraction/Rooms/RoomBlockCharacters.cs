using System.Collections.Immutable;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The characters each block of one block set reads (#1431, #1466), as asset extraction composes the blocks' pictures: for
/// every block, by its index in what a room of the set loads, the runs of characters its picture reads, each shown by one
/// artwork. Extraction pictures each block over what each appearance input can show at its runs; the room names the
/// picture by the inputs alone.
/// </summary>
public sealed class RoomBlockCharacters
{
    /// <summary>The runs each block reads, by block.</summary>
    private readonly ImmutableArray<RoomBlockCharacterRun>[] blocks;

    /// <summary>Keeps the runs each block of <paramref name="set"/> reads.</summary>
    /// <param name="set">The block set whose blocks the runs describe.</param>
    /// <param name="blocks">Each block's runs, by block index: at least one, ascending and not overlapping.</param>
    /// <exception cref="InvalidDataException">The set has no blocks, or a block reads no run, an empty run or overlapping runs.</exception>
    public RoomBlockCharacters(RoomBlockSet set, IEnumerable<IEnumerable<RoomBlockCharacterRun>> blocks)
    {
        Set = set;
        this.blocks = [.. Ensure.NotNull(blocks).Select(runs => ImmutableArray.CreateRange(Ensure.NotNull(runs)))];
        if (this.blocks.Length == 0)
            throw new InvalidDataException($"Block set {set} records no blocks.");
        for (int block = 0; block < this.blocks.Length; block++)
        {
            ImmutableArray<RoomBlockCharacterRun> runs = this.blocks[block];
            if (runs.IsEmpty)
                throw new InvalidDataException($"Block {block} of {set} reads no characters.");
            for (int run = 0; run < runs.Length; run++)
            {
                if (runs[run].FirstCharacter < 0 || runs[run].CharacterCount <= 0)
                    throw new InvalidDataException($"Block {block} of {set} reads an empty run or one before the first character.");
                if (run > 0 && runs[run].FirstCharacter < runs[run - 1].FirstCharacter + runs[run - 1].CharacterCount)
                    throw new InvalidDataException($"Block {block} of {set} reads runs out of order or overlapping.");
            }
        }
    }

    /// <summary>The block set whose blocks the runs describe.</summary>
    public RoomBlockSet Set { get; }

    /// <summary>The blocks a room of the set loads.</summary>
    public int Count => blocks.Length;

    /// <summary>The runs of characters block <paramref name="block"/> reads, ascending.</summary>
    /// <exception cref="InvalidDataException">The set loads no such block.</exception>
    public ImmutableArray<RoomBlockCharacterRun> RunsOf(int block) =>
        (uint)block < (uint)blocks.Length ? blocks[block]
            : throw new InvalidDataException($"Block {block} is not among the {blocks.Length} blocks of {Set}.");
}
