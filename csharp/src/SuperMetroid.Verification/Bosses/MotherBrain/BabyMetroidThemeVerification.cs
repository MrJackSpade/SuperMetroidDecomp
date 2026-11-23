using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Baby Metroid's fatal-stop and theme-start music requests through the cutscene transition.</summary>
internal static class BabyMetroidThemeVerification
{
    /// <summary>Confirms the frame-72 data-upload and track pair follows the initial stop, each with the native eight-frame queue delay and no duplicate requests.</summary>
    internal static int VerifyBabyMetroidTheme()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        // Mother Brain is placed as her room's population places her, and her late fight brings in the Baby, whose own main
        // AI then runs its cutscene; it stands at its final charge with no health left.
        // Her late fight and the Baby read the room's random numbers, which stand at zero here.
        enemies.VerificationServices().ReadRandom = () => 0;
        enemies.VerificationServices().NextRandom = () => 0;
        MotherBrainEnemyState state = MotherBrainPlacement.PlaceMotherBrain(enemies, samus);
        MotherBrainRainbowBeamAttackSequence lateFight = MotherBrainBabyArrival.BringInBaby(state, samus);
        BabyMetroidCutsceneEntity entity = MotherBrainBabyArrival.BabyOf(lateFight);
        BabyMetroidCutsceneState baby = lateFight.Baby ?? throw new InvalidOperationException("Verification requires the final beam's spawn to make the Baby.");
        baby.Phase = new BabyMetroidCutscenePhase.Flying.FinalCharge();
        baby.Health = 0;
        var begin = new Action(enemies.BeginEnemySoundRequestFrame);
        for (int frame = 0; frame <= 73; frame++)
        {
            begin();
            entity.Place.RunMainAi(samus);
            // #1410: the requests are asserted by kind and identity instead of by their queue words ($0000, $FF48, $0005).
            MusicRequest[] expected = frame switch
            {
                0 => [MusicRequest.Stop],
                72 => [MusicRequest.LoadDataSet(MusicDataSet.SamusThemeUpperCrateria), MusicRequest.SelectTrack(MusicTrack.SamusTheme)],
                _ => [],
            };
            AssertEqual(expected.Length, enemies.Audio.MusicRequests.Count, $"Native cutscene music request count at frame {frame}");
            for (int index = 0; index < expected.Length; index++)
            {
                AssertEqual(expected[index], enemies.Audio.MusicRequests[index].Request, "Stop, data set change and track retain native ordering");
                AssertEqual(8, enemies.Audio.MusicRequests[index].Delay.Updates, "Each native command retains its eight-frame queue delay");
            }
            if (frame == 72)
            {
                AssertEqual(new BabyMetroidCutscenePhase.Flying.PrepareSamusForHyperBeam(), baby.Phase, "Theme starts at preparation transition");
                AssertEqual(11, baby.PhaseTimer.CartridgeCount, "Theme transition retains same-call Hyper Beam preparation decrement");
            }
        }
        Console.WriteLine("Baby Metroid music: fatal stop, 72-frame theme boundary, ordered data/track, eight-frame delays and no duplicate requests passed.");
        return 0;
    }

    /// <summary>Runs the focused Baby Metroid cutscene music request check.</summary>
    [VerificationCommand("--baby-metroid-theme")]
    internal static int BabyMetroidThemeCommand()
    {
        return VerifyBabyMetroidTheme();
    }
}
