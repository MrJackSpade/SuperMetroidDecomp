namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="NorfairEnvironmentalPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class NorfairEnvironmentalPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes <see cref="NorfairEnvironmentalPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(NorfairEnvironmentalPaletteFxColorDefinition self)
    {
        /// <summary>Bytes from one record's first mechanic through its wait command.</summary>
        public int FrameByteCount => self.PublishesHeatPhase ? 19 : 16;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.Rows.Color(frame, color);
    }
}
