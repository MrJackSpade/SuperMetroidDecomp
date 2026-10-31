using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidFixtureHeadVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
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
        var enemies = new RoomEnemySystem(new EnemySession());
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]);
        enemies.MotherBrainEncounter.MotherBrain = state;
        enemies.Presentation.BindColours(new GameplayColours(new SpriteColourSets()));
        var baby = new BabyMetroidCutsceneState();
        baby.Phase = BabyMetroidCutscenePhase.FinalCharge;
        baby.Health = 0;
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        var samus = new SamusState(InstalledFixtures.AnimationContinuation);
        Action<BabyMetroidCutsceneStepResult> apply = enemies.BabyMetroidCutsceneBehavior.ApplyBabyMetroidFrameEffects;
        var begin = new Action(enemies.BeginEnemySoundRequestFrame);
        var beginMotherBrain = new Action(state.BeginFrame);
        var collect = new Action(enemies.PublishEnemyAudioRequests);
        for (int frame = 0; frame <= 73; frame++)
        {
            begin();
            beginMotherBrain();
            var step = baby.Step(samus, sequence, HeadOf(sequence));
            apply(step);
            collect();
            // #1410: the requests are asserted by kind and identity instead of by their queue words ($0000, $FF48, $0005).
            MusicRequest[] expected = frame switch
            {
                0 => [MusicRequest.Stop],
                72 => [MusicRequest.LoadDataSet(MusicDataSet.SamusThemeUpperCrateria), MusicRequest.SelectTrack(5)],
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
                AssertEqual(BabyMetroidCutscenePhase.PrepareSamusForHyperBeam, step.PhaseAfter, "Theme starts at preparation transition");
                AssertEqual((ushort)11, baby.FunctionTimer.RawValue, "Theme transition retains same-call Hyper Beam preparation decrement");
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
