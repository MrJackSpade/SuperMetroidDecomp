using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="SpriteComposition"/> members production does not use.</summary>
internal static class SpriteCompositionAccess
{
    /// <summary>Exposes <see cref="SpriteComposition"/> state that only verification reads.</summary>
    extension(SpriteComposition self)
    {
        /// <summary>Appends all parts using the ordinary on-screen origin clipping path.</summary>
        /// <param name="oam">Object-attribute buffer that receives the ordered parts.</param>
        /// <param name="origin">Composition origin in screen-space pixels.</param>
        /// <param name="paletteBits">OBJ palette bits applied only to parts configured to inherit them.</param>
        public void DrawOnScreen(OamBuffer oam, Position16<SpritePixels> origin, ushort paletteBits)
        {
            _ = SnesObjAttributeWordConstants.FromPaletteBits(paletteBits);
            foreach (var part in self.parts)
            {
                SpritemapPart encoded = SpritemapPart.From(part.Part);
                oam.AddOnScreenSpritePart(encoded.X, encoded.Y,
                    part.InheritPalette ? encoded.Attributes.WithPaletteBits(paletteBits) : encoded.Attributes, origin);
            }
        }
        /// <summary>Preserves the cartridge's opposite Y-wrap clipping for negative origins.</summary>
        public void DrawOffScreen(OamBuffer oam, Position16<SpritePixels> origin, ushort paletteBits)
        {
            _ = SnesObjAttributeWordConstants.FromPaletteBits(paletteBits);
            foreach (var part in self.parts)
            {
                SpritemapPart encoded = SpritemapPart.From(part.Part);
                oam.AddOffScreenSpritePart(encoded.X, encoded.Y,
                    part.InheritPalette ? encoded.Attributes.WithPaletteBits(paletteBits) : encoded.Attributes,
                    origin);
            }
        }
    }
}
