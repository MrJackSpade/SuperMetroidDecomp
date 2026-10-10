using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:9AF2: suited jumping pose, with independently positioned limb
/// strips and upper/lower body atlas regions.</summary>
internal sealed class EndingRewardJumpParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses the 20-piece suited jump layout for the matching ending frame.</summary>
    /// <param name="pointer">Native frame pointer used to select the ending pose.</param>
    /// <param name="supplied">Composition to retain when the pointer selects another frame.</param>
    /// <returns>The supplied composition with this layout applied only for the matching frame.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied) =>
        pointer == EndingRewardSpriteDefinitions.FramePointer(EndingRewardSpriteFrame.LargeSamusFromEndingJumping)
            ? supplied.CalculateIfMatching(new EndingRewardJumpParts()) : supplied;

    /// <summary>Number of independently positioned sprite pieces in the suited jumping pose.</summary>
    public int Count => 20;

    /// <summary>Gets a sprite piece at its position in the pose's draw order.</summary>
    /// <param name="index">Zero-based piece index, from zero through <see cref="Count"/> minus one.</param>
    /// <returns>The compiled sprite piece at the requested index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the available pieces.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile, x, y;
            if (index == 0)
            {
                tile = EndingRewardJumpAtlas.UpperBodyCenter; x = -4; y = -53;
            }
            else if (index < 3)
            {
                int row = 2 - index;
                tile = 0x0e + row * 32; x = 19; y = -61 + row * 16;
            }
            else if (index == 3)
            {
                tile = EndingRewardJumpAtlas.LeftLimbCap; x = -27; y = -62;
            }
            else if (index < 6)
            {
                int row = 5 - index;
                tile = 0x17 + row * 16; x = -36; y = -52 + row * 8;
            }
            else if (index < 11)
            {
                // Lower upper-body row right-to-left, then the two upper corners.
                tile = index < 9 ? 0x3d - 2 * (index - 6) : 0x1d - 4 * (index - 9);
                x = -20 + 8 * ((tile & 15) - 9);
                y = -53 + 8 * (tile / 16 - 1);
            }
            else
            {
                if (index < 13) tile = 0x5c - 2 * (index - 11); // waist pair
                else if (index < 16) tile = 0xec - 32 * (index - 13); // lower leg, bottom to top
                else
                {
                    int cell = index - 16;
                    tile = 0x8c - 32 * (cell / 2) - 2 * (cell % 2); // paired upper-leg rows
                }
                x = -16 + 8 * ((tile & 15) - 10);
                y = -13 + 8 * (tile / 16 - 6);
            }
            return new(SnesSpritemapXWord.Create(x, index != 13), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }

    /// <summary>Enumerates all sprite pieces in their composition order.</summary>
    /// <returns>An enumerator over the compiled pieces, from first drawn to last.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Individually ordered pieces of the suited jumping pose.</summary>
internal static class EndingRewardJumpAtlas
{
    /// <summary>Tile$1B, upper-body center drawn ahead of the limbs and remaining body rows.</summary>
    internal const int UpperBodyCenter = 0x1b;
    /// <summary>Tile$08, separately positioned upper-left limb cap.</summary>
    internal const int LeftLimbCap = 0x08;
}
