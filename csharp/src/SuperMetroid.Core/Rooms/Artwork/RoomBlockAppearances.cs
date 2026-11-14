using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The appearance inputs each block of one tileset's rooms depends on (#1466), as asset extraction recorded them when it
/// pictured the blocks: for every block, by its index in what a room of the tileset loads, whether some room of the tileset
/// shows it (#1636), and for each block shown, the inputs its picture changes with (an animation's frame, Kraid's backdrop,
/// Ceres Ridley's look in his arena, #1791). A room names the picture a block shows by the block and the values its inputs
/// take, so this is all about a block the room needs to choose its picture. A block no room of the tileset shows has no
/// picture and depends on nothing.
/// </summary>
public sealed class RoomBlockAppearances
{
    /// <summary>The inputs each block depends on, by block, none for a block no room shows.</summary>
    internal readonly ImmutableArray<AppearanceInput>[] blocks;

    /// <summary>Whether some room of the tileset shows each block, by block.</summary>
    private readonly bool[] shown;

    /// <summary>Keeps the inputs each block some room of the tileset shows depends on.</summary>
    /// <param name="blocks">Each block's inputs, by block index, empty for a block no room shows.</param>
    /// <param name="shownBlocks">
    /// The blocks some room of the tileset shows, which alone have pictures; none for a tileset whose rooms show only their
    /// turning floor, the Ceres elevator shaft's (#1647).
    /// </param>
    /// <exception cref="InvalidDataException">
    /// The tileset has no blocks, a block's inputs are unrecorded, a shown block lies outside them, or a block no room shows
    /// depends on an input.
    /// </exception>
    public RoomBlockAppearances(IReadOnlyList<ImmutableArray<AppearanceInput>> blocks, IEnumerable<int> shownBlocks)
    {
        this.blocks = [.. Ensure.NotNull(blocks)];
        if (this.blocks.Any(inputs => inputs.IsDefault))
            throw new InvalidDataException("A tileset's block appearances leave a block's inputs unrecorded.");
        if (this.blocks.Length == 0)
            throw new InvalidDataException("A tileset's block appearances record no blocks.");
        shown = new bool[this.blocks.Length];
        foreach (int block in Ensure.NotNull(shownBlocks))
        {
            if ((uint)block >= (uint)shown.Length)
                throw new InvalidDataException($"A tileset's block appearances show block {block}, not among its {shown.Length} blocks.");
            shown[block] = true;
        }
        for (int block = 0; block < shown.Length; block++)
        {
            if (!shown[block] && !this.blocks[block].IsEmpty)
                throw new InvalidDataException($"A tileset's block appearances give block {block}, which no room shows, inputs to depend on.");
        }
    }

    /// <summary>The blocks a room of the tileset loads, shown or not.</summary>
    public int Count => shown.Length;

    /// <summary>Whether some room of the tileset shows block <paramref name="block"/>, which alone then has pictures (#1636).</summary>
    /// <exception cref="InvalidDataException">The tileset loads no such block.</exception>
    public bool Shows(int block) =>
        (uint)block < (uint)shown.Length ? shown[block]
            : throw new InvalidDataException($"Block {block} is not among the tileset's {shown.Length} blocks.");

    /// <summary>The inputs block <paramref name="block"/>'s picture depends on.</summary>
    /// <exception cref="InvalidDataException">The tileset loads no such block, or no room of it shows the block.</exception>
    public ImmutableArray<AppearanceInput> Of(int block) =>
        Shows(block) ? blocks[block]
            : throw new InvalidDataException($"Block {block} is shown by no room of the tileset, which therefore has no picture of it.");
}
