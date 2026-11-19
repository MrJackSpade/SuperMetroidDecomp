using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The blocks the X-ray reveals over the screen (#1431, #1514, #2444), each place named by its column and row counted from
/// the reveal's first block, the room block the camera's top-left corner lies in: the rows of blocks the gameplay area
/// shows, and the screen's columns with the one a scroll within a block also shows. A place holds the whole 16x16 block it
/// reveals, or nothing where its block lies beyond the room. The reveal and its overlays write whole blocks, and a block
/// written beyond the grid's places shows nowhere.
/// </summary>
public sealed class XrayRevealGrid
{
    /// <summary>The grid's columns and rows of blocks.</summary>
    public static Extent16<RoomBlocks> Size => XrayRevealDefinitions.GridSize;

    /// <summary>Every place in the grid, row by row from the top and from left to right along each row.</summary>
    public static ImmutableArray<Position16<RoomBlocks>> Places { get; } =
    [
        .. from row in Enumerable.Range(0, Size.Y)
           from column in Enumerable.Range(0, Size.X)
           select Position16<RoomBlocks>.Of(column, row),
    ];

    /// <summary>What each place reveals, by column and then row; null where its block lies beyond the room.</summary>
    private readonly RevealedBlock?[,] blocks;

    /// <summary>A grid that reveals nothing at any place, as a reveal starts before it copies the room's blocks.</summary>
    public XrayRevealGrid() => blocks = new RevealedBlock?[Size.X, Size.Y];

    /// <summary>Keeps a copy of <paramref name="source"/>'s blocks, so later writes to either reach only its own.</summary>
    private XrayRevealGrid(XrayRevealGrid source) => blocks = (RevealedBlock?[,])source.blocks.Clone();

    /// <summary>What the grid reveals at <paramref name="place"/>; null where its block lies beyond the room.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="place"/> lies outside the grid.</exception>
    public RevealedBlock? this[Position16<RoomBlocks> place] =>
        Size.Contains(place) ? blocks[place.X, place.Y]
            : throw new ArgumentOutOfRangeException(nameof(place), place, "The place lies outside the X-ray reveal.");

    /// <summary>Every place's block in <see cref="Places"/> order; null where a block lies beyond the room.</summary>
    public IEnumerable<RevealedBlock?> Blocks => Places.Select(place => blocks[place.X, place.Y]);

    /// <summary>
    /// Reveals <paramref name="block"/> at <paramref name="place"/> when the place lies in the grid; a block written beyond
    /// the grid's last column or row shows nowhere.
    /// </summary>
    /// <param name="place">The place, counted from the reveal's first block.</param>
    /// <param name="block">The block revealed there, or null where the place's block lies beyond the room.</param>
    internal void Show(Position16<RoomBlocks> place, RevealedBlock? block)
    {
        if (Size.Contains(place))
            blocks[place.X, place.Y] = block;
    }

    /// <summary>A copy of this grid, which later writes to this grid do not reach.</summary>
    internal XrayRevealGrid Copy() => new(this);
}
