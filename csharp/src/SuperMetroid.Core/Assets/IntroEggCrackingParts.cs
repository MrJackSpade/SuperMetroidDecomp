using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Progressive replacement of intact cells with the matching cracked atlas patch.</summary>
internal sealed class IntroEggCrackingParts(int stage) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces a matching intact egg pose with the cell patches for the requested crack stage.</summary>
    /// <param name="pointer">Native sprite-list pointer identifying an egg pose.</param>
    /// <param name="supplied">Composition to retain when the pointer is not an egg pose.</param>
    /// <returns>The supplied composition or its stage-specific cracked-egg replacement.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int stage = 0; stage < 6; stage++)
            if (IntroDiscoveryActorSpriteDefinitions.EggFramePointer(stage + 3) == pointer)
                return supplied.CalculateIfMatching(new IntroEggCrackingParts(stage));
        return supplied;
    }

    /// <summary>Number of sprite cells emitted for the egg's three-by-three shell.</summary>
    public int Count => 9;

    /// <summary>Orders shell cells as the native pose lists expose them at this crack stage.</summary>
    /// <returns>All nine cells in draw order, including cells not yet cracked.</returns>
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

    /// <summary>Builds the sprite part for a cell in the stage-specific draw order.</summary>
    /// <param name="index">Zero-based position in the nine-cell composition.</param>
    /// <returns>The cell's intact or cracked tile with its grid-relative sprite coordinates.</returns>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            IntroEggCell cell = OrderedCells().ElementAt(index);
            int row = (int)cell / 3, column = (int)cell % 3;
            bool cracked = cell switch
            {
                IntroEggCell.TopLeft => true,
                IntroEggCell.TopMiddle => stage >= 1,
                IntroEggCell.MiddleLeft => stage >= 2,
                IntroEggCell.TopRight => stage >= 3,
                IntroEggCell.MiddleRight => stage >= 4,
                IntroEggCell.Center => stage >= 5,
                _ => false,
            };
            int tile = cracked ? IntroEggCrackingAtlas.Patch + 16 * row + column
                : row == 0 ? IntroEggRockingAtlas.TopStrip + column
                : column == 2 ? IntroEggRockingAtlas.RightColumn + row - 1
                : IntroEggRockingAtlas.LowerLeft + 16 * (row - 1) + column;
            return new(SnesSpritemapXWord.Create(-12 + 8 * column, false),
                unchecked((byte)(-12 + 8 * row)), SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    /// <summary>Enumerates the nine cell sprites in native draw order.</summary>
    /// <returns>An enumerator over this stage's compiled sprite parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Mutually exclusive cells in the egg three-by-three grid,in row-major order.</summary>
internal enum IntroEggCell
{
    /// <summary>Cell in the shell's upper-left grid position.</summary>
    TopLeft,
    /// <summary>Cell in the shell's upper-middle grid position.</summary>
    TopMiddle,
    /// <summary>Cell in the shell's upper-right grid position.</summary>
    TopRight,
    /// <summary>Cell in the shell's middle-left grid position.</summary>
    MiddleLeft,
    /// <summary>Cell at the center of the shell grid.</summary>
    Center,
    /// <summary>Cell in the shell's middle-right grid position.</summary>
    MiddleRight,
    /// <summary>Cell in the shell's lower-left grid position.</summary>
    BottomLeft,
    /// <summary>Cell in the shell's lower-middle grid position.</summary>
    BottomMiddle,
    /// <summary>Cell in the shell's lower-right grid position.</summary>
    BottomRight,
}

/// <summary>Identifies the first tile of the contiguous cracked-shell patch used by the intro egg.</summary>
internal static class IntroEggCrackingAtlas
{
    /// <summary>Tile$102,upper-left of the three-by-two cracked shell patch.</summary>
    internal const int Patch = 0x102;
}
