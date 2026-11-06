using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's installed artwork: its text, font, staff credits, turning floor, object and palette assets and their loads.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingArtwork
{
    /// <summary>The ending's screen: its palette, the pictures its scenes have loaded and its text pages (#1358, #1467).</summary>
    private readonly EndingScreen screen ;
    /// <summary>What every ending scene draws on: brightness, the phase timer, the turning-floor view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingArtwork(EndingScreen screen, EndingStage stage)
    {
        this.screen = screen;
        _stage = stage;
    }

    /// <summary>Scrolling staff-list owner active during the credits phase.</summary>
    internal CreditsObjectState? CurrentCredits { get; set; }

    /// <summary>Whether the credits and reward art have been loaded.</summary>
    private bool creditsAssetsLoaded;

    /// <summary>Binds the installed sprite sheets when the game is bound to its installed content.</summary>
    internal void BindSpriteSheets(InstalledSpriteSheets? value) => Sheets = value;

    /// <summary>The installed sheet <paramref name="sheet"/>.</summary>
    internal SpriteSheet SpriteSheet(SpriteSheetId sheet) => Sheets.Get(sheet);

    /// <summary>The installed sprite sheets the ending's actors and backgrounds draw from (#1392).</summary>
    [AllowNull]
    internal InstalledSpriteSheets Sheets
    {
        get => field ?? throw new InvalidOperationException("Ending sprites require installed sprite sheets.");
        private set;
    }

    /// <summary>Binds the installed ending text when the game is bound to its installed content.</summary>
    public void BindEndingText(EndingTypewriterText? value)
    {
        EndingText = value;
        ActivePostCreditsText?.BindPresentation(value);
    }


    /// <summary>Binds the ending's installed object artwork when the game is bound to its installed content.</summary>
    internal void BindObjectArtwork(EndingObjectArtwork? value)
    {
        _stage.ObjectArtwork = value;
    }

    /// <summary>Requires the escape clouds' installed artwork, which the clouds' sheet is composed from (#1463, #1467).</summary>
    internal void RequireEscapeCloudArtwork() =>
        _ = _stage.ObjectArtwork ?? throw new InvalidOperationException("Escape clouds require installed object artwork.");

    /// <summary>Loads the explosion's pictures, which show its objects, its four maps and the ending font.</summary>
    internal void LoadExplosionPictures()
    {
        _ = _stage.ObjectArtwork ?? throw new InvalidOperationException("Ending objects require installed artwork.");
        _ = _stage.ResolveEndingFont();
        screen.LoadExplosionMaps();
    }

    /// <summary>Performs the one-time palette and post-credits art loads at the staff handoff, after a full clear.</summary>
    internal void LoadCreditsAndPostCreditsAssets()
    {
        if (creditsAssetsLoaded)
            return;

        _stage.LoadColours(EndingColourLoad.CreditsBackgrounds);
        _ = _stage.ResolveEndingFont();
        _ = _stage.ObjectArtwork ?? throw new InvalidOperationException("Post-credits scenes require installed object artwork.");
        screen.Clear();
        // Function 126 selects suitless (<3h) or suited (>=3h) reward art; the installed reward sheets hold both.
        screen.LoadPostCreditsArt();

        creditsAssetsLoaded = true;
    }

    /// <summary>Loads the reward icon's floor, which the reward jump reveals as Samus falls back to her landing.</summary>
    internal void LoadRewardIcon() => screen.LoadRewardIcon();

    /// <summary>
    /// Binds authored ending colors when the game is bound to its installed content. The current colour image
    /// remains live state: overwriting it here would discard a partially
    /// completed fade or palette-FX step. Future palette loads and
    /// target-color reads use the newly selected resource.
    /// </summary>
    internal void BindPaletteArtwork(EndingPaletteSet? value)
    {
        _stage.BindPaletteArtwork(value);
    }

    /// <summary>
    /// Uses the same editable palette-FX color payloads as gameplay while leaving
    /// ending-specific object allocation, palette destinations and timing as the game has them.
    /// Rebinding affects future interpreter reads without resetting a live fade.
    /// </summary>
    internal void BindPaletteFxColors(RoomPaletteFxPresentation? value)
    {
        PaletteFxArtwork = value;
    }

    /// <summary>Final logo actor created after the white flash.</summary>
    internal EndingLogo? CurrentEndingLogo { get; set; }

    /// <summary>The palette effects' installed colors.</summary>
    internal RoomPaletteFxPresentation? PaletteFxArtwork { get; private set; }

    /// <summary>The ending text's installed content.</summary>
    internal EndingTypewriterText? EndingText { get; private set; }

    /// <summary>The post-credits text, once it starts.</summary>
    internal EndingBackgroundTextState? ActivePostCreditsText { get; private set; }

    /// <summary>Starts the post-credits text the credits roll hands over to.</summary>
    internal void StartPostCreditsText(EndingBackgroundTextState text) => ActivePostCreditsText = text;
}
