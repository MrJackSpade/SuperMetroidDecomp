using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// Intro checks the 13% movie replay adds to its per-update comparison: every intro phase
/// handoff on the update its bsnes capture shows bank $8B's matching cinematic function
/// installed, no other phase change, and the Samus values the capture records at the intro
/// updates owned by one fix each. One instance follows one replay.
/// </summary>
internal sealed class LowPercentIntroReplayChecks
{
    /// <summary>
    /// Every phase handoff, keyed by the update after which the capture shows bank $8B's
    /// matching cinematic function installed. Between them no handoff occurs.
    /// </summary>
    private static readonly Dictionary<int, LowPercentIntroReplayPhase> phases = new()
    {
        [214] = new LowPercentIntroReplayPhase("Initial", "$A395"),
        [215] = new LowPercentIntroReplayPhase("WaitForInitialMusicQueue", "$A5A7"),
        [232] = new LowPercentIntroReplayPhase("FadeInFirstNarration", "$A5BD"),
        [262] = new LowPercentIntroReplayPhase("LastMetroidIsInCaptivity", "$A5F8"),
        [322] = new LowPercentIntroReplayPhase("GalaxyIsAtPeace", "$A613"),
        [522] = new LowPercentIntroReplayPhase("WaitForSecondMusicQueue", "$A639"),
        [554] = new LowPercentIntroReplayPhase("FourSecondHold", "$A64C"),
        [794] = new LowPercentIntroReplayPhase("FadeOutFirstNarration", "$A663"),
        [824] = new LowPercentIntroReplayPhase("SetupPageOne", "$A66F"),
        [825] = new LowPercentIntroReplayPhase("WaitForPageOneMusicQueue", "$A82B"),
        [857] = new LowPercentIntroReplayPhase("FadeInPageOne", "$A84A"),
        [887] = new LowPercentIntroReplayPhase("PageOneText", "$A391"),
        [1718] = new LowPercentIntroReplayPhase("PageOneAwaitingInput", "$AEB8"),
        [1719] = new LowPercentIntroReplayPhase("MotherBrainCrossfade", "$B250"),
        [1847] = new LowPercentIntroReplayPhase("MotherBrainFlashback", "$A391"),
        [2235] = new LowPercentIntroReplayPhase("PageTwoCrossfade", "$B3F4"),
        [2363] = new LowPercentIntroReplayPhase("PageTwoText", "$A391"),
        [3011] = new LowPercentIntroReplayPhase("PageTwoAwaitingInput", "$AF6C"),
        [3012] = new LowPercentIntroReplayPhase("BabyDiscoveryCrossfade", "$B250"),
        [3140] = new LowPercentIntroReplayPhase("BabyDiscovery", "$A391"),
        [3816] = new LowPercentIntroReplayPhase("PageThreeCrossfade", "$B3F4"),
        [3944] = new LowPercentIntroReplayPhase("PageThreeText", "$A391"),
        [4432] = new LowPercentIntroReplayPhase("PageThreeAwaitingInput", "$B0F2"),
        [4433] = new LowPercentIntroReplayPhase("BabyMetroidDeliveryCrossfade", "$B2D2"),
        [4561] = new LowPercentIntroReplayPhase("BabyMetroidDelivery", "$A391"),
        [4834] = new LowPercentIntroReplayPhase("PageFourCrossfade", "$B458"),
        [4962] = new LowPercentIntroReplayPhase("PageFourText", "$A391"),
        [5500] = new LowPercentIntroReplayPhase("PageFourAwaitingInput", "$B123"),
        [5501] = new LowPercentIntroReplayPhase("BabyMetroidExaminationCrossfade", "$B2D2"),
        [5629] = new LowPercentIntroReplayPhase("BabyMetroidExamination", "$A391"),
        [5902] = new LowPercentIntroReplayPhase("PageFiveCrossfade", "$B458"),
        [6030] = new LowPercentIntroReplayPhase("PageFiveText", "$A391"),
        [6683] = new LowPercentIntroReplayPhase("PageFiveAwaitingInput", "$B1DA"),
        [6684] = new LowPercentIntroReplayPhase("PageSixText", "$A390"),
        [6968] = new LowPercentIntroReplayPhase("IntroFadeOut", "$B72F"),
        [6998] = new LowPercentIntroReplayPhase("CeresFlight/Initial", "$BCA0"),
        [6999] = new LowPercentIntroReplayPhase("CeresFlight/WaitForMusicQueue", "$BDE4"),
        [7023] = new LowPercentIntroReplayPhase("CeresFlight/FlyingIntoCamera", "$BDF9"),
        [7055] = new LowPercentIntroReplayPhase("CeresFlight/FlyingTowardCeres", "$BFDA"),
        [7407] = new LowPercentIntroReplayPhase("CeresFlight/SpaceColonyTitle", "$A38F"),
        [7711] = new LowPercentIntroReplayPhase("CeresFlight/FadeOut", "$C0C5"),
        [7741] = new LowPercentIntroReplayPhase("CeresFlight/StartGameAtCeres", "$C100"),
    };

