using SuperMetroid.Core.Frontend.Title.Artwork;

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

        /// <summary>
        /// Shows <paramref name="sprite"/> or the typed card <paramref name="card"/> at <paramref name="origin"/> in
        /// <paramref name="colouring"/>, for a check that draws each composition the title can show.
        /// </summary>
        internal void ShowComposition(TitleSprite? sprite, TitleCardTyping? card, Offset16<ScreenPixels> origin, TitleSpriteColouring colouring)
        {
            PrivateFrontendState.Set(self, nameof(TitleSequenceState.ActiveSprite), sprite);
            PrivateFrontendState.Set(self, nameof(TitleSequenceState.ActiveCard), card);
            PrivateFrontendState.Set(self, nameof(TitleSequenceState.ActiveOrigin), origin);
            PrivateFrontendState.Set(self, nameof(TitleSequenceState.ActiveColouring), colouring);
        }

        /// <summary>Puts the title at its title screen with no composition or card shown, for a check of the copyright that screen draws.</summary>
        internal void ShowTitleScreenOnly()
        {
            PrivateFrontendState.Set(self, nameof(TitleSequenceState.ActiveSprite), null);
            PrivateFrontendState.Set(self, nameof(TitleSequenceState.ActiveCard), null);
            PrivateFrontendState.Set(self, nameof(TitleSequenceState.Phase), TitleSequencePhase.TitleScreen);
        }
    }
}
