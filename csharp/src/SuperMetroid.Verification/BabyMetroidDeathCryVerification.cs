using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static BabyMetroidFixtureHeadVerification;

/// <summary>Checks Baby Metroid's fatal blackout, palette recovery, and one-shot SPC death cry.</summary>
internal static class BabyMetroidDeathCryVerification
{
    /// <summary>Compares all affected CGRAM entries and verifies exactly one native library-three sound publication.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Baby Metroid death-cry palette frames")]
    internal static int VerifyBabyMetroidDeathCry()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        var installation = RepositoryInstallation.Installation;
        enemies.Presentation.MotherBrainRoomColors = InstalledFixtures.Maps.MotherBrainRoomColors;
        var cgram = new SnesCgram();
        for (int index = 0; index < 256; index++) cgram.SetColor(index, Bgr555.FromWord((ushort)(index + 1)));
        enemies.Presentation._cgram = cgram;
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]);
        var baby = new BabyMetroidCutsceneState();
        baby.Phase = BabyMetroidCutscenePhase.FinalCharge;
        baby.Health = (ushort)79;
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        var samus = new SamusState();
        Action<MotherBrainEnemyState, BabyMetroidCutsceneStepResult> apply = enemies.BabyMetroidCutsceneBehavior.ApplyBabyMetroidFrameEffects;
        apply(state, baby.Step(bus, samus, sequence, HeadOf(sequence)));
        AssertEqual(0, enemies.Audio.SoundRequests.Count, "The live final charge does not emit a death cry early");
        for (int index = 0; index < 256; index++)
            AssertEqual((ushort)(index + 1), cgram.Colors[index], "Live final charge preserves room colors");
        baby.ApplyMotherBrainOnionRingHit(80);
        var fatal = baby.Step(bus, samus, sequence, HeadOf(sequence));
        AssertEqual(BabyMetroidCutscenePhase.TakeFinalBlow, fatal.PhaseAfter, "Fatal hit enters native shake phase");
        apply(state, fatal);
        for (int index = 0; index < 256; index++)
            AssertEqual((ushort)(IsBlackoutColor(index) ? 0 : index + 1), cgram.Colors[index],
                $"Fatal blow blacks out only native background colors: {index}");
        AssertEqual(1, enemies.Audio.SoundRequests.Count, "Fatal transition emits exactly one death cry");
        var request = enemies.Audio.SoundRequests.Single();
        AssertEqual(SoundEffectLibrary.Library3, request.SoundEffect.Library, "Death cry belongs to library three");
        AssertEqual((byte)0x19, request.SoundEffect.Value, "Death cry retains native sequence 19");
        AssertEqual((byte)6, request.MaximumQueued, "Death cry uses native Max6");
        for (int frame = 0; frame < 3; frame++)
        {
            enemies.BeginEnemySoundRequestFrame();
            apply(state, baby.Step(bus, samus, sequence, HeadOf(sequence)));
            AssertEqual(0, enemies.Audio.SoundRequests.Count, "Fatal shake does not replay the death cry");
        }
        baby.Phase = BabyMetroidCutscenePhase.FinalCutscene;
        for (int frame = 0; frame < 7; frame++)
        {
            var recovery = baby.Step(bus, samus, sequence, HeadOf(sequence));
            AssertEqual((ushort)frame, (recovery.BackgroundPaletteTransfer ?? throw new InvalidOperationException("Verification requires recovery.BackgroundPaletteTransfer.")).PaletteIndex, "Recovery publishes each native light row");
            apply(state, recovery);
            for (int index = 0; index < 256; index++)
            {
                int offset = index is >= 49 and <= 62 ? index - 49 : 14 + index - 81;
                ushort expected = IsBlackoutColor(index)
                    ? bus.ReadCartridgeWord(0xadf3d3 - frame * 0x38 + offset * 2)
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
        AssertEqual(1, writes.Count(command => command.InputPort == ApuPort.SoundLibrary3 && command.Value == 0x19), "SPC receives exactly one library-three death cry");
        AssertEqual(0, writes.Count(command => command.InputPort == ApuPort.SoundLibrary2 && command.Value == 0x19), "Death cry never reaches the wrong library");
        Console.WriteLine("Baby Metroid fatal blow: exact 28-color blackout, seven cartridge recovery rows, untouched sprite palettes, one-shot death cry and SPC publication passed.");
        return 0;

        static bool IsBlackoutColor(int index) => index is >= 49 and <= 62 or >= 81 and <= 94;
    }

    /// <summary>Runs the fatal-blow palette and audio verification.</summary>
    [VerificationCommand("--baby-metroid-death-cry")]
    internal static int BabyMetroidDeathCryCommand()
    {
        return VerifyBabyMetroidDeathCry();
    }
}
