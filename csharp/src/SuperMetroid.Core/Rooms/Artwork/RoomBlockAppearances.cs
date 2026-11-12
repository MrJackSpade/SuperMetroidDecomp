using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The appearance inputs each block of one tileset's rooms depends on (#1466), as asset extraction recorded them when it
/// pictured the blocks: for every block, by its index in what a room of the tileset loads, whether some room of the tileset
/// shows it (#1636), and for each block shown, the inputs its picture changes with over each artwork the tileset's rooms
/// show their blocks over: the tileset's own, the sprites', or both (#1648). A room names the picture a block shows by the
/// block and the values its inputs take, so this is all about a block the room needs to choose its picture. A block no room
/// of the tileset shows has no picture and depends on nothing, and no block has a picture over an artwork no room of the
/// tileset shows its blocks over.
/// </summary>
public sealed class RoomBlockAppearances
{
    /// <summary>
    /// The inputs each block depends on over the tileset's artwork, by block, none for a block no room shows; null where no
    /// room shows that artwork, as in Ceres Ridley's arena, whose setup points its blocks at the sprites' artwork before its
    /// first frame (#1648).
    /// </summary>
    internal readonly ImmutableArray<AppearanceInput>[]? tileset;

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
    /// <param name="tileset">
    /// Each block's inputs over the tileset's artwork, by block index, empty for a block no room shows; or null where no room
    /// of the tileset shows that artwork.
    /// </param>
    /// <param name="sprites">
    /// Each block's inputs over the sprites' artwork, by block index, empty for a block no room shows; or null where no room
    /// of the tileset shows that artwork.
    /// </param>
    /// <param name="shownBlocks">
    /// The blocks some room of the tileset shows, which alone have pictures; none for a tileset whose rooms show only their
    /// turning floor, the Ceres elevator shaft's (#1647).
    /// </param>
    /// <exception cref="InvalidDataException">
    /// The tileset's rooms show their blocks over no artwork, the tileset has no blocks, the two artworks describe different
    /// blocks, a shown block lies outside them, or a block no room shows depends on an input.
    /// </exception>
    public RoomBlockAppearances(IReadOnlyList<ImmutableArray<AppearanceInput>>? tileset, IReadOnlyList<ImmutableArray<AppearanceInput>>? sprites,
        IEnumerable<int> shownBlocks)
    {
        this.tileset = tileset is null ? null : [.. tileset];
        this.sprites = sprites is null ? null : [.. sprites];
        ImmutableArray<AppearanceInput>[] recorded = this.tileset ?? this.sprites
            ?? throw new InvalidDataException("A tileset's block appearances show its blocks over no artwork.");
        if (this.tileset?.Any(inputs => inputs.IsDefault) == true || this.sprites?.Any(inputs => inputs.IsDefault) == true)
            throw new InvalidDataException("A tileset's block appearances leave a block's inputs unrecorded.");
        if (recorded.Length == 0)
            throw new InvalidDataException("A tileset's block appearances record no blocks.");
        if (this.sprites is { } overSprites && overSprites.Length != recorded.Length)
            throw new InvalidDataException($"A tileset's blocks over the sprites' artwork number {overSprites.Length}, not its {recorded.Length}.");
        shown = new bool[recorded.Length];
        foreach (int block in Ensure.NotNull(shownBlocks))
        {
            if ((uint)block >= (uint)shown.Length)
                throw new InvalidDataException($"A tileset's block appearances show block {block}, not among its {shown.Length} blocks.");
            shown[block] = true;
        }
        for (int block = 0; block < shown.Length; block++)
        {
            if (!shown[block] && (this.tileset?[block].IsEmpty == false || this.sprites?[block].IsEmpty == false))
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

    /// <summary>
    /// Whether some room of the tileset shows its blocks over <paramref name="artwork"/>, over which alone they then have
    /// pictures (#1648).
    /// </summary>
    /// <exception cref="InvalidDataException">The artwork is undefined.</exception>
    public bool ShowsOver(RoomBlockArtwork artwork) => artwork switch
    {
        RoomBlockArtwork.Tileset => tileset is not null,
        RoomBlockArtwork.Sprites => sprites is not null,
        _ => throw new InvalidDataException($"Undefined block artwork {artwork}."),
    };

    /// <summary>The inputs block <paramref name="block"/>'s picture depends on over <paramref name="artwork"/>.</summary>
    /// <exception cref="InvalidDataException">
    /// The tileset loads no such block, no room of it shows the block, or no room of it shows its blocks over that artwork.
    /// </exception>
    public ImmutableArray<AppearanceInput> Of(int block, RoomBlockArtwork artwork)
    {
        ImmutableArray<AppearanceInput>[] blocks = artwork switch
        {
            RoomBlockArtwork.Tileset => tileset ?? throw new InvalidDataException("No room of the tileset shows its blocks over the tileset's artwork."),
            RoomBlockArtwork.Sprites => sprites ?? throw new InvalidDataException("No room of the tileset shows its blocks over the sprites' artwork."),
            _ => throw new InvalidDataException($"Undefined block artwork {artwork}."),
        };
        return Shows(block) ? blocks[block]
            : throw new InvalidDataException($"Block {block} is shown by no room of the tileset, which therefore has no picture of it.");
    }
}
