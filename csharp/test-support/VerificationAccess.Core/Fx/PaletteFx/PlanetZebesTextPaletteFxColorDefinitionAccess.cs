
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="PlanetZebesTextPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class PlanetZebesTextPaletteFxColorDefinitionAccess
{
    /// <summary>Resolves fade-in or fade-out definition pointers for the Planet Zebes text palette.</summary>
    extension(PlanetZebesTextPaletteFxColorDefinition self)
    {
        /// <summary>Native four-byte definition identity selected by fade direction.</summary>
        internal ushort DefinitionPointer => (ushort)(PlanetZebesTextPaletteFxColorDefinitions.FadeInDefinition + (int)self.Owner * 4);
    }
}
