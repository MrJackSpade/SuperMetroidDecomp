using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static int VerifyBabyMetroidTheme()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        var state = new MotherBrainEnemyState(enemies.Slots[0]);
        typeof(RoomEnemySystem).GetField("_motherBrain", flags)!.SetValue(enemies, state);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        var baby = new BabyMetroidCutsceneState();
        typeof(BabyMetroidCutsceneState).GetProperty(nameof(baby.Phase))!.SetValue(baby, BabyMetroidCutscenePhase.FinalCharge);
        typeof(BabyMetroidCutsceneState).GetProperty(nameof(baby.Health))!.SetValue(baby, (ushort)0);
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        var samus = new SamusState();
        var apply = typeof(RoomEnemySystem).GetMethod("ApplyBabyMetroidFrameEffects", flags)!
            .CreateDelegate<Action<MotherBrainEnemyState, BabyMetroidCutsceneStepResult>>(enemies);
        var begin = typeof(RoomEnemySystem).GetMethod("BeginEnemySoundRequestFrame", flags)!.CreateDelegate<Action>(enemies);
        var beginMotherBrain = typeof(MotherBrainEnemyState).GetMethod("BeginFrame", flags)!.CreateDelegate<Action>(state);
        var collect = typeof(RoomEnemySystem).GetMethod("CollectLegacyEnemyAudioRequests", flags)!.CreateDelegate<Action>(enemies);
        for (int frame = 0; frame <= 73; frame++)
        {
            begin();
            beginMotherBrain();
            var step = baby.Step(bus, samus, sequence);
            apply(state, step);
            collect();
            ushort[] expected = frame switch { 0 => [0], 72 => [0xff48, 5], _ => [] };
            AssertEqual(expected.Length, enemies.MusicRequests.Count, $"Native cutscene music request count at frame {frame}");
            for (int index = 0; index < expected.Length; index++)
            {
                AssertEqual(expected[index], enemies.MusicRequests[index].Command.RawValue, "Stop, data upload and track retain native ordering");
                AssertEqual((ushort)8, enemies.MusicRequests[index].Delay.Frames, "Each native command retains its eight-frame queue delay");
            }
            if (frame == 72)
            {
                AssertEqual(BabyMetroidCutscenePhase.PrepareSamusForHyperBeam, step.PhaseAfter, "Theme starts at preparation transition");
                AssertEqual((ushort)11, baby.FunctionTimer, "Theme transition retains same-call Hyper Beam preparation decrement");
            }
        }
        Console.WriteLine("Baby Metroid music: fatal stop, 72-frame theme boundary, ordered data/track, eight-frame delays and no duplicate requests passed.");
        return 0;
    }
}
