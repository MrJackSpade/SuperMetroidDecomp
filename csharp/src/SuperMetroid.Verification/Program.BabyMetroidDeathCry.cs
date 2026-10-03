using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static int VerifyBabyMetroidDeathCry()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        string root = Path.GetFullPath("out/workbook-investigation/crocomire-install");
        var installation = GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
        enemies.MotherBrainRoomColors = installation.LoadMaps().MotherBrainRoomColors;
        var cgram = new SnesCgram();
        for (int index = 0; index < 256; index++) cgram.SetColor(index, (ushort)(index + 1));
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
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
        for (int index = 0; index < 256; index++)
            AssertEqual((ushort)(index + 1), cgram.Colors[index], "Live final charge preserves room colors");
        baby.ApplyMotherBrainOnionRingHit(80);
        var fatal = baby.Step(bus, samus, sequence);
        AssertEqual(BabyMetroidCutscenePhase.TakeFinalBlow, fatal.PhaseAfter, "Fatal hit enters native shake phase");
        apply(state, fatal);
        for (int index = 0; index < 256; index++)
            AssertEqual((ushort)(IsBlackoutColor(index) ? 0 : index + 1), cgram.Colors[index],
                $"Fatal blow blacks out only native background colors: {index}");
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
        typeof(BabyMetroidCutsceneState).GetProperty(nameof(baby.Phase))!.SetValue(baby, BabyMetroidCutscenePhase.FinalCutscene);
        for (int frame = 0; frame < 7; frame++)
        {
            var recovery = baby.Step(bus, samus, sequence);
            AssertEqual((ushort)frame, recovery.BackgroundPaletteTransfer!.Value.PaletteIndex, "Recovery publishes each native light row");
            apply(state, recovery);
            for (int index = 0; index < 256; index++)
            {
                int offset = index is >= 49 and <= 62 ? index - 49 : 14 + index - 81;
                ushort expected = IsBlackoutColor(index)
                    ? (ushort)(bus.ReadCartridgeByte(0xadf3d3 - frame * 0x38 + offset * 2) | bus.ReadCartridgeByte(0xadf3d3 - frame * 0x38 + offset * 2 + 1) << 8)
                    : (ushort)(index + 1);
                AssertEqual(expected, cgram.Colors[index], $"Recovery row {frame} restores exact cartridge colors and preserves other palettes: {index}");
            }
        }
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
        Console.WriteLine("Baby Metroid fatal blow: exact 28-color blackout, seven cartridge recovery rows, untouched sprite palettes, one-shot death cry and SPC publication passed.");
        return 0;

        static bool IsBlackoutColor(int index) => index is >= 49 and <= 62 or >= 81 and <= 94;
    }
}
