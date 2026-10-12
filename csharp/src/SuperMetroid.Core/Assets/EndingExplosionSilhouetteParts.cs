using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:A57C silhouette: lower body/tip pieces followed by four right-to-left
/// rows of body and ray tiles. The packed atlas regions are reordered vertically
/// to reconstruct the original streaked image. Priority3, no flips, inherited palette.</summary>
internal sealed class EndingExplosionSilhouetteParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Right-hand tile and top Y of the four streaked rows, top to bottom in draw
    /// order: body, inner rays, middle rays, outer rays.</summary>
    private static readonly (int RightTile, int Top)[] Rows = [(0xbe, 0), (0xe6, -16), (0xd6, -24), (0xb6, -40)];

    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied) =>
        pointer == EndingExplosionSpriteDefinitions.Pointer(EndingExplosionSpriteDefinitions.Pose.Silhouette)
            ? supplied.CalculateIfMatching(new EndingExplosionSilhouetteParts()) : supplied;

    public int Count => 20;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int x, y, tile;
            bool large = true;
            if (index < 2) { x = index * 16; y = 8; tile = 0xcc + index * 2; }
            else if (index == 2) { x = 0; y = 24; tile = 0xec; large = false; }
            else if (index == 3) { x = -32; y = 16; tile = 0xd8; }
            else
            {
                int column = (index - 4) % 4;
                x = 16 - column * 16;
                (int rightTile, int top) = Rows[(index - 4) / 4];
                tile = rightTile - column * 2;
                y = top;
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
