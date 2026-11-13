namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Verification access to <see cref="TitleSequenceState"/> members production does not use.</summary>
internal static class TitleSequenceStateAccess
{
    /// <summary>Reads title display and demo-timer state for focused frontend verification.</summary>
    extension(TitleSequenceState self)
    {
        /// <summary>Current screen brightness level; zero is black and fifteen is full.</summary>
        internal byte Brightness => (byte)self.Brightness;

        /// <summary>The floor's current zoom, exposed for title regression checks.</summary>
        internal FloorZoom FloorZoom => self.Zoom;

        /// <summary>
        /// NTSC demo countdown. Retail initializes this to $0384 (900 frames); PAL uses $02D0
        /// so both revisions hold the title for approximately fifteen seconds.
        /// </summary>
        internal int TitleScreenFramesRemaining => self.Phase == TitleSequencePhase.TitleScreen ? self.PhaseTimer : 0;
    }
}
