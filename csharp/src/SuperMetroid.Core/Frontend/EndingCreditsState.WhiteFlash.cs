using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    /// <summary>VRAM word address receiving the currently active post-credits tilemap.</summary>
    private ushort postCreditsUploadWord = EndingCreditsRomData.Rendering.PostCreditsTilemapWord;
    /// <summary>Height in tilemap rows used when configuring the post-credits background screen.</summary>
    private int postCreditsMapHeight = 32;
    /// <summary>White component applied during the transition from the final text screen to the logo scene.</summary>
    private byte whiteFlashColor;
    /// <summary>Logo choreography instance advanced after the lower post-credits screen is prepared.</summary>
    private EndingLogo? endingLogo;

    /// <summary>Restores the final post-credits palette and prepares the two-screen text map for the white flash.</summary>
    private void BeginPostCreditsWhiteFlash()
    {
        postShot = null;
        // E48A restores the shooting palette and selects BG1SC=$4E: the final
        // text map is two screens tall, not the credits' single-screen map.
        LoadStaticPalette(EndingPaletteId.PostCredits,
            EndingPostShotDefinitions.ShootingPaletteStart,
            EndingPostShotDefinitions.PaletteColors,
            EndingPostShotDefinitions.ShootingPaletteStart);
        postCreditsUploadWord = EndingPostShotDefinitions.FinalTextTilemapWord;
        postCreditsMapHeight = EndingPostShotDefinitions.FinalTextMapHeight;
        postCreditsVerticalScroll = 0;
        Array.Fill(postCreditsTilemap, EndingCreditsRomData.Rendering.BlankTile,
            EndingPostShotDefinitions.ClearedCopyrightStart, EndingPostShotDefinitions.ClearedCopyrightWords);
        UploadPostCreditsTilemap();
        whiteFlashColor = EndingPostShotDefinitions.WhiteComponent;
        phaseTimer = EndingPostShotDefinitions.FadeFrames;
        Phase = EndingCreditsPhase.PostCreditsWhiteFlash;
    }

    /// <summary>Copies the staged map to the lower screen and starts the ending-logo actors.</summary>
    private void FinishPostCreditsWhiteFlash()
    {
        // E504 copies the same staging map into the lower screen before the logo
        // actors enter. Subsequent percentage writes target the upper screen again.
        vram.ExecuteWordTransfer(postCreditsTilemap, EndingPostShotDefinitions.FinalLowerTilemapWord, 1);
        rewardJump = null;
        sprites.Clear();
        endingLogo = new EndingLogo(bus, cgram,
            () => paletteFx.SpawnDefinition(bus, EndingLogoDefinitions.LandingPaletteFx, 0),
            paletteArtwork);
        Phase = EndingCreditsPhase.PostCreditsLogo;
    }

    /// <summary>Advances the logo scene and starts item-percentage text after its actors complete.</summary>
    private void StepPostCreditsLogo()
    {
        endingLogo!.Step(cgram, objectArtwork is null
            ? null : EndingLogoInstructionDefinitions.ReadWord);
        if (!endingLogo.Completed) return;
        postCreditsText = new EndingBackgroundTextState(bus, postCreditsTilemap,
            EndingCreditsRomData.Instructions.ItemPercentageText, inventory, japaneseText,
            EndingPostShotDefinitions.FinalTextTilemapWord,
            endingText,
            endingText is null ? null : SuperMetroid.Core.Assets.EndingTextSequence.ItemPercentage);
        Phase = EndingCreditsPhase.ItemPercentage;
    }
}
