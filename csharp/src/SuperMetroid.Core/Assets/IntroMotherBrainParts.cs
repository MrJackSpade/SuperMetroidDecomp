using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Three-by-three sixteen-pixel grid with shared bottom row and center,
/// plus five animated cells packed into adjacent atlas patches.</summary>
internal sealed class IntroMotherBrainParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroMotherBrainSpriteDefinitions.FramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroMotherBrainParts(frame));
        return supplied;
    }
    public int Count => 9;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int row, column, tile;
            if (frame == 0)
            {
                row = 2 - index / 3;
                column = 2 - index % 3;
                tile = IntroMotherBrainAtlas.FirstPatch + 32 * row + 2 * column;
            }
            else if (index >= 5)
            {
                row = index == 8 ? 1 : 2;
                column = index == 8 ? 1 : 7 - index;
                tile = IntroMotherBrainAtlas.FirstPatch + 32 * row + 2 * column;
            }
            else if (frame == 2 && index == 0)
            {
                row = 0;
                column = 2;
                // The last patch crosses the atlas row boundary. Its top-right drawing
                // occupies the middle-center hole in the preceding animated patch.
                tile = IntroMotherBrainAtlas.FirstPatch + 6 + 32 + 2;
            }
            else
            {
                int sideIndex = frame == 2 ? index - 1 : index;
                bool middleSide = sideIndex < 2;
                row = middleSide ? 1 : 0;
                column = middleSide ? 2 - 2 * sideIndex : 4 - index;
                // Only two tile columns remain in the last patch; its middle sides pack adjacently.
                int atlasColumn = frame == 2 && middleSide ? column / 2 : column;
                tile = IntroMotherBrainAtlas.FirstPatch + 6 * frame + 32 * row + 2 * atlasColumn;
            }
            return new(SnesSpritemapXWord.Create(-24 + 16 * column, true),
                unchecked((byte)(-24 + 16 * row)), SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class IntroMotherBrainAtlas
{
    /// <summary>Tile$150, upper-left of the first48x48 drawing. Animated patches
    /// begin six tile columns apart;shared center and bottom cells remain in this patch.</summary>
    internal const int FirstPatch = 0x150;
}
