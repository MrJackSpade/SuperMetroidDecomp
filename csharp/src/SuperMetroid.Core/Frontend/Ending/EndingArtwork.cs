using System.Diagnostics.CodeAnalysis;
﻿using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's installed artwork: its text, font, staff credits, Mode 7, object and palette assets and their loads.</summary>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingArtwork
{
    /// <summary>The ending's screen: its palette and the graphics transfers its scenes have made (#1358).</summary>
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

    /// <summary>Whether the credits and reward art have been transferred to VRAM.</summary>
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


    /// <summary>Rebinds host-owned staff credits after catalog reload or restoration.</summary>
    public void BindStaffCredits(CreditsPresentation? value)
    {
        StaffCredits = value;
        CurrentCredits?.BindPresentation(value);
    }


    /// <summary>Reapplies the current ending OBJ uploads after a state restore.</summary>
    internal void BindObjectArtwork(EndingObjectArtworkCatalog? value)
    {
        _stage.ObjectArtwork = value;
        screen.Invalidate();
    }

    /// <summary>Transfers the cloud object's character sheet into the ending OBJ lane.</summary>
    internal void LoadEscapeCloudCharacters()
    {
        byte[] clouds = (_stage.ObjectArtwork ?? throw new InvalidOperationException(
            "Escape clouds require installed object artwork.")).Clouds.Characters.ToArray();
        EndingStage.RequireMinimum(clouds, EndingCreditsRomData.Rendering.FloorBytes, "escape cloud characters");
        screen.Record(EndingUploadKind.EscapeClouds, 0);
    }

    /// <summary>Uploads explosion OBJ art, four fragments and the ending font at their native VRAM destinations.</summary>
    internal void LoadEndingObjectCharacters()
    {
        byte[] main = (_stage.ObjectArtwork ?? throw new InvalidOperationException(
            "Ending objects require installed artwork.")).Explosion.Characters.ToArray();
        // $8B:D8C1 uploads the complete explosion object sheet from $7F:8000.
        EndingStage.RequireMinimum(main, EndingCreditsRomData.Rendering.ExplosionObjectBytes,
            "ending OBJ characters");
        screen.Record(EndingUploadKind.ExplosionObjects, 0);
        LoadObjectFragment(EndingObjectFragmentId.Segment70);
        LoadObjectFragment(EndingObjectFragmentId.Segment74);
        LoadObjectFragment(EndingObjectFragmentId.Segment78);
        LoadObjectFragment(EndingObjectFragmentId.Segment7C);
        ReadOnlyMemory<byte> font = _stage.ResolveEndingFont().Characters;
        EndingStage.RequireMinimum(font, EndingCreditsRomData.Rendering.ObjectFragmentLimit,
            "ending font characters");
        screen.Record(EndingUploadKind.EndingFont, 0);
    }

    /// <summary>Copies one authored ending OBJ fragment into its named VRAM lane.</summary>
    private void LoadObjectFragment(EndingObjectFragmentId fragmentId)
    {
        byte[] fragment = (_stage.ObjectArtwork ?? throw new InvalidOperationException(
            "Ending object fragments require installed artwork.")).Fragment(fragmentId).Characters.ToArray();
        EndingStage.RequireMinimum(fragment, EndingCreditsRomData.Rendering.ObjectFragmentBytes,
            "ending OBJ fragment");
        screen.Record(EndingUploadKind.ObjectFragment, (int)fragmentId);
    }

    /// <summary>Performs the one-time palette, font and post-credits art transfers at the staff handoff.</summary>
    internal void LoadCreditsAndPostCreditsAssets()
    {
        if (creditsAssetsLoaded)
            return;

        _stage.LoadStaticPalette(EndingPaletteId.Credits, screen.Palette.Background);
        ReadOnlyMemory<byte> font = _stage.ResolveEndingFont().Characters;
        EndingStage.RequireMinimum(font, EndingCreditsRomData.Rendering.FontCharacterBytes,
            "credits font");

        screen.Clear();
        screen.Record(EndingUploadKind.CreditsFont, 0);
        LoadCreditsCharacterArt();

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

    /// <summary>Transfers waiting, shooting and selected reward art plus their post-credits map fragments.</summary>
    private void LoadCreditsCharacterArt()
    {
        EndingObjectArtworkCatalog artwork = _stage.ObjectArtwork ?? throw new InvalidOperationException(
            "Post-credits scenes require installed object artwork.");
        byte[] waiting = artwork.WaitingSamus.Characters.ToArray();
        byte[] shooting = artwork.ShootingScreen.Characters.ToArray();
        byte[] waitingMap = artwork.WaitingTilemap.Bytes.ToArray();
        byte[] fragmentA = artwork.PostCreditsFragmentA.Characters.ToArray();
        byte[] fragmentB = artwork.PostCreditsFragmentB.Characters.ToArray();
        byte[] result = _stage.EndingReward == EndingReward.Suitless
            ? artwork.SuitlessSamus.Characters.ToArray()
            : waiting;
        EndingStage.RequireMinimum(waiting, EndingCreditsRomData.Rendering.FloorBytes,
            "waiting-Samus characters");
        EndingStage.RequireMinimum(shooting, EndingCreditsRomData.Rendering.ShootingCharacterBytes,
            "post-credits OBJ characters");
        EndingStage.RequireMinimum(waitingMap, EndingCreditsRomData.Rendering.WaitingTilemapBytes,
            "waiting-Samus tilemap");
        EndingStage.RequireMinimum(fragmentA, EndingCreditsRomData.Rendering.PostCreditsFragmentABytes,
            "post-credits tile fragment A");
        EndingStage.RequireMinimum(fragmentB, EndingCreditsRomData.Rendering.ObjectFragmentBytes,
            "post-credits tile fragment B");
        EndingStage.RequireMinimum(result, EndingCreditsRomData.Rendering.FloorBytes,
            "post-credits reward characters");
        // Function 126 selects suitless (<3h) or suited (>=3h) art; the
        // latter deliberately reuses the waiting-scene sheet, not Mode-7 art.
        screen.Record(EndingUploadKind.CreditsCharacterArt, 0);
    }

    /// <summary>
    /// Rebinds authored ending colors after state restoration. The current CGRAM image
    /// remains serialized live state: overwriting it here would discard a partially
    /// completed native fade or palette-FX step. Future native palette transfers and
    /// target-color reads use the newly selected resource.
    /// </summary>
    internal void BindPaletteArtwork(EndingPaletteCatalog? value)
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

    /// <summary>The staff credits' installed content.</summary>
    [field: NonSerialized]
    internal CreditsPresentation? StaffCredits { get; private set; }

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
