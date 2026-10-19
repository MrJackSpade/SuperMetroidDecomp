namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="ZebesExplosionAmbientPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class ZebesExplosionAmbientPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes <see cref="ZebesExplosionAmbientPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(ZebesExplosionAmbientPaletteFxColorDefinition self)
    {
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.Rows.Color(frame, color);
    }
}
