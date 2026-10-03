using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static int VerifyBabyMetroidDeathCry()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        var state = new MotherBrainEnemyState(enemies.Slots[0]);
        var baby = new BabyMetroidCutsceneState();
        typeof(BabyMetroidCutsceneState).GetProperty(nameof(baby.Phase))!.SetValue(baby, BabyMetroidCutscenePhase.FinalCharge);
        typeof(BabyMetroidCutsceneState).GetProperty(nameof(baby.Health))!.SetValue(baby, (ushort)79);
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        var samus = new SamusState();
        var apply = typeof(RoomEnemySystem).GetMethod("ApplyBabyMetroidFrameEffects", flags)!
            .CreateDelegate<Action<MotherBrainEnemyState, BabyMetroidCutsceneStepResult>>(enemies);
        apply(state, baby.Step(bus, samus, sequence));
        AssertEqual(0, enemies.SoundRequests.Count, "The live final charge does not emit a death cry early");
        baby.ApplyMotherBrainOnionRingHit(80);
        var fatal = baby.Step(bus, samus, sequence);
        AssertEqual(BabyMetroidCutscenePhase.TakeFinalBlow, fatal.PhaseAfter, "Fatal hit enters native shake phase");
        apply(state, fatal);
        AssertEqual(1, enemies.SoundRequests.Count, "Fatal transition emits exactly one death cry");
        var request = enemies.SoundRequests.Single();
        AssertEqual(SoundEffectLibrary.Library3, request.SoundEffect.Library, "Death cry belongs to library three");
        AssertEqual((byte)0x19, request.SoundEffect.Value, "Death cry retains native sequence 19");
        AssertEqual((byte)6, request.MaximumQueued, "Death cry uses native Max6");
        for (int frame = 0; frame < 3; frame++)
        {
            typeof(RoomEnemySystem).GetMethod("BeginEnemySoundRequestFrame", flags)!.Invoke(enemies, null);
            apply(state, baby.Step(bus, samus, sequence));
            AssertEqual(0, enemies.SoundRequests.Count, "Fatal shake does not replay the death cry");
        }
        string root = Path.GetFullPath("out/workbook-investigation/crocomire-install");
        var installation = GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
        var audio = new CartridgeAudioState();
        var renderer = new CartridgeAudioRenderer(installation.LoadAudio());
        renderer.RenderFrame(audio.AdvanceFrame(bus, renderer.ReadAcknowledgements()));
        renderer.RenderFrame([CartridgeAudioCommand.Upload(AudioUploadAddresses.MotherBrain)]);
        audio.QueueSound(request.SoundEffect, request.MaximumQueued);
        var writes = new List<CartridgeAudioCommand>();
        for (int frame = 0; frame < 8; frame++)
        {
            var commands = audio.AdvanceFrame(bus, renderer.ReadAcknowledgements());
            writes.AddRange(commands.Where(command => command.Kind == CartridgeAudioCommandKind.WritePort));
            renderer.RenderFrame(commands);
        }
        AssertEqual(1, writes.Count(command => command.Port == 3 && command.Value == 0x19), "SPC receives exactly one library-three death cry");
        AssertEqual(0, writes.Count(command => command.Port == 2 && command.Value == 0x19), "Death cry never reaches the wrong library");
        Console.WriteLine("Baby Metroid death cry: fatal transition, one-shot live request, native library/Max6 and SPC publication passed.");
        return 0;
    }
}
