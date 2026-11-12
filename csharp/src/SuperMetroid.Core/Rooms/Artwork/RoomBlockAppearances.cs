using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The appearance inputs each block of one tileset's rooms depends on (#1466), as asset extraction recorded them when it
/// pictured the blocks: for every block, by its index in what a room of the tileset loads, whether some room of the tileset
/// shows it (#1636), and for each block shown, the inputs its picture changes with over the tileset's artwork and, for a
/// tileset whose rooms show the sprites' artwork, over that. A room names the picture a block shows by the block and the
/// values its inputs take, so this is all about a block the room needs to choose its picture. A block no room of the
/// tileset shows has no picture and depends on nothing.
/// </summary>
public sealed class RoomBlockAppearances
{
    /// <summary>The inputs each block depends on over the tileset's artwork, by block; none for a block no room shows.</summary>
    internal readonly ImmutableArray<AppearanceInput>[] tileset;

    /// <summary>
    /// The inputs each block depends on over the sprites' artwork, by block, none for a block no room shows; null where no
    /// room shows that artwork.
    /// </summary>
    internal readonly ImmutableArray<AppearanceInput>[]? sprites;

    /// <summary>Whether some room of the tileset shows each block, by block.</summary>
    private readonly bool[] shown;

    /// <summary>
    /// Keeps the inputs each block some room of the tileset shows depends on over each artwork the tileset's rooms' blocks
    /// show.
    /// </summary>
    /// <param name="tileset">Each block's inputs over the tileset's artwork, by block index; empty for a block no room shows.</param>
    /// <param name="sprites">
    /// Each block's inputs over the sprites' artwork, by block index, empty for a block no room shows; or null where no room
    /// of the tileset shows that artwork.
    /// </param>
    /// <param name="shownBlocks">The blocks some room of the tileset shows, which alone have pictures.</param>
    /// <exception cref="InvalidDataException">
    /// The tileset has no blocks or shows none, the two artworks describe different blocks, a shown block lies outside them,
    /// or a block no room shows depends on an input.
    /// </exception>
    public RoomBlockAppearances(IReadOnlyList<ImmutableArray<AppearanceInput>> tileset, IReadOnlyList<ImmutableArray<AppearanceInput>>? sprites,
        IEnumerable<int> shownBlocks)
    {
        this.tileset = [.. Ensure.NotNull(tileset)];
        this.sprites = sprites is null ? null : [.. sprites];
        if (this.tileset.Any(inputs => inputs.IsDefault) || this.sprites?.Any(inputs => inputs.IsDefault) == true)
            throw new InvalidDataException("A tileset's block appearances leave a block's inputs unrecorded.");
        if (this.tileset.Length == 0)
            throw new InvalidDataException("A tileset's block appearances record no blocks.");
        if (this.sprites is { } overSprites && overSprites.Length != this.tileset.Length)
            throw new InvalidDataException($"A tileset's blocks over the sprites' artwork number {overSprites.Length}, not its {this.tileset.Length}.");
        shown = new bool[this.tileset.Length];
        foreach (int block in Ensure.NotNull(shownBlocks))
        {
            if ((uint)block >= (uint)shown.Length)
                throw new InvalidDataException($"A tileset's block appearances show block {block}, not among its {shown.Length} blocks.");
            shown[block] = true;
        }
        if (!shown.Contains(true))
            throw new InvalidDataException("A tileset's block appearances show no block.");
        for (int block = 0; block < shown.Length; block++)
        {
            if (!shown[block] && (!this.tileset[block].IsEmpty || this.sprites?[block].IsEmpty == false))
                throw new InvalidDataException($"A tileset's block appearances give block {block}, which no room shows, inputs to depend on.");
        }
    }

    /// <summary>The blocks a room of the tileset loads, shown or not.</summary>
    public int Count => tileset.Length;

    /// <summary>Whether some room of the tileset shows block <paramref name="block"/>, which alone then has pictures (#1636).</summary>
    /// <exception cref="InvalidDataException">The tileset loads no such block.</exception>
    public bool Shows(int block) =>
        (uint)block < (uint)shown.Length ? shown[block]
            : throw new InvalidDataException($"Block {block} is not among the tileset's {shown.Length} blocks.");

    /// <summary>The inputs block <paramref name="block"/>'s picture depends on over <paramref name="artwork"/>.</summary>
    /// <exception cref="InvalidDataException">
    /// The tileset loads no such block, no room of it shows the block, or no room of it shows that artwork.
    /// </exception>
    public ImmutableArray<AppearanceInput> Of(int block, RoomBlockArtwork artwork)
    {
        ImmutableArray<AppearanceInput>[] blocks = artwork switch
        {
            RoomBlockArtwork.Tileset => tileset,
            RoomBlockArtwork.Sprites => sprites ?? throw new InvalidDataException("No room of the tileset shows its blocks over the sprites' artwork."),
            _ => throw new InvalidDataException($"Undefined block artwork {artwork}."),
        };
        return Shows(block) ? blocks[block]
            : throw new InvalidDataException($"Block {block} is shown by no room of the tileset, which therefore has no picture of it.");
    }
}
