namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="OldTourianEscapeAccentPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class OldTourianEscapeAccentPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes <see cref="OldTourianEscapeAccentPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(OldTourianEscapeAccentPaletteFxColorDefinition self)
    {
        /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
        public ushort LoopInstructionPointer => unchecked((ushort)(
            self.FirstFramePointer +
            OldTourianEscapeAccentPaletteFxColorDefinitions.FrameCount *
            OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.FrameByteCount));
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.Rows.Color(frame, color);
    }
}
