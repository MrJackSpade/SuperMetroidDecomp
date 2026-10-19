namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="SamusLoadingSuitPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class SamusLoadingSuitPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes <see cref="SamusLoadingSuitPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(SamusLoadingSuitPaletteFxColorDefinition self)
    {
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.Rows.Color(frame, color);
    }
}