    /// <summary>The intro phase after the previous intro update, or null before the intro.</summary>
    private string? previous;

    /// <summary>Checks the port intro phase and Samus milestone after an intro update.</summary>
    internal IEnumerable<string> Compare(SuperMetroidGame game, int update)
    {
        if (game.GameState != SuperMetroidGameState.IntroCinematic || game.Session.ScreenAs<IntroCinematicState>() is not { } intro)
            return [];
        string phase = intro.Phase.ToString();
        if (intro.CurrentCeresFlight is { } flight)
            phase += "/" + flight.Phase;
        var mismatches = new List<string>();
        if (phases.TryGetValue(update, out var expected))
        {
            if (phase != expected.Phase)
                mismatches.Add($"Intro phase: native installs $8B:{expected.Native[1..]} ({expected.Phase}), port is in {phase}");
        }
        else if (previous is not null && phase != previous)
            mismatches.Add($"Intro phase: port entered {phase}, where native installs no new cinematic function");
        previous = phase;
        CheckSamusMilestone(intro, update);
        return mismatches;
    }

    /// <summary>Native Samus values the 13% capture records at intro updates owned by one fix each.</summary>
    private static void CheckSamusMilestone(IntroCinematicState intro, int update)
    {
        SamusState? flashback = intro.CurrentFlashbackSamus;
        SamusState? discovery = intro.CurrentBabyDiscovery is { } scene
            ? scene.Samus : null;
        switch (update)
        {
            case 1809:
                // $90:A3BA: the demo's held shot restarts and holds the standing animation.
                AssertEqual((ushort)0x000f, (flashback ?? throw new InvalidOperationException("Verification requires flashback.")).AnimationFrameTimer.RawValue, "update 1809 standing-shot animation timer");
                AssertEqual(0, flashback.AnimationFrame, "update 1809 standing-shot animation frame");
                break;
            case 1884:
                // $8B:B90B: Rinka zero, stepped in its spawn pass, strikes Samus on native's update.
                AssertEqual((byte)0x54, (byte)(flashback ?? throw new InvalidOperationException("Verification requires flashback.")).Pose, "update 1884 Rinka knockback pose");
                break;
            case 2050:
                // $90:EB02 cleared the run's movement records, so the missile inherits nothing.
                var slots = intro.flashbackProjectiles;
                SamusProjectileSlot missile = slots.SlotTable.Slots[0];
                AssertEqual(unchecked((short)0xff00), missile.Velocity.X.SignedRawValue, "update 2050 third missile launch velocity");
                AssertEqual((ushort)0x0088, missile.Position.X.Whole, "update 2050 third missile X");
                break;
            case 2111:
                // $91:874B: the demo-end lock keeps the running animation of an unchanged pose.
                AssertEqual((ushort)0x000e, (flashback ?? throw new InvalidOperationException("Verification requires flashback.")).AnimationFrameTimer.RawValue, "update 2111 locked Samus animation timer");
                break;
            case 2400:
                // $8B:B842 cleared $1A57, so Samus is not processed during the page-two text.
                AssertEqual((ushort)0x0003, (flashback ?? throw new InvalidOperationException("Verification requires flashback.")).AnimationFrameTimer.RawValue, "update 2400 frozen Samus animation timer");
                AssertEqual(0, flashback.AnimationFrame, "update 2400 frozen Samus animation frame");
                break;
            case 3012:
                // $8B:AF99: pose two was already set, so the discovery keeps her animation running.
                AssertEqual((ushort)0x0002, (discovery ?? throw new InvalidOperationException("Verification requires discovery.")).AnimationFrameTimer.RawValue, "update 3012 discovery Samus animation timer");
                AssertEqual(0, discovery.AnimationFrame, "update 3012 discovery Samus animation frame");
                break;
            case 3990:
                // $91:8682 pointed both state handlers at an RTL during update 3943.
                AssertEqual((ushort)0x0028, (discovery ?? throw new InvalidOperationException("Verification requires discovery.")).Position.X.Whole, "update 3990 stopped Samus X");
                AssertEqual((ushort)0x5000, discovery.Kinematics.Position.X.Fraction, "update 3990 stopped Samus X fraction");
                break;
            default:
                break;
        }
    }
}
