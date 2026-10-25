using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Progressive replacement of intact cells with the matching cracked atlas patch.</summary>
internal sealed class IntroEggCrackingParts(int stage) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Recalculates one of the six cracking poses only when its pose matches.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        for (int stage = 0; stage < 6; stage++)
            if (IntroDiscoveryActorSpriteDefinitions.EggFramePose(stage + 3) == shown)
                return supplied.CalculateIfMatching(new IntroEggCrackingParts(stage));
        return supplied;
    }
    /// <summary>Every cracking stage keeps the full three-by-three egg grid.</summary>
    public int Count => 9;

    /// <summary>Emits newly exposed shell cells before intact cells in the cartridge's draw order.</summary>
    private IEnumerable<IntroEggCell> OrderedCells()
    {
        // Native lists publish the newly exposed region before the remaining shell.
        // Center moves with the middle-right opening one pose before its tile changes.
        if (stage >= 4)
        {
            yield return IntroEggCell.Center;
            yield return IntroEggCell.MiddleRight;
        }
        if (stage >= 3) yield return IntroEggCell.TopRight;
        if (stage >= 2) yield return IntroEggCell.MiddleLeft;
        yield return IntroEggCell.TopLeft;
        if (stage >= 1) yield return IntroEggCell.TopMiddle;
        yield return IntroEggCell.BottomRight;
        if (stage < 4) yield return IntroEggCell.MiddleRight;
        if (stage < 3) yield return IntroEggCell.TopRight;
        if (stage < 1) yield return IntroEggCell.TopMiddle;
        yield return IntroEggCell.BottomMiddle;
        yield return IntroEggCell.BottomLeft;
        if (stage < 4) yield return IntroEggCell.Center;
        if (stage < 2) yield return IntroEggCell.MiddleLeft;
    }

    /// <summary>Chooses cracked patch or intact atlas tile for the ordered cell at this animation stage.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            IntroEggCell cell = OrderedCells().ElementAt(index);
            (int row, int column) = GridCell(cell);
            bool cracked = cell switch
            {
                IntroEggCell.TopLeft => true,
                IntroEggCell.TopMiddle => stage >= 1,
                IntroEggCell.MiddleLeft => stage >= 2,
                IntroEggCell.TopRight => stage >= 3,
                IntroEggCell.MiddleRight => stage >= 4,
                IntroEggCell.Center => stage >= 5,
                IntroEggCell.BottomLeft or IntroEggCell.BottomMiddle or IntroEggCell.BottomRight => false,
                _ => throw new InvalidOperationException($"Undefined egg cell {cell}."),
            };
            int tile = cracked ? IntroEggCrackingCellDefinitions.Patch + 16 * row + column
                : row == 0 ? IntroEggRockingCellDefinitions.TopStrip + column
                : column == 2 ? IntroEggRockingCellDefinitions.RightColumn + row - 1
                : IntroEggRockingCellDefinitions.LowerLeft + 16 * (row - 1) + column;
            return new(SnesSpritemapXWord.Create(-12 + 8 * column, false),
                SnesSpritemapYByte.Create(-12 + 8 * row), SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    /// <summary>The row and column <paramref name="cell"/> occupies in the shell's three-by-three grid.</summary>
    private static GridPosition GridCell(IntroEggCell cell) => cell switch
    {
        IntroEggCell.TopLeft => new(0, 0),
        IntroEggCell.TopMiddle => new(0, 1),
        IntroEggCell.TopRight => new(0, 2),
        IntroEggCell.MiddleLeft => new(1, 0),
        IntroEggCell.Center => new(1, 1),
        IntroEggCell.MiddleRight => new(1, 2),
        IntroEggCell.BottomLeft => new(2, 0),
        IntroEggCell.BottomMiddle => new(2, 1),
        IntroEggCell.BottomRight => new(2, 2),
        _ => throw new InvalidOperationException($"Undefined egg cell {cell}."),
    };

    /// <summary>A cell's row and column in the shell's three-by-three grid.</summary>
    /// <param name="Row">The grid row, top first.</param>
    /// <param name="Column">The grid column, left first.</param>
    private readonly record struct GridPosition(int Row, int Column);

    /// <summary>Enumerates all nine egg cells in the stage's native draw order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same cell order to non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
