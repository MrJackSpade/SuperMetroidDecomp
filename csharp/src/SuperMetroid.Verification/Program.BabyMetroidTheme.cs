using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static BabyMetroidFixtureHeadVerification;

internal static class BabyMetroidThemeVerification
{
    internal static int VerifyBabyMetroidTheme()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]);
        enemies.MotherBrainEncounter.MotherBrain = state;
        enemies.Presentation._cgram = new SnesCgram();
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
            var step = baby.Step(bus, samus, sequence, HeadOf(sequence));
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

    [VerificationCommand("--baby-metroid-theme")]
    internal static int BabyMetroidThemeCommand()
    {
        return VerifyBabyMetroidTheme();
    }
}
