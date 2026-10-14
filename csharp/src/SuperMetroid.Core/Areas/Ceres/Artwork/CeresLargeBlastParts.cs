using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Four single-sprite Ceres blasts: centered X=-8 and Y=-10-2*frame.
/// Size16, priority3, no flips and inherited palette are common to all four records.
/// The four atlas selections retain the authored cloud-breakup drawing sequence.</summary>
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
    /// <summary>Replaces a matching one-part native blast with calculated placement while retaining its supplied tile drawing.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        if (supplied.PartCount != 1) return supplied;
        for (int frame = 0; frame < 4; frame++)
            if (pointer == CeresDestructionSpriteDefinitions.LargeFrame(frame))
                return supplied.CalculateIfMatching(new CeresLargeBlastParts(frame, supplied.Part(0).Attributes.TileNumber));
        return supplied;
    }
    /// <summary>Each of the four blast frames emits exactly one 16-pixel sprite.</summary>
    public int Count => 1;
    /// <summary>Places the supplied cloud tile at X=-8 and a frame-dependent rising Y offset.</summary>
    public CompiledSpritePart this[int index] => index == 0
        ? new(SnesSpritemapXWord.Create(-8, true), unchecked((byte)(-10 - 2 * frame)),
            SnesObjAttributeWord.Create(tile, 0, 3, 0), true)
        : throw new ArgumentOutOfRangeException(nameof(index));
    /// <summary>Emits the sole sprite part in native draw order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator() { yield return this[0]; }
    /// <summary>Uses the same single-part enumeration for non-generic consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
