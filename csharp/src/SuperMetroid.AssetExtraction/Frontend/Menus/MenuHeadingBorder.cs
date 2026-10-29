using System.Collections;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using static SuperMetroid.AssetExtraction.Frontend.Menus.MenuHeadingBorderDefinitions;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Menus;

/// <summary>$82:D00B/D0AD/D177 and D24B/D2F7/D41B: file-select/options heading outlines. Stock bounds follow immutable heading typography;
/// independently edited title pages or sprite parts do not change these stock layout definitions. Exact title/padding, glyph roles and traversal are selected display content. Native82:8CA1 feeds ordered OAM; order may affect hardware limits and is preserved. Pixels, colors and timing are excluded.</summary>
internal sealed class MenuHeadingBorder(MenuHeading heading) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>The heading word this outline encloses.</summary>
    private string Text => Ensure.Listed(Texts, heading);
    /// <summary>Character columns spanned by the heading word and its padding on both sides.</summary>
    private int Columns => Text.Length + 2 * SidePadding;

    /// <summary>Uses calculated border geometry only for the six authored file and options headings.</summary>
    internal static SpriteComposition CalculateIfMatching(MenuHeading heading, SpriteComposition supplied) =>
        supplied.CalculateIfMatching(new MenuHeadingBorder(heading));

    /// <summary>Perimeter OAM parts for both horizontal edges and the two vertical sides.</summary>
    public int Count => 2 * (Columns + Rows - 2);
    /// <summary>Calculates an indexed glyph or border cell in native heading traversal order.</summary>
    private MenuHeadingCell Position(int index)
    {
        bool primary = heading == MenuHeading.OptionsPrimary;
        bool fileSelect = heading is MenuHeading.FileSelectMain or MenuHeading.FileSelectCopy or MenuHeading.FileSelectClear;
        if (fileSelect)
        {
            bool clear = heading == MenuHeading.FileSelectClear;
            if (clear)
            {
                if (index < 2) return new(OuterRunLength + 1, index == 0 ? 0 : Rows - 1);
                index -= 2;
            }
            if (heading != MenuHeading.FileSelectMain)
            {
                if (index < 8)
                    return new(OuterRunLength + 1 + (clear ? 1 : 0) + (index % 4 + 2) % 4,
                        index < 4 ? Rows - 1 : 0);
                index -= 8;
            }
        }
        else if (primary)
        {
            if (index < 4) return new(OuterRunLength + 1 - index / 2, index % 2 == 0 ? Rows - 1 : 0);
            index -= 4;
        }
        else
        {
            ReadOnlySpan<byte> center = heading == MenuHeading.OptionsController ? ControllerCenter : SpecialCenter;
            if (index < center.Length * 2)
                return new(OuterRunLength + 3 + center[index % center.Length], index < center.Length ? Rows - 1 : 0);
            index -= center.Length * 2;
            if (index < 10)
            {
                int leftOuter = OuterRunLength + 1, leftInner = leftOuter + 1;
                int rightOuter = Columns - OuterRunLength - 2, rightInner = rightOuter - 1, rightNear = rightOuter - 2;
                return index switch
                {
                    0 => new(leftOuter, 0), 1 => new(leftOuter, Rows - 1),
                    2 => new(rightInner, Rows - 1), 3 => new(rightOuter, Rows - 1),
                    4 => new(leftInner, Rows - 1), 5 => new(rightNear, Rows - 1),
                    6 => new(rightInner, 0), 7 => new(rightOuter, 0),
                    8 => new(leftInner, 0), _ => new(rightNear, 0),
                };
            }
            index -= 10;
        }
        if (index < 8)
            return index switch
            {
                0 => new(0, Rows - 2), 1 => new(0, 1),
                2 => new(Columns - 1, Rows - 2), 3 => new(Columns - 1, 1),
                4 => new(Columns - 1, Rows - 1), 5 => new(0, Rows - 1),
                6 => new(Columns - 1, 0), _ => new(0, 0),
            };
        index -= 8;
        int leftLength = OuterRunLength - (primary ? 1 : 0);
        if (index < leftLength) return new(leftLength - index, Rows - 1);
        index -= leftLength;
        if (index < OuterRunLength * 2)
            return new(Columns - 2 - index % OuterRunLength, index < OuterRunLength ? Rows - 1 : 0);
        index -= OuterRunLength * 2;
        return new(leftLength - index, 0);
    }
    /// <summary>Builds one OAM part from the selected border cell and its atlas tile.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            var (column, row) = Position(index);
            int tile = row is 0 or (Rows - 1)
                ? column == 0 ? row == 0 ? TopLeftCorner : BottomLeftCorner
                : column == Columns - 1 ? row == 0 ? TopRightCorner : BottomRightCorner : HorizontalEdge
                : VerticalEdge;
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(-Columns * 8 / 2 + column * 8, -Rows * 8 / 2 + row * 8), false,
                tile, 0, 3, default), true);
        }
    }
    /// <summary>Visits border parts in their authored OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Provides the same OAM order to non-generic readers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
