namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private void ShowRewardCopyright()
    {
        // Func135 removes the producer panel and replaces rows twelve/thirteen with
        // the literal DEDB copyright tilemap. BG1 alone is visible until Func137 ends.
        Array.Fill(postCreditsTilemap, EndingCreditsRomData.Rendering.BlankTile,
            EndingCreditsRomData.Text.ResultPanelDestination, EndingCreditsRomData.Text.ResultPanelWords);
        CopyPostCreditsWords(EndingCreditsRomData.Instructions.CopyrightPanel,
            EndingCreditsRomData.Text.CopyrightPanelDestination,
            EndingCreditsRomData.Text.CopyrightPanelWords);
        UploadPostCreditsTilemap();
        rewardCopyrightShown = true;
        phaseTimer = EndingCreditsRomData.Timing.CopyrightHoldFrames;
        Phase = EndingCreditsPhase.PostCreditsCopyright;
    }
}
