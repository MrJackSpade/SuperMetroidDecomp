using System.Collections;
using Role = SuperMetroid.Core.Frontend.Ending.Artwork.EndingCloudSpriteDefinitions.Role;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>$8C:B6F3..B97E: six cloud compositions use four-column32-pixel grids,
/// repeated horizontally for the two-row strips or vertically for four-row panels.
/// Upper strips reflect both axes; the left panel reflects X. The native small-OBJ
/// flag stays clear: the scene's OBJ size selection supplies the32-pixel geometry.</summary>
internal sealed class EndingCloudGridParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Upper or lower strip, or side panel, determining grid repeat and reflection.</summary>
    private readonly Role role;
    /// <summary>Admits one of the six authored cloud roles before calculating its repeated tile grid.</summary>
    internal EndingCloudGridParts(Role role)
    {
        if (!Enum.IsDefined(role)) throw new ArgumentOutOfRangeException(nameof(role));
        this.role = role;
    }
    /// <summary>Side panels repeat a four-row grid; horizontal strips repeat a two-row grid.</summary>
    private bool Side => role is Role.Right or Role.Left;
    /// <summary>Two copies of sixteen side-panel parts or eight strip parts.</summary>
    public int Count => Side ? 32 : 16;
    /// <summary>Places a 32-pixel cloud tile in the repeated grid with the role's native axis reflections.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int rows = Side ? 4 : 2;
            int column = 3 - index % 4;
            int row = rows - 1 - index / 4 % rows;
            int copy = index / (4 * rows);
            int x = column * 32 - (Side ? 0 : copy * 128);
            int y = Side ? row * 32 - copy * 128 : (row - 1) * 32;
            bool upper = role is Role.UpperPattern or Role.UpperEdge;
            bool flipX = upper || role == Role.Left;
            if (flipX) x = -x - 32;
            if (upper) y = -y - 32;
            int firstTile = Side ? CharacterSheetGrid.Cell(16, 0) : role is Role.UpperPattern or Role.LowerPattern ? CharacterSheetGrid.Cell(8, 0) : 0;
            TileFlip flips = (flipX ? TileFlip.Horizontal : 0) |
                (upper ? TileFlip.Vertical : 0);
            return new(SpritePart.Create(x, y, false,
                firstTile + row * 4 * CharacterSheetGrid.CharactersPerRow + column * 4, 0, 3, flips), true);
        }
    }
    /// <summary>Yields cloud tiles in the native row and copy order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Preserves native cloud-part order for non-generic consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
