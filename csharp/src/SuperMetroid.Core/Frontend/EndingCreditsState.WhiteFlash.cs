using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private ushort postCreditsUploadWord = EndingCreditsRomData.Rendering.PostCreditsTilemapWord;
    private int postCreditsMapHeight = 32;
    private byte whiteFlashColor;
    private EndingLogo? endingLogo;

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
