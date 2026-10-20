using System.Collections.Immutable;
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
        [PpuLayoutBoundary("frontend OAM writer: SpriteComposition.DrawOnScreen until #1358 moves it onto sprite entries")]
        public void DrawOnScreen(OamBuffer oam, Position16<OamPixels> origin, ushort paletteBits)
        {
            _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
            foreach (var part in self.parts)
                oam.AddOnScreenSpritePart(part.X, part.Y,
                    part.InheritPalette ? part.Attributes.WithPaletteBits(paletteBits) : part.Attributes, origin);
        }
        /// <summary>Preserves the cartridge's opposite Y-wrap clipping for negative origins.</summary>
        [PpuLayoutBoundary("frontend OAM writer: SpriteComposition.DrawOffScreen until #1358 moves it onto sprite entries")]
        public void DrawOffScreen(OamBuffer oam, Position16<OamPixels> origin, ushort paletteBits)
        {
            _ = SnesObjAttributeWord.FromPaletteBits(paletteBits);
            foreach (var part in self.parts)
                oam.AddOffScreenSpritePart(part.X, part.Y,
                    part.InheritPalette ? part.Attributes.WithPaletteBits(paletteBits) : part.Attributes,
                    origin);
        }
    }
}
