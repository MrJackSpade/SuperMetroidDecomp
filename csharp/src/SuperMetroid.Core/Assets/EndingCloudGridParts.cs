using System.Collections;
using SuperMetroid.Core.Hardware;
using Role = SuperMetroid.Core.Assets.EndingCloudSpriteDefinitions.Role;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:B6F3..B97E: six cloud compositions use four-column32-pixel grids,
/// repeated horizontally for the two-row strips or vertically for four-row panels.
/// Upper strips reflect both axes; the left panel reflects X. The native small-OBJ
/// flag stays clear: the scene's OBJ size selection supplies the32-pixel geometry.</summary>
internal sealed class EndingCloudGridParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Selects the cloud composition whose tile layout is generated.</summary>
    private readonly Role role;

    /// <summary>Creates a lazy grid of compiled sprite parts for one authored cloud composition.</summary>
    /// <param name="role">Composition role that determines the grid dimensions and reflection rules.</param>
    /// <exception cref="ArgumentOutOfRangeException">The role is not one of the six defined cloud compositions.</exception>
    internal EndingCloudGridParts(Role role)
    {
        if ((uint)role > (uint)Role.Left) throw new ArgumentOutOfRangeException(nameof(role));
        this.role = role;
    }

    /// <summary>Indicates whether the selected composition is a vertical side panel.</summary>
    private bool Side => role is Role.Right or Role.Left;

    /// <summary>Gets the number of compiled sprite parts in this composition.</summary>
    public int Count => Side ? 32 : 16;

    /// <summary>Gets the part at its native ordering index in the generated cloud grid.</summary>
    /// <param name="index">Zero-based position within the composition.</param>
    /// <returns>The compiled sprite part, including its tile, position, and flip flags.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the generated composition.</exception>
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
            int firstTile = Side ? 0x100 : role is Role.UpperPattern or Role.LowerPattern ? 0x80 : 0;
            SnesTileFlipFlags flips = (flipX ? SnesTileFlipFlags.Horizontal : 0) |
                (upper ? SnesTileFlipFlags.Vertical : 0);
            return new(SnesSpritemapXWord.Create(x, false), unchecked((byte)y),
                SnesObjAttributeWord.Create(firstTile + row * 0x40 + column * 4, 0, 3, flips), true);
        }
    }

    /// <summary>Enumerates every generated sprite part in native ordering.</summary>
    /// <returns>An enumerator that lazily produces this composition's parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
