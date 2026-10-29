using System.Collections;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Intact egg and two opposite row-sheared rocking poses.</summary>
internal sealed class IntroEggRockingParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Recalculates an egg frame only when its pose selects one of the three rocking poses.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroDiscoveryActorSpritePoses.EggFramePose(frame) == shown)
                return supplied.CalculateIfMatching(new IntroEggRockingParts(frame));
        return supplied;
    }
    /// <summary>The intact egg uses a large lower-left part; either sheared pose splits it into four small parts.</summary>
    public int Count => frame == 0 ? 6 : 9;
    /// <summary>Places an egg atlas part on its grid, shearing each rocking row according to the selected pose.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int row, column, tile;
            bool large = frame == 0 && index == 3;
            if (index < 3)
            {
                row = 0;
                column = 2 - index;
                tile = IntroEggRockingCellDefinitions.TopStrip + column;
            }
            else if (large)
            {
                row = 1;
                column = 0;
                tile = IntroEggRockingCellDefinitions.LowerLeft;
            }
            else
            {
                int piece = index - (frame == 0 ? 4 : 3);
                if (piece < 2)
                {
                    row = 2 - piece;
                    column = 2;
                    tile = IntroEggRockingCellDefinitions.RightColumn + row - 1;
                }
                else
                {
                    int grid = piece - 2;
                    row = 2 - grid / 2;
                    column = 1 - grid % 2;
                    tile = IntroEggRockingCellDefinitions.LowerLeft + 16 * (row - 1) + column;
                }
            }
            int shear = frame == 0 ? 0 : (frame == 1 ? -1 : 1) * (3 - row);
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(-12 + 8 * column + shear, -12 + 8 * row), large, tile, 0, 3, 0), true);
        }
    }
    /// <summary>Enumerates egg parts in the cartridge's top-strip then lower-body drawing order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same drawing order to callers using the non-generic collection interface.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
