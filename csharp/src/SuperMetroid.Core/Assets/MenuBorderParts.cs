using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// $82:D00B/D0AD/D177 and D24B/D2F7/D41B: file-select/options rectangular borders of eight-pixel OBJ tiles.
/// Supplied bounds, six appearance roles and draw order remain independent required inputs;
/// only perimeter coordinates and repeated edge appearances calculate.
/// </summary>
internal sealed class MenuBorderParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Original signed left/top tile offsets and tile-grid width/height inferred from the supplied perimeter.</summary>
    private readonly int left, top, columns, rows;
    /// <summary>Perimeter coordinate index for each part in the supplied draw order.</summary>
    private readonly byte[] order;
    /// <summary>Shared attributes and palette-inheritance setting for each of the six corner or edge roles.</summary>
    private readonly Appearance[] appearances;
    /// <summary>Rendering attributes shared by all parts assigned to a particular border role.</summary>
    /// <param name="Attributes">OBJ attributes retained from a representative supplied part for this role.</param>
    /// <param name="InheritPalette">Whether parts in this role use the inherited palette selection.</param>
    private readonly record struct Appearance(SnesObjAttributeWord Attributes, bool InheritPalette);

    /// <summary>Stores the rectangular grid and appearance mapping inferred from a supplied border perimeter.</summary>
    /// <param name="left">Signed horizontal offset of the grid's leftmost tile.</param>
    /// <param name="top">Signed vertical offset of the grid's topmost tile.</param>
    /// <param name="columns">Number of eight-pixel tile columns in the border.</param>
    /// <param name="rows">Number of eight-pixel tile rows in the border.</param>
    /// <param name="order">Perimeter coordinates in the original supplied draw order.</param>
    /// <param name="appearances">Shared rendering attributes indexed by border role.</param>
    private MenuBorderParts(int left, int top, int columns, int rows, byte[] order, Appearance[] appearances)
    {
        this.left = left;
        this.top = top;
        this.columns = columns;
        this.rows = rows;
        this.order = order;
        this.appearances = appearances;
    }

    /// <summary>Replaces a recognized menu-border composition with its calculated perimeter form when its parts form a consistent grid.</summary>
    /// <param name="name">Composition name used to limit calculation to the supported menu border and heading sprites.</param>
    /// <param name="supplied">Authored parts whose perimeter, order, and repeated-role attributes are checked.</param>
    /// <returns>A composition using the calculated border parts when the supplied shape matches; otherwise <paramref name="supplied"/>.</returns>
    internal static SpriteComposition CalculateIfMatching(string name, SpriteComposition supplied)
    {
        if (name is not ("Border.Main" or "Border.Copy" or "Border.Clear" or "Heading.Primary" or "Heading.Controller" or "Heading.Special") || supplied.PartCount < 8)
            return supplied;
        int left = int.MaxValue, right = int.MinValue, top = int.MaxValue, bottom = int.MinValue;
        for (int index = 0; index < supplied.PartCount; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            left = Math.Min(left, part.X.SignedOffset);
            right = Math.Max(right, part.X.SignedOffset);
            top = Math.Min(top, unchecked((sbyte)part.Y));
            bottom = Math.Max(bottom, unchecked((sbyte)part.Y));
        }
        if ((right - left) % 8 != 0 || (bottom - top) % 8 != 0) return supplied;
        int columns = (right - left) / 8 + 1, rows = (bottom - top) / 8 + 1;
        if (columns < 3 || rows < 3 || 2 * (columns + rows - 2) != supplied.PartCount) return supplied;
        var order = new byte[supplied.PartCount];
        var appearances = new Appearance[6];
        var assigned = new bool[6];
        var occupied = new bool[supplied.PartCount];
        var calculated = new MenuBorderParts(left, top, columns, rows, order, appearances);
        for (int index = 0; index < supplied.PartCount; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            int x = part.X.SignedOffset - left, y = unchecked((sbyte)part.Y) - top;
            if (x % 8 != 0 || y % 8 != 0) return supplied;
            int column = x / 8, row = y / 8;
            int perimeter = row == 0 ? column
                : row == rows - 1 ? columns + column
                : column == 0 ? 2 * columns + row - 1
                : column == columns - 1 ? 2 * columns + rows - 2 + row - 1 : -1;
            if ((uint)perimeter >= occupied.Length || occupied[perimeter]) return supplied;
            occupied[perimeter] = true;
            order[index] = checked((byte)perimeter);
            int role = calculated.Role(column, row);
            var appearance = new Appearance(part.Attributes, part.InheritPalette);
            if (assigned[role] && appearances[role] != appearance) return supplied;
            appearances[role] = appearance;
            assigned[role] = true;
        }
        return supplied.CalculateIfMatching(calculated);
    }

    // Four distinct corners, one horizontal edge and one vertical edge.
    /// <summary>Maps a grid coordinate to one of four corners, the horizontal edge, or the vertical edge.</summary>
    /// <param name="column">Zero-based tile column within the rectangular border.</param>
    /// <param name="row">Zero-based tile row within the rectangular border.</param>
    /// <returns>Index of the appearance role assigned to that perimeter position.</returns>
    private int Role(int column, int row) => row == 0 || row == rows - 1
        ? column == 0 ? row == 0 ? 0 : 2
            : column == columns - 1 ? row == 0 ? 1 : 3 : 4
        : 5;

    /// <summary>Gets the number of border parts, preserving the supplied perimeter draw-order length.</summary>
    public int Count => order.Length;
    /// <summary>Gets the calculated tile part at its original supplied draw-order position.</summary>
    /// <param name="index">Zero-based position in the border's draw order.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside this border's parts.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int perimeter = order[index];
            int column, row;
            if (perimeter < 2 * columns)
            {
                column = perimeter % columns;
                row = perimeter < columns ? 0 : rows - 1;
            }
            else
            {
                int side = perimeter - 2 * columns;
                column = side < rows - 2 ? 0 : columns - 1;
                row = side % (rows - 2) + 1;
            }
            Appearance appearance = appearances[Role(column, row)];
            return new(SnesSpritemapXWord.Create(left + column * 8, false),
                unchecked((byte)(top + row * 8)), appearance.Attributes, appearance.InheritPalette);
        }
    }
    /// <summary>Enumerates calculated border parts in the original supplied draw order.</summary>
    /// <returns>An enumerator over the border's compiled sprite parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
