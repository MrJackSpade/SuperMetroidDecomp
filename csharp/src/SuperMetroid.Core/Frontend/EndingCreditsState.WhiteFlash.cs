namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private ushort postCreditsUploadWord = EndingCreditsRomData.Rendering.PostCreditsTilemapWord;
    private int postCreditsMapHeight = 32;
    private byte whiteFlashColor;

    private void BeginPostCreditsWhiteFlash()
    {
        postShot = null;
        // E48A restores the shooting palette and selects BG1SC=$4E: the final
        // text map is two screens tall, not the credits' single-screen map.
        cgram.LoadFromBus(bus, EndingCreditsRomData.Assets.PostCreditsPalette
            + EndingPostShotDefinitions.ShootingPaletteStart * sizeof(ushort),
            EndingPostShotDefinitions.PaletteColors, EndingPostShotDefinitions.ShootingPaletteStart);
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
        postCreditsText = new EndingBackgroundTextState(bus, postCreditsTilemap,
            EndingCreditsRomData.Instructions.ItemPercentageText, inventory, japaneseText,
            EndingPostShotDefinitions.FinalTextTilemapWord);
        Phase = EndingCreditsPhase.ItemPercentage;
    }
}
