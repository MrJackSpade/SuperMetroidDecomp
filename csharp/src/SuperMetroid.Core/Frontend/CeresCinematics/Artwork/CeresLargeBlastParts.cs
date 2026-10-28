using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

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
internal static class CeresLargeBlastParts
{
    /// <summary>Replaces a matching one-part native blast with calculated placement while retaining its supplied tile drawing.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        if (supplied.PartCount != 1) return supplied;
        for (int frame = 0; frame < 4; frame++)
            if (shown == CeresDestructionSpriteDefinitions.LargeFrame(frame))
                return supplied.CalculateIfMatching(Parts(frame, supplied.Part(0).Part.Character));
        return supplied;
    }
    /// <summary>Each blast frame's one 16-pixel sprite: the supplied cloud tile at X=-8 and a frame-dependent rising Y offset.</summary>
    private static CompiledSpritePart[] Parts(int frame, int tile) =>
        [new(SpritePart.Create(Offset16<ScreenPixels>.Of(-8, -10 - 2 * frame), true, tile, 0, 3, 0), true)];
}
