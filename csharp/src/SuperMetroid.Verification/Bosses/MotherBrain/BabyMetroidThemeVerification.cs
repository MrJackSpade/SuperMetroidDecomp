using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidFixtureHeadVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Baby Metroid's fatal-stop and theme-start music requests through the cutscene transition.</summary>
internal static class BabyMetroidThemeVerification
{
    /// <summary>Confirms the frame-72 data-upload and track pair follows the initial stop, each with the native eight-frame queue delay and no duplicate requests.</summary>
    internal static int VerifyBabyMetroidTheme()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem(new EnemySession());
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]);
        enemies.MotherBrainEncounter.MotherBrain = state;
        enemies.Presentation.BindCgram(new SnesCgram());
        var baby = new BabyMetroidCutsceneState();
        baby.Phase = BabyMetroidCutscenePhase.FinalCharge;
        baby.Health = (ushort)0;
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        var samus = new SamusState();
        Action<MotherBrainEnemyState, BabyMetroidCutsceneStepResult> apply = enemies.BabyMetroidCutsceneBehavior.ApplyBabyMetroidFrameEffects;
        var begin = new Action(enemies.BeginEnemySoundRequestFrame);
        var beginMotherBrain = new Action(state.BeginFrame);
        var collect = new Action(enemies.CollectLegacyEnemyAudioRequests);
        for (int frame = 0; frame <= 73; frame++)
        {
            begin();
            beginMotherBrain();
            var step = baby.Step(samus, sequence, HeadOf(sequence));
            apply(state, step);
            collect();
            ushort[] expected = frame switch { 0 => [0], 72 => [0xff48, 5], _ => [] };
            AssertEqual(expected.Length, enemies.Audio.MusicRequests.Count, $"Native cutscene music request count at frame {frame}");
            for (int index = 0; index < expected.Length; index++)
            {
                AssertEqual(expected[index], enemies.Audio.MusicRequests[index].Command.RawValue, "Stop, data upload and track retain native ordering");
                AssertEqual((ushort)8, enemies.Audio.MusicRequests[index].Delay.Frames, "Each native command retains its eight-frame queue delay");
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
