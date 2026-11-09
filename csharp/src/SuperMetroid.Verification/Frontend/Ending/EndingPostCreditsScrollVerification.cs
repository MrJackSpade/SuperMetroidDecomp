using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
namespace SuperMetroid.Verification.Frontend.Ending;
using SuperMetroid.Verification.Harness;

/// <summary>Checks how far the post-credits text is moved down through the production white-flash reset and item-percentage move down.</summary>
internal static class EndingPostCreditsScrollVerification
{
    /// <summary>Function 148's call count: forty two-pixel steps move the text eighty pixels down.</summary>
    private const int ScrollDownCalls = 40;
    /// <summary>Pixels the text moves down on each move-down call.</summary>
    private const int PixelsPerCall = 2;
    /// <summary>Bounds the post-shot hold so a broken handoff fails instead of looping.</summary>
    private const int MaximumShotCalls = 1000;

    /// <summary>
    /// Drives <see cref="EndingCreditsState.Step"/> through the post-shot handoff, whose white flash puts the text back at
    /// the top, then through every <see cref="EndingCreditsPhase.ItemPercentageScrollDown"/> call. The text moves down two
    /// pixels a call, 2, 4, ... 80, an offset with no wrap (#1555).
    /// </summary>
    internal static void VerifyEndingPostCreditsScroll()
    {
        EndingCreditsState ending = CreateRetailEndingFixture(AudioQueues.Standalone(), gameTimeHours: 2, gameTimeMinutes: 0);
        EndingPostCreditsText text = ending.InstalledArtwork.PostCreditsText;

        // The post-shot is reached only after the reward has shown the producer panel and replaced it with the
        // copyright, which the white flash then clears; the production steps show that text before the shot begins.
        ending.Stage.Phase = EndingCreditsPhase.PostCreditsWaitingBackdrop;
        ending.Stage.PhaseTimer = 1;
        ending.Step();
        AssertEqual(EndingCreditsPhase.PostCreditsWaitingSamus, ending.Stage.Phase, "the waiting backdrop shows the producer panel");
        AssertEqual(EndingPostCreditsPanel.Result, text.Panel, "the waiting backdrop's end shows the result panel");
        ending.CreditsRoll.ShowRewardCopyright();
        AssertEqual(EndingPostCreditsPanel.Copyright, text.Panel, "the reward replaces the result panel with the copyright");

        // A leftover drop from an earlier move down must not survive the white flash.
        int leftover = ScrollDownCalls * PixelsPerCall;
        text.Drop = leftover;
        ending.CreditsRoll.CurrentPostShot = new EndingPostShot();
        ending.CreditsRoll.CurrentRewardJump = new EndingRewardJump(ending.Stage.EndingReward);
        ending.Stage.Phase = EndingCreditsPhase.PostCreditsShot;
        for (int call = 0; ending.Stage.Phase == EndingCreditsPhase.PostCreditsShot; call++)
        {
            if (call == MaximumShotCalls)
                throw new InvalidOperationException("The post-shot owner never handed off to the white flash.");
            AssertEqual(leftover, text.Drop, "the drop is untouched until the white flash begins");
            ending.Step();
        }
        AssertEqual(EndingCreditsPhase.PostCreditsWhiteFlash, ending.Stage.Phase, "the post-shot owner hands off to the white flash");
        AssertEqual(0, text.Drop, "the white flash puts the text back at the top");
        AssertEqual(EndingPostCreditsPanel.None, text.Panel, "the white flash clears the copyright");

        ending.Stage.Phase = EndingCreditsPhase.ItemPercentageScrollDown;
        ending.Stage.PhaseTimer = ScrollDownCalls;
        for (int call = 1; call <= ScrollDownCalls; call++)
        {
            ending.Step();
            AssertEqual(call * PixelsPerCall, text.Drop, $"move-down call {call} moves the text {call * PixelsPerCall} pixels down");
        }
        AssertEqual(EndingCreditsPhase.SeeYouNextMission, ending.Stage.Phase, "the last move-down call starts the closing message");
        AssertTrue(text.FinalMessage is not null, "the last move-down call starts the final message");
        Console.WriteLine($"Ending post-credits text: the white flash puts it at the top and {ScrollDownCalls} move-down calls move it 2 through 80 pixels down.");
    }

    /// <summary>Runs the focused post-credits text move-down regression.</summary>
    [VerificationCommand("--ending-post-credits-scroll")]
    internal static int EndingPostCreditsScrollCommand()
    {
        VerifyEndingPostCreditsScroll();
        return 0;
    }
}
