using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's installed artwork: its text, font, staff credits, Mode 7, object and palette assets and their loads.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingArtwork
{
    /// <summary>The ending's screen: its palette, the pictures its scenes have loaded and its text pages (#1358, #1467).</summary>
    private readonly EndingScreen screen ;
    /// <summary>What every ending scene draws on: brightness, the phase timer, the Mode 7 view, palettes, the shared sprites and stars, and the artwork they share.</summary>
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

    /// <summary>Whether the reward landing has started queuing its icon chunks on the actor's native instruction boundary.</summary>
    private bool rewardIconUploading;

    /// <summary>Rebinds the installed sprite sheets after a catalog reload or state restoration.</summary>
    internal void BindSpriteSheets(InstalledSpriteSheets? value) => Sheets = value;

    /// <summary>The installed sheet <paramref name="sheet"/>.</summary>
    internal SpriteSheet SpriteSheet(SpriteSheetId sheet) => Sheets.Get(sheet);

    /// <summary>The installed sprite sheets the ending's actors and backgrounds draw from (#1392).</summary>
    [field: NonSerialized]
    [AllowNull]
    internal InstalledSpriteSheets Sheets
    {
        get => field ?? throw new InvalidOperationException("Ending sprites require installed sprite sheets.");
        private set;
    }

    /// <summary>Rebinds host-owned ending text after catalog reload or graph restoration.</summary>
    public void BindEndingText(EndingTextPresentation? value)
    {
        EndingText = value;
        ActivePostCreditsText?.BindPresentation(value);
    }


    /// <summary>Rebinds the ending's installed object artwork after a state restore.</summary>
    internal void BindObjectArtwork(EndingObjectArtwork? value)
    {
        _stage.ObjectArtwork = value;
        screen.Invalidate();
    }

    /// <summary>
    /// Loads the cloud objects; the installed sheet the clouds draw from holds their characters, which the import
    /// composes once (#1463, #1467).
    /// </summary>
    internal void LoadEscapeCloudCharacters() =>
        _ = _stage.ObjectArtwork ?? throw new InvalidOperationException("Escape clouds require installed object artwork.");

    /// <summary>Loads the explosion's objects, its four maps and the ending font, which the explosion's pictures and sheet show.</summary>
    internal void LoadEndingObjectCharacters()
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

        _stage.StaticScreen(EndingPaletteId.Credits).LoadBackgroundsTo(screen.Palette);
        _ = _stage.ResolveEndingFont();
        _ = _stage.ObjectArtwork ?? throw new InvalidOperationException("Post-credits scenes require installed object artwork.");
        screen.Clear();
        // Function 126 selects suitless (<3h) or suited (>=3h) reward art; the installed reward sheets hold both.
        screen.LoadPostCreditsArt();

        creditsAssetsLoaded = true;
    }

    /// <summary>Uploads the indexed reward fragment requested by the active landing actor.</summary>
    internal void UploadRewardGraphic(int index)
    {
        if (!rewardIconUploading)
            throw new InvalidOperationException("Reward landing has no active graphics upload.");
        if ((uint)index >= EndingRewardJumpDefinitions.IconChunkCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        screen.Record(EndingUploadKind.RewardChunk, index);
    }

    /// <summary>
    /// Rebinds authored ending colors after state restoration. The current CGRAM image
    /// remains serialized live state: overwriting it here would discard a partially
    /// completed native fade or palette-FX step. Future native palette transfers and
    /// target-color reads use the newly selected resource.
    /// </summary>
    internal void BindPaletteArtwork(EndingPaletteSet? value)
    {
        _stage.BindPaletteArtwork(value);
        CurrentEndingLogo?.BindPaletteArtwork(value);
    }

    /// <summary>
    /// Uses the same editable bank-$8D color payloads as gameplay while leaving
    /// ending-specific object allocation, palette destinations and timing native.
    /// Rebinding affects future interpreter reads without resetting a live fade.
    /// </summary>
    internal void BindPaletteFxColors(RoomPaletteFxPresentation? value)
    {
        PaletteFxArtwork = value;
    }

    /// <summary>Final logo actor created after the white flash.</summary>
    internal EndingLogo? CurrentEndingLogo { get; set; }

    /// <summary>The palette effects' installed colors.</summary>
    [field: NonSerialized]
    internal RoomPaletteFxPresentation? PaletteFxArtwork { get; private set; }

    /// <summary>The ending text's installed content.</summary>
    [field: NonSerialized]
    internal EndingTextPresentation? EndingText { get; private set; }

    /// <summary>The post-credits text, once it starts.</summary>
    internal EndingBackgroundTextState? ActivePostCreditsText { get; private set; }

    /// <summary>Marks the reward icon as uploading, so the reward graphic streams in.</summary>
    internal void BeginRewardIconUpload() => rewardIconUploading = true;

    /// <summary>Starts the post-credits text the credits roll hands over to.</summary>
    internal void StartPostCreditsText(EndingBackgroundTextState text) => ActivePostCreditsText = text;
}
