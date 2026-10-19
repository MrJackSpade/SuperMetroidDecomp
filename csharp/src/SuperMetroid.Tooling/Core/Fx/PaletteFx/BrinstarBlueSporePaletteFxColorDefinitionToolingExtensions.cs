namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Development-tool instance members of <see cref="BrinstarBlueSporePaletteFxColorDefinition"/>.</summary>
internal static class BrinstarBlueSporePaletteFxColorDefinitionToolingExtensions
{
    /// <summary>Development-tool instance members of <see cref="BrinstarBlueSporePaletteFxColorDefinition"/>.</summary>
    extension(BrinstarBlueSporePaletteFxColorDefinition self)
    {
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.Rows.Color(frame, color);
    }
}
