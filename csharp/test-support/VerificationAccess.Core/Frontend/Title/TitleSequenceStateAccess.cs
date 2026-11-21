using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Verification access to <see cref="TitleSequenceState"/> members production does not use.</summary>
internal static class TitleSequenceStateAccess
{
    /// <summary>The finest fraction of a floor pixel a zoom counts in: two hundred and fifty-sixths.</summary>
    private const int ZoomDenominator = 256;

    /// <summary>Reads title display and demo-timer state for focused frontend verification.</summary>
    extension(TitleSequenceState self)
    {

        /// <summary>
        /// The zoom of the floor the title shows, read from the scale of the floor in the scene it composes, or null while it
        /// shows no floor. Composing builds only the shown sprites, so it leaves the title's run as it was.
        /// </summary>
        internal FloorZoom? FloorZoom => self.ComposeScene().Floor is { } floor
            ? Spatial.FloorZoom.FromFloorPixelsPerScreenPixel(
                checked((int)Math.Round(floor.View.Scale * ZoomDenominator)), ZoomDenominator)
            : null;

        /// <summary>
        /// NTSC demo countdown. Retail initializes this to $0384 (900 frames); PAL uses $02D0
        /// so both revisions hold the title for approximately fifteen seconds.
        /// </summary>
        internal int TitleScreenFramesRemaining => self.Phase == TitleSequencePhase.TitleScreen ? self.IdleTimeout.UpdatesLeft : 0;

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
