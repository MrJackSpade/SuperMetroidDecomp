namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    /// <summary>Replaces the reward panel with the installed copyright tilemap and starts its timed post-credits phase.</summary>
    private void ShowRewardCopyright()
    {
        // Func135 removes the producer panel and replaces rows twelve/thirteen with
        // the literal DEDB copyright tilemap. BG1 alone is visible until Func137 ends.
        Array.Fill(postCreditsTilemap, EndingCreditsRomData.Rendering.BlankTile,
            EndingCreditsRomData.Text.ResultPanelDestination, EndingCreditsRomData.Text.ResultPanelWords);
        (endingText ?? throw new InvalidOperationException(
            "Ending copyright requires installed text assets.")).BuildCopyrightPanel().CopyTo(
            postCreditsTilemap, EndingCreditsRomData.Text.CopyrightPanelDestination);
        UploadPostCreditsTilemap();
        rewardCopyrightShown = true;
        phaseTimer = EndingCreditsRomData.Timing.CopyrightHoldFrames;
        Phase = EndingCreditsPhase.PostCreditsCopyright;
    }
}
