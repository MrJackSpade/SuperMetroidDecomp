using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Four single-sprite Ceres blasts: centered X=-8 and Y=-10-2*frame.
/// Size16, priority3, no flips and inherited palette are common to all four records.
/// The four atlas selections retain the authored cloud-breakup drawing sequence.</summary>
/// <param name="frame">Zero-based frame in the four-part authored cloud-breakup sequence.</param>
/// <param name="tile">Atlas tile selected by the original spritemap for this drawing.</param>
/// <remarks>
/// Original tiles76/78/98/9E select independently drawn cloud, open cloud, broken
/// rim and scattered specks from9A:D200. Native8B:CD1B..CD38 plays these drawings
/// for five ticks each before a blank interval; it supplies no geometric input
/// from which to derive their changing contours or interior markings. A fitted
/// ordinal formula or numeric switch would merely recite this chosen artwork
/// sequence. Retain only these drawing selections under the nonsense exception;
/// positions and shared attributes remain calculated. Source pixels are a separate
/// review, not covered by this disposition.
/// </remarks>
internal sealed class CeresLargeBlastParts(int frame, int tile) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>
    /// Replaces the matching one-part Ceres blast composition with calculated position and attributes while retaining its selected tile.
    /// </summary>
    /// <param name="pointer">Native large-blast animation pointer used to identify the frame.</param>
    /// <param name="supplied">Composition produced by the ordinary spritemap path.</param>
    /// <returns>The calculated blast composition for a matching pointer, or <paramref name="supplied"/> unchanged otherwise.</returns>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (supplied.PartCount != 1) return supplied;
        for (int frame = 0; frame < 4; frame++)
            if (pointer == CeresDestructionSpriteDefinitions.LargeFrame(frame))
                return supplied.CalculateIfMatching(new CeresLargeBlastParts(frame, supplied.Part(0).Attributes.TileNumber));
        return supplied;
    }

    /// <summary>
    /// Gets the number of sprite parts used for one large-blast frame.
    /// </summary>
    public int Count => 1;

    /// <summary>
    /// Gets the sole sprite part, calculating its frame position and shared attributes.
    /// </summary>
    /// <param name="index">Zero-based part index; only zero is valid.</param>
    /// <returns>The calculated sprite part.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not zero.</exception>
    public CompiledSpritePart this[int index] => index == 0
        ? new(SnesSpritemapXWord.Create(-8, true), unchecked((byte)(-10 - 2 * frame)),
            SnesObjAttributeWord.Create(tile, 0, 3, 0), true)
        : throw new ArgumentOutOfRangeException(nameof(index));
    /// <summary>
    /// Enumerates the single calculated sprite part in this frame.
    /// </summary>
    /// <returns>An enumerator containing the part at index zero.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator() { yield return this[0]; }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
