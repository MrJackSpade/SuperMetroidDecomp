using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The appearance inputs each block of one tileset's rooms depends on (#1466), as asset extraction recorded them when it
/// pictured the blocks: for every block, by its index in what a room of the tileset loads, the inputs its picture changes
/// with over the tileset's artwork and, for a tileset whose rooms show the sprites' artwork, over that. A room names the
/// picture a block shows by the block and the values its inputs take, so this is all about a block the room needs to
/// choose its picture.
/// </summary>
public sealed class RoomBlockAppearances
{
    /// <summary>The inputs each block depends on over the tileset's artwork, by block.</summary>
    private readonly ImmutableArray<AppearanceInput>[] tileset;

    /// <summary>The inputs each block depends on over the sprites' artwork, by block; null where no room shows it.</summary>
    private readonly ImmutableArray<AppearanceInput>[]? sprites;

    /// <summary>Keeps the inputs each block depends on over each artwork the tileset's rooms' blocks show.</summary>
    /// <param name="tileset">Each block's inputs over the tileset's artwork, by block index.</param>
    /// <param name="sprites">Each block's inputs over the sprites' artwork, or null where no room of the tileset shows it.</param>
    /// <exception cref="InvalidDataException">The tileset has no blocks, or the two artworks describe different blocks.</exception>
    public RoomBlockAppearances(IReadOnlyList<ImmutableArray<AppearanceInput>> tileset, IReadOnlyList<ImmutableArray<AppearanceInput>>? sprites)
    {
        this.tileset = [.. Ensure.NotNull(tileset)];
        this.sprites = sprites is null ? null : [.. sprites];
        if (this.tileset.Any(inputs => inputs.IsDefault) || this.sprites?.Any(inputs => inputs.IsDefault) == true)
            throw new InvalidDataException("A tileset's block appearances leave a block's inputs unrecorded.");
        if (this.tileset.Length == 0)
            throw new InvalidDataException("A tileset's block appearances record no blocks.");
        if (this.sprites is { } shown && shown.Length != this.tileset.Length)
            throw new InvalidDataException($"A tileset's blocks over the sprites' artwork number {shown.Length}, not its {this.tileset.Length}.");
    }

    /// <summary>The blocks a room of the tileset loads.</summary>
    public int Count => tileset.Length;

    /// <summary>Whether a room of the tileset shows its blocks over the sprites' artwork.</summary>
    public bool ShowsSprites => sprites is not null;

    /// <summary>The inputs block <paramref name="block"/>'s picture depends on over <paramref name="artwork"/>.</summary>
    /// <exception cref="InvalidDataException">The tileset loads no such block, or no room of it shows that artwork.</exception>
    public ImmutableArray<AppearanceInput> Of(int block, RoomBlockArtwork artwork)
    {
        ImmutableArray<AppearanceInput>[] blocks = artwork switch
        {
            RoomBlockArtwork.Tileset => tileset,
            RoomBlockArtwork.Sprites => sprites ?? throw new InvalidDataException("No room of the tileset shows its blocks over the sprites' artwork."),
            _ => throw new InvalidDataException($"Undefined block artwork {artwork}."),
        };
        return (uint)block < (uint)blocks.Length ? blocks[block]
            : throw new InvalidDataException($"Block {block} is not among the tileset's {blocks.Length} blocks.");
    }

    /// <summary>Appends every block's inputs to a content identity.</summary>
    internal void AppendIdentity(SelectedPresentationHash content)
    {
        content.Append("blocks", tileset.Length);
        ImmutableArray<AppearanceInput>[][] artworks = sprites is null ? [tileset] : [tileset, sprites];
        foreach (ImmutableArray<AppearanceInput>[] artwork in artworks)
        {
            foreach (ImmutableArray<AppearanceInput> inputs in artwork)
            {
                content.Append("inputs", inputs.Length);
                foreach (AppearanceInput input in inputs)
                    content.AppendName("input", input.Token);
            }
        }
    }
}
