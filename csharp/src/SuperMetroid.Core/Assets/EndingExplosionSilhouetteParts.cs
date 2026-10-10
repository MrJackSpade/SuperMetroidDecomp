using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$8C:A57C silhouette: lower body/tip pieces followed by four right-to-left
/// rows of body and ray tiles. The packed atlas regions are reordered vertically
/// to reconstruct the original streaked image. Priority3, no flips, inherited palette.</summary>
internal sealed class EndingExplosionSilhouetteParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Identifies the vertical bands emitted after the silhouette's lower body pieces.</summary>
    private enum Row
    {
        /// <summary>The central body band at the top of the atlas reconstruction.</summary>
        Body,

        /// <summary>The ray band immediately surrounding the body.</summary>
        InnerRays,

        /// <summary>The next ray band, extending farther from the body.</summary>
        MiddleRays,

        /// <summary>The outermost ray band at the top of the silhouette.</summary>
        OuterRays
    }

    /// <summary>Uses the reconstructed silhouette only when the supplied spritemap is the native silhouette pose.</summary>
    /// <param name="pointer">Native spritemap pointer being resolved.</param>
    /// <param name="supplied">Composition produced by the general sprite resolver.</param>
    /// <returns>The matching composition with this silhouette's atlas regions, or the original composition when the pointer differs.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied) =>
        pointer == EndingExplosionSpriteDefinitions.Pointer(EndingExplosionSpriteDefinitions.Pose.Silhouette)
            ? supplied.CalculateIfMatching(new EndingExplosionSilhouetteParts()) : supplied;

    /// <summary>Gets the number of ordered sprite parts that form the silhouette.</summary>
    public int Count => 20;

    /// <summary>Builds one sprite part from the native silhouette's ordered lower pieces and reconstructed atlas rows.</summary>
    /// <param name="index">Zero-based part position in the 20-part composition.</param>
    /// <returns>The tile, placement, palette, and priority data for that part.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the composition.</exception>
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
                (int rightTile, int top) = (Row)((index - 4) / 4) switch
                {
                    Row.Body => (0xbe, 0),
                    Row.InnerRays => (0xe6, -16),
                    Row.MiddleRays => (0xd6, -24),
                    Row.OuterRays => (0xb6, -40),
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                };
                tile = rightTile - column * 2;
                y = top;
            }
            return new(SnesSpritemapXWord.Create(x, large), unchecked((byte)y),
                SnesObjAttributeWord.Create(tile, 0, 3), true);
        }
    }
    /// <summary>Enumerates all sprite parts in the order expected by the compiled composition.</summary>
    /// <returns>An enumerator over the 20 silhouette parts.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
