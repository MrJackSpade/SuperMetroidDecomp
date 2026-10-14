using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="PlanetZebesTextPaletteFxProgramDefinition"/> members production does not use.</summary>
internal static class PlanetZebesTextPaletteFxProgramDefinitionAccess
{
    /// <summary>Resolves fade-in or fade-out definition pointers for the Planet Zebes text palette.</summary>
    extension(PlanetZebesTextPaletteFxProgramDefinition self)
    {
        /// <summary>Native four-byte definition identity selected by fade direction.</summary>
        internal ushort DefinitionPointer => (ushort)(PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FadeInDefinition + (int)self.Owner * 4);
    }
}
