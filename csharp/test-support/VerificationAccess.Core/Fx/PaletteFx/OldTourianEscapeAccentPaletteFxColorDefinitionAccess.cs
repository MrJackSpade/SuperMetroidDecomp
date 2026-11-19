using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="OldTourianEscapeAccentPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class OldTourianEscapeAccentPaletteFxColorDefinitionAccess
{
    /// <summary>
    /// What each program the cartridge holds that production does not read (#2570), by each program's owner: the checks compare
    /// each value against the cartridge.
    /// </summary>
    private static readonly Dictionary<OldTourianEscapeAccentPaletteOwner, ColourEffectId> CrossChecks = new()
    {
        [OldTourianEscapeAccentPaletteOwner.OrangeRailings] = ColourEffectId.OldTourianBackgroundRailingsFlashYellow,
        [OldTourianEscapeAccentPaletteOwner.YellowPanels] = ColourEffectId.OldTourianBackgroundPanelsFlashYellow,
    };

    /// <summary>Exposes <see cref="OldTourianEscapeAccentPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(OldTourianEscapeAccentPaletteFxColorDefinition self)
    {
        /// <summary>The palette-FX object that installs this program.</summary>
        internal ColourEffectId Object => CrossChecks[self.Owner];
        /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
        public ushort LoopInstructionPointer => unchecked((ushort)(
            self.FirstFramePointer +
            OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.FrameCount *
            OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.FrameByteCount));
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}
