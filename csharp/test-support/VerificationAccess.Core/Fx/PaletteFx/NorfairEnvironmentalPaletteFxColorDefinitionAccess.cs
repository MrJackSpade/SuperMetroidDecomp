namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="NorfairEnvironmentalPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class NorfairEnvironmentalPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes <see cref="NorfairEnvironmentalPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(NorfairEnvironmentalPaletteFxColorDefinition self)
    {
        /// <summary>Bytes of the heat-phase publication before each record's timer word; zero outside the heat program.</summary>
        public int HeatPublicationByteCount => self.PublishesHeatPhase ? 3 : 0;
        /// <summary>Bytes from one record's first command through its wait command.</summary>
        public int FrameByteCount => 16 + self.HeatPublicationByteCount;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.Rows.Color(frame, color);
    }
}
