using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// $82:D00B/D0AD/D177: file-select rectangular borders made from eight-pixel OBJ tiles.
/// Supplied bounds, six appearance roles and draw order remain independent required inputs;
/// only perimeter coordinates and repeated edge appearances calculate.
/// </summary>
internal sealed class MenuBorderParts : IReadOnlyList<CompiledSpritePart>
{
    private readonly int left, top, columns, rows;
    private readonly byte[] order;
    private readonly Appearance[] appearances;
    private readonly record struct Appearance(SnesObjAttributeWord Attributes, bool InheritPalette);

    private MenuBorderParts(int left, int top, int columns, int rows, byte[] order, Appearance[] appearances)
    {
        this.left = left;
        this.top = top;
        this.columns = columns;
        this.rows = rows;
        this.order = order;
        this.appearances = appearances;
    }

    internal static SpriteComposition CalculateIfMatching(string name, SpriteComposition supplied)
    {
        if (name is not ("Border.Main" or "Border.Copy" or "Border.Clear") || supplied.PartCount < 8)
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
    private int Role(int column, int row) => row == 0 || row == rows - 1
        ? column == 0 ? row == 0 ? 0 : 2
            : column == columns - 1 ? row == 0 ? 1 : 3 : 4
        : 5;

    public int Count => order.Length;
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
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
