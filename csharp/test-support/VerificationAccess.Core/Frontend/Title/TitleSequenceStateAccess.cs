using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Verification access to <see cref="TitleSequenceState"/> members production does not use.</summary>
internal static class TitleSequenceStateAccess
{
    /// <summary>Reads title display and demo-timer state for focused frontend verification.</summary>
    extension(TitleSequenceState self)
    {
        /// <summary>Current screen brightness level; zero is black and fifteen is full.</summary>
        internal byte Brightness => (byte)self.Brightness;

        /// <summary>Current title CGRAM, including cartridge palette-animation writes.</summary>
        [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
        internal ReadOnlySpan<Bgr555> PaletteColors => self.palette.Colors;

        /// <summary>Current native Mode-7 A/D scalar, exposed for transform regression audits.</summary>
        internal ushort Mode7MatrixScale => unchecked((ushort)self.Zoom);

        /// <summary>
        /// NTSC demo countdown. Retail initializes this to $0384 (900 frames); PAL uses $02D0
        /// so both revisions hold the title for approximately fifteen seconds.
        /// </summary>
        internal int TitleScreenFramesRemaining => self.Phase == TitleSequencePhase.TitleScreen ? self.PhaseTimer : 0;
    }
}
