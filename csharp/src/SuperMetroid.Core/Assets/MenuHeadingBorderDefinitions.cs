using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$82:D24B/D2F7/D41B: options heading outlines. Stock bounds follow immutable heading typography;
/// independently edited title pages or sprite parts do not change these stock layout definitions. Exact title/padding, glyph roles and traversal are selected display content. Native82:8CA1 feeds ordered OAM; order may affect hardware limits and is preserved. Pixels, colors and timing are excluded.</summary>
internal sealed class MenuHeadingBorderDefinitions(string heading) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Native heading words on the options BG pages; each uses eight-pixel character cells.</summary>
    private string Text => heading switch
    {
        "Heading.Primary" => "OPTION MODE",
        "Heading.Controller" => "CONTROLLER SETTING MODE",
        "Heading.Special" => "SPECIAL SETTING MODE",
        _ => throw new ArgumentOutOfRangeException(nameof(heading)),
    };
    /// <summary>$82:D24B/D2F7/D41B: one blank cell and one corner cell pad each side of the heading.</summary>
    private int Columns => Text.Length + 2 * 2;
    /// <summary>Two glyph rows enclosed above/below by the border tile rows.</summary>
    private const int Rows = 2 + 2;
    /// <summary>$82:D24B/D2F7/D41B: common outer traversal runs span six horizontal edge tiles; primary's paired center consumes one extra left tile.</summary>
    private const int OuterRunLength = 6;
    /// <summary>$82:D24B and siblings: F9..FD is the packed top-left/edge/top-right/bottom-left/bottom-right artwork strip.</summary>
    private const int CornerStrip = 0xf9;
    /// <summary>$82:D24B and siblings: ED is the vertical edge artwork.</summary>
    private const int VerticalEdge = 0xed;
    /// <summary>$82:D2F7: selected center traversal reused identically for bottom and top rows; chosen native OAM order retained as authored ordering; geometry and repeated runs calculate.</summary>
    private static ReadOnlySpan<byte> ControllerCenter => [0, 1, 7, 6, 5, 2, 4, 3];
    /// <summary>$82:D41B: selected shorter center traversal reused on both rows; chosen native OAM order retained as authored ordering; geometry and repeated runs calculate.</summary>
    private static ReadOnlySpan<byte> SpecialCenter => [0, 1, 4, 3, 2];

    internal static SpriteComposition CalculateIfMatching(string name, SpriteComposition supplied) =>
        name is "Heading.Primary" or "Heading.Controller" or "Heading.Special"
            ? supplied.CalculateIfMatching(new MenuHeadingBorderDefinitions(name)) : supplied;

    public int Count => 2 * (Columns + Rows - 2);
    private (int Column, int Row) Position(int index)
    {
        bool primary = heading == "Heading.Primary";
        if (primary)
        {
            if (index < 4) return (OuterRunLength + 1 - index / 2, index % 2 == 0 ? Rows - 1 : 0);
            index -= 4;
        }
        else
        {
            ReadOnlySpan<byte> center = heading == "Heading.Controller" ? ControllerCenter : SpecialCenter;
            if (index < center.Length * 2)
                return (OuterRunLength + 3 + center[index % center.Length], index < center.Length ? Rows - 1 : 0);
            index -= center.Length * 2;
            if (index < 10)
            {
                int leftOuter = OuterRunLength + 1, leftInner = leftOuter + 1;
                int rightOuter = Columns - OuterRunLength - 2, rightInner = rightOuter - 1, rightNear = rightOuter - 2;
                return index switch
                {
                    0 => (leftOuter, 0), 1 => (leftOuter, Rows - 1),
                    2 => (rightInner, Rows - 1), 3 => (rightOuter, Rows - 1),
                    4 => (leftInner, Rows - 1), 5 => (rightNear, Rows - 1),
                    6 => (rightInner, 0), 7 => (rightOuter, 0),
                    8 => (leftInner, 0), _ => (rightNear, 0),
                };
            }
            index -= 10;
        }
        if (index < 8)
            return index switch
            {
                0 => (0, Rows - 2), 1 => (0, 1),
                2 => (Columns - 1, Rows - 2), 3 => (Columns - 1, 1),
                4 => (Columns - 1, Rows - 1), 5 => (0, Rows - 1),
                6 => (Columns - 1, 0), _ => (0, 0),
            };
        index -= 8;
        int leftLength = OuterRunLength - (primary ? 1 : 0);
        if (index < leftLength) return (leftLength - index, Rows - 1);
        index -= leftLength;
        if (index < OuterRunLength * 2)
            return (Columns - 2 - index % OuterRunLength, index < OuterRunLength ? Rows - 1 : 0);
        index -= OuterRunLength * 2;
        return (leftLength - index, 0);
    }
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            var (column, row) = Position(index);
            int tile = row == 0 || row == Rows - 1
                ? column == 0 ? CornerStrip + (row == 0 ? 0 : 3)
                : column == Columns - 1 ? CornerStrip + (row == 0 ? 2 : 4) : CornerStrip + 1
                : VerticalEdge;
            return new(SnesSpritemapXWord.Create(-Columns * 8 / 2 + column * 8, false),
                unchecked((byte)(-Rows * 8 / 2 + row * 8)),
                SnesObjAttributeWord.Create(tile, 0, 3, default), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
