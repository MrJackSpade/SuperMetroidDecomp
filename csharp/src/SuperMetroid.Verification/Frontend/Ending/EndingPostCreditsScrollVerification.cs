using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Frontend.Ending;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the post-credits text map's scroll through the production white-flash reset and item-percentage scroll-down.</summary>
internal static class EndingPostCreditsScrollVerification
{
    /// <summary>Function 148's call count: forty two-pixel steps move the map from zero to -80.</summary>
    private const int ScrollDownCalls = 40;
    /// <summary>Pixels the map moves down on each scroll-down call.</summary>
    private const int PixelsPerCall = 2;
    /// <summary>One full turn of the sixteen-bit scroll word, the value a negative scroll wraps by.</summary>
    private const int ScrollWordRange = 65536;
    /// <summary>Bounds the post-shot hold so a broken handoff fails instead of looping.</summary>
    private const int MaximumShotCalls = 1000;

    /// <summary>
    /// Drives <see cref="EndingCreditsState.Step"/> through the post-shot handoff, whose white flash resets the
    /// scroll to zero, then through every <see cref="EndingCreditsPhase.ItemPercentageScrollDown"/> call. The scroll
    /// must wrap below zero exactly as the native sixteen-bit word did: 65534, 65532, ... 65456, with X unchanged.
    /// </summary>
    internal static void VerifyEndingPostCreditsScroll()
    {
        EndingCreditsState ending = CreateRetailEndingFixture(new CartridgeAudioState(), gameTimeHours: 2, gameTimeMinutes: 0);
        var installation = RepositoryInstallation.Installation;

        // A leftover scroll from an earlier scroll-down must not survive the white flash.
        var leftover = new Position16<BgPlanePixels>(0, ScrollWordRange - ScrollDownCalls * PixelsPerCall);
        ending.postCreditsScroll = leftover;
        ending.postShot = new EndingPostShot(ending.cgram, installation.LoadMaps().EndingFont, installation.LoadEndingObjectArt());
        ending.rewardJump = new EndingRewardJump(ending.EndingReward, _ => { });
        ending.Phase = EndingCreditsPhase.PostCreditsShot;
        for (int call = 0; ending.Phase == EndingCreditsPhase.PostCreditsShot; call++)
        {
            if (call == MaximumShotCalls)
                throw new InvalidOperationException("The post-shot owner never handed off to the white flash.");
            AssertEqual(leftover, ending.postCreditsScroll, "the scroll is untouched until the white flash begins");
            ending.Step();
        }
        AssertEqual(EndingCreditsPhase.PostCreditsWhiteFlash, ending.Phase, "the post-shot owner hands off to the white flash");
        AssertEqual(Position16<BgPlanePixels>.Zero, ending.postCreditsScroll, "the white flash resets the text map scroll to zero");

        ending.Phase = EndingCreditsPhase.ItemPercentageScrollDown;
        ending.phaseTimer = ScrollDownCalls;
        for (int call = 1; call <= ScrollDownCalls; call++)
        {
            ending.Step();
            var expected = new Position16<BgPlanePixels>(0, checked((ushort)(ScrollWordRange - call * PixelsPerCall)));
            AssertEqual(expected, ending.postCreditsScroll,
                $"scroll-down call {call} wraps the vertical scroll below zero to {expected.Y}, as the native word did");
        }
        AssertEqual(EndingCreditsPhase.SeeYouNextMission, ending.Phase, "the last scroll-down call starts the closing message");
        Console.WriteLine($"Ending post-credits scroll: the white flash resets it to zero and {ScrollDownCalls} scroll-down calls wrap it to 65534 through 65456.");
    }

    /// <summary>Runs the focused post-credits scroll wrap regression.</summary>
    [VerificationCommand("--ending-post-credits-scroll")]
    internal static int EndingPostCreditsScrollCommand()
    {
        VerifyEndingPostCreditsScroll();
        return 0;
    }
}
