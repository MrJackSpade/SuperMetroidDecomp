using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="PlanetZebesTextPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class PlanetZebesTextPaletteFxColorDefinitionAccess
{
    /// <summary>Resolves fade-in or fade-out definition pointers for the Planet Zebes text palette.</summary>
    extension(PlanetZebesTextPaletteFxColorDefinition self)
    {
        /// <summary>Native four-byte definition identity selected by fade direction.</summary>
        internal ushort DefinitionPointer => (ushort)(PlanetZebesTextPaletteFxColorDefinitions.FadeInDefinition + (int)self.Owner * 4);
        /// <summary>The colour group the definition's object animates.</summary>
        internal ColourEffectColours AnimatedColours => PaletteFxObjectAccess.FromNative(self.DefinitionPointer).AnimatedColours;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
