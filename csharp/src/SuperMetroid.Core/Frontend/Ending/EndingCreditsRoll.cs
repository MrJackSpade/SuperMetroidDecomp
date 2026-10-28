using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The staff credits and what follows them: the reward copyright and palette, the white flash and the post-credits logo.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingCreditsRoll
{
    /// <summary>Collected-item snapshot displayed with the final result.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Chooses the localized post-credits text asset.</summary>
    private readonly bool japaneseText;
    /// <summary>The ending's screen: its palette and the graphics transfers its scenes have made (#1358).</summary>
    private readonly EndingScreen screen ;
    /// <summary>Dynamic cinematic actors in native slot traversal order.</summary>
    private readonly List<EndingSprite> sprites ;
    /// <summary>Staging copy of the two-screen post-credits result tilemap.</summary>
    private readonly BackgroundTile[] postCreditsTilemap ;
    /// <summary>What every ending scene draws on: brightness, the phase timer, the Mode 7 view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    private readonly EndingStage _stage;
    /// <summary>The ending's installed artwork: its text, font, staff credits, Mode 7, object and palette assets and their loads.</summary>
    private readonly EndingArtwork _artwork;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal EndingCreditsRoll(EndingInventorySnapshot inventory, bool japaneseText, EndingScreen screen, List<EndingSprite> sprites, BackgroundTile[] postCreditsTilemap, EndingStage stage, EndingArtwork artwork)
    {
        this.inventory = inventory;
        this.japaneseText = japaneseText;
        this.screen = screen;
        this.sprites = sprites;
        this.postCreditsTilemap = postCreditsTilemap;
        _stage = stage;
        _artwork = artwork;
    }

    /// <summary>Background star animation retained into the post-credits scene.</summary>
    internal EndingShootingStars? ShootingStars { get; set; }

    /// <summary>Post-credits reward jump and landing actor.</summary>
    internal EndingRewardJump? CurrentRewardJump { get; set; }

    /// <summary>Final shot-into-camera actor and its drawing state.</summary>
    internal EndingPostShot? CurrentPostShot { get; set; }

    /// <summary>Replaces planet actors with the staff scroll, shooting stars and blank reward staging map.</summary>
    internal void SetupCredits()
    {
        ShootingStars = new EndingShootingStars();
        // Func126 clears palette objects before installing credits/reward palettes.
        _stage.ResetPaletteFx();
        _artwork.LoadCreditsAndPostCreditsAssets();
        _artwork.CurrentCredits = new CreditsObjectState(_artwork.StaffCredits ?? throw new InvalidOperationException(
            "Ending credits require installed ending-credits.json content."));
        _artwork.CurrentCredits.UploadTilemap(screen);
        Array.Fill(postCreditsTilemap, EndingScreenDefinitions.BlankTile);
        sprites.Clear();
        _stage.Brightness = 15;
        _stage.Phase = EndingCreditsPhase.Credits;
    }

    /// <summary>Restores the post-credits palette except colors zero through three and holds a blank screen for sixty calls.</summary>
    internal void SetupPostCreditsBlank()
    {
        // F6FE copies Intro4 colors $04-$FF, disables text glow, forces blank, and arms
        // function 129 for sixty calls. Preserve colors zero through three as native does.
        _stage.LoadStaticPalette(EndingPaletteId.PostCredits, screen.Palette.All.Slice(EndingColourRowDefinitions.PostCreditsKeptColours,
            ScreenPalette.ColorCount - EndingColourRowDefinitions.PostCreditsKeptColours));
        _stage.Brightness = 0;
        UploadPostCreditsTilemap();
        _stage.PhaseTimer = 60;
        _stage.Phase = EndingCreditsPhase.PostCreditsBlank;
    }

    /// <summary>Copies the current result-panel staging tiles to the phase-selected tilemap destination.</summary>
    internal void UploadPostCreditsTilemap() =>
        screen.Record(PostCreditsTextTilemap, postCreditsTilemap);

    /// <summary>Replaces the producer result rows with the copyright panel and starts its authored hold.</summary>
    internal void ShowRewardCopyright()
    {
        // Func135 removes the producer panel and replaces rows twelve/thirteen with
        // the literal DEDB copyright tilemap. BG1 alone is visible until Func137 ends.
        Array.Fill(postCreditsTilemap, EndingScreenDefinitions.BlankTile,
            EndingResultTextLayout.ResultPanelFirstCell, EndingResultTextLayout.ResultPanelWords);
        (_artwork.EndingText ?? throw new InvalidOperationException(
            "Ending copyright requires installed text assets.")).BuildCopyrightPanel().CopyTo(
            postCreditsTilemap, EndingResultTextLayout.CopyrightPanelFirstCell);
        UploadPostCreditsTilemap();
        RewardCopyrightShown = true;
        _stage.PhaseTimer = EndingSequenceDefinitions.Timing.CopyrightHoldFrames;
        _stage.Phase = EndingCreditsPhase.PostCreditsCopyright;
    }

    /// <summary>Reads one authored palette target without replacing the current fade state.</summary>
    private RgbColor StaticPaletteColor(EndingPaletteId id, ColourRun positions, int colour)
        => (_stage.PaletteArtwork ?? throw new InvalidOperationException(
            "Ending palettes require installed artwork."))[id].Color(positions, colour);

    /// <summary>Fades the post-credits waiting backdrop toward its authored palette.</summary>
    internal void ApplyWaitingBackdropPalette(int step)
        => ApplyPostCreditsPaletteRange(screen.Palette.BackgroundRow(EndingColourRowDefinitions.WaitingBackgroundRow), step);

    /// <summary>Interpolation step for suitless or helmetless reward colors.</summary>
    private int rewardPaletteStep;

    /// <summary>Crossfades waiting colors into the selected non-armored reward palette.</summary>
    internal void ApplyRewardPalette()
    {
        if (_stage.EndingReward == EndingReward.Armored) return;
        ApplyWaitingBackdropPalette(EndingSequenceDefinitions.Timing.WaitingPaletteFadeFrames - rewardPaletteStep);
        ApplyPostCreditsPaletteRange(screen.Palette.SpriteRow(EndingColourRowDefinitions.SuitlessRewardSpriteRow), rewardPaletteStep);
        if (_stage.EndingReward == EndingReward.Helmetless)
            ApplyPostCreditsPaletteRange(screen.Palette.SpriteRow(EndingColourRowDefinitions.SuitedRewardSpriteRow), rewardPaletteStep);
    }

    /// <summary>Fades one post-credits row toward its authored colours with 8.8 component truncation.</summary>
    private void ApplyPostCreditsPaletteRange(ColourRun row, int step)
    {
        // E110 saves Intro4 as its target, clears colors $20-$2F, then E158 runs the
        // shared 8CB2 component accumulator 32 times. Each increment is component<<3
        // in 8.8 precision; composing takes only its high byte, preserving truncation.
        for (int colour = 0; colour < row.Count; colour++)
        {
            RgbColor target = StaticPaletteColor(EndingPaletteId.PostCredits, row, colour);
            row.Write(colour, target.Map((_, channel) => EightEight.Multiply(channel << 3, step)));
        }
    }

    /// <summary>Background map the post-credits staging tilemap is uploaded into, switched by the white flash.</summary>
    internal EndingTextTilemap PostCreditsTextTilemap { get; private set; } = EndingTextTilemap.ResultPanel;

    /// <summary>Visible result-panel map height, enlarged for the final shot scene.</summary>
    internal int PostCreditsMapHeight { get; private set; } = 32;

    /// <summary>Fixed-color white component of the brief transition into the final logo, fading from 31 to zero, where it stays.</summary>
    internal int WhiteFlashColor { get; set; }

    /// <summary>The state of the ending logo, which exists while that phase runs.</summary>
    internal EndingLogo EndingLogo => _artwork.CurrentEndingLogo ??
        throw new InvalidOperationException("The ending logo is not running.");

    /// <summary>Restores the shooting palette, switches to the tall text map and begins the white transition.</summary>
    internal void BeginPostCreditsWhiteFlash()
    {
        CurrentPostShot = null;
        // E48A restores the shooting palette and selects BG1SC=$4E: the final
        // text map is two screens tall, not the credits' single-screen map.
        _stage.LoadStaticPalette(EndingPaletteId.PostCredits, screen.Palette.SpriteRow(EndingColourRowDefinitions.ShootingSpriteRow));
        PostCreditsTextTilemap = EndingTextTilemap.FinalText;
        PostCreditsMapHeight = EndingPostShotDefinitions.FinalTextMapHeight;
        _stage.PostCreditsScroll = Position16<TilePlanePixels>.Zero;
        Array.Fill(postCreditsTilemap, EndingScreenDefinitions.BlankTile,
            EndingPostShotDefinitions.ClearedCopyrightStart, EndingPostShotDefinitions.ClearedCopyrightWords);
        UploadPostCreditsTilemap();
        WhiteFlashColor = EndingPostShotDefinitions.WhiteComponent;
        _stage.PhaseTimer = EndingPostShotDefinitions.FadeFrames;
        _stage.Phase = EndingCreditsPhase.PostCreditsWhiteFlash;
    }

    /// <summary>Copies the staged lower tilemap and installs the final logo actor.</summary>
    internal void FinishPostCreditsWhiteFlash()
    {
        // E504 copies the same staging map into the lower screen before the logo
        // actors enter. Subsequent percentage writes target the upper screen again.
        screen.Record(EndingUploadKind.PostShotLowerTilemap, postCreditsTilemap);
        CurrentRewardJump = null;
        sprites.Clear();
        _artwork.CurrentEndingLogo = new EndingLogo(screen.Palette,
            () => _stage.PaletteFxSystem.SpawnDefinition(EndingLogoDefinitions.LandingPaletteFx, 0),
            _stage.PaletteArtwork);
        _stage.Phase = EndingCreditsPhase.PostCreditsLogo;
    }

    /// <summary>Advances logo instructions and starts the item-percentage text once the logo completes.</summary>
    internal void StepPostCreditsLogo()
    {
        EndingLogo.Step(screen.Palette);
        if (!EndingLogo.Completed) return;
        _artwork.StartPostCreditsText(new EndingBackgroundTextState(new EndingBackgroundTextSetup { Tilemap = postCreditsTilemap, Inventory = inventory, JapaneseText = japaneseText, DestinationTilemap = EndingTextTilemap.FinalText, Presentation = _artwork.EndingText, InstalledSequence = _artwork.EndingText is null ? null : SuperMetroid.Core.Frontend.Ending.Artwork.EndingTextSequence.ItemPercentage }));
        _stage.Phase = EndingCreditsPhase.ItemPercentage;
    }

    /// <summary>Whether the reward copyright is showing.</summary>
    internal bool RewardCopyrightShown { get; private set; }

    /// <summary>Starts the reward palette from its first step.</summary>
    internal void ResetRewardPalette() => rewardPaletteStep = 0;

    /// <summary>Moves the reward palette to its next step.</summary>
    internal void AdvanceRewardPalette() => rewardPaletteStep++;
}
