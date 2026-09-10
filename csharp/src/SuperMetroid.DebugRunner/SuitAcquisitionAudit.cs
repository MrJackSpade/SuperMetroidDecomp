using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Room-local Varia acquisition through normal input, message and managed audio.</summary>
internal static class SuitAcquisitionAudit
{
    public static int Run(string rom, string audioDirectory)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, water: false);
        runtime.LoadCartridgeRoomForDebug(0xa6e2, 0, 0);
        runtime.InitializeDebugGroundedSamus(80, 139, 8);
        runtime.Samus!.InputLocked = false;
        var game = new SuperMetroidGame(bus);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
        var assets = ExtractedAudioAssetCatalog.Load(audioDirectory);
        var renderer = new CartridgeAudioRenderer(assets);
        var silentControl = new CartridgeAudioRenderer(assets);
        renderer.RenderFrame(new[] { CartridgeAudioCommand.Upload(AudioUploadAddresses.SpcEngine) });
        silentControl.RenderFrame(new[] { CartridgeAudioCommand.Upload(AudioUploadAddresses.SpcEngine) });
        bool message = false, transformation = false, returned = false;
        int sounds = 0;
        int effectStart = -1, soundFrame = -1, differentPcmFrames = 0;
        var recordedMono = new List<short>();
        for (int frame = 0; frame < 900; frame++)
        {
            ushort input = frame < 60 ? (ushort)SnesButton.X : frame < 94
                ? (ushort)(SnesButton.Right | SnesButton.A) : (ushort)0;
            if (frame >= 120 && frame < 125) input = (ushort)SnesButton.A;
            if (frame >= 125 && frame < 155) input = (ushort)(SnesButton.Down | SnesButton.X);
            if (runtime.MessageBox.Phase == GameplayMessageBoxPhase.AwaitingInput)
                input = (ushort)SnesButton.X;
            var output = game.Step(input);
            var pcm = renderer.RenderFrame(output.AudioCommands);
            var control = silentControl.RenderFrame(output.AudioCommands.Where(c =>
                !(c.Kind == CartridgeAudioCommandKind.WritePort && c.Port == 2 && c.Value == 0x56)).ToArray());
            if (runtime.SuitPickup.IsActive && effectStart < 0) effectStart = frame;
            if (output.AudioCommands.Any(c => c.Kind == CartridgeAudioCommandKind.WritePort && c.Port == 2 && c.Value == 0x56))
                soundFrame = frame;
            if (!pcm.AsSpan().SequenceEqual(control))
            {
                if (soundFrame < 0) throw new InvalidDataException("Audio diverged before the transformation sound.");
                differentPcmFrames++;
            }
            if (effectStart >= 0)
                for (int i = 0; i < pcm.Length; i += 2)
                    recordedMono.Add((short)(((int)pcm[i] + pcm[i + 1]) / 2));
            game.SetAudioAcknowledgements(renderer.ReadAcknowledgements());
            message |= runtime.MessageBox.IsActive;
            transformation |= runtime.SuitPickup.IsActive;
            returned |= transformation && !runtime.SuitPickup.IsActive;
            sounds += output.AudioCommands.Count(c => c.Kind == CartridgeAudioCommandKind.WritePort && c.Port == 2 && c.Value == 0x56);
            if (frame % 60 == 0)
                Console.WriteLine($"frame={frame} XY={runtime.Samus.XPosition},{runtime.Samus.YPosition} message={runtime.MessageBox.Phase} suit={runtime.SuitPickup.IsActive} sounds={sounds}");
        }
        Directory.CreateDirectory("csharp/test-temp");
        PcmWaveFile.WriteMonoPcm16("csharp/test-temp/suit-acquisition-522.wav", CartridgeAudioRenderer.SampleRate, recordedMono);
        Console.WriteLine($"Varia acquisition: message={message}, transform={transformation}, return={returned}, sound={sounds}, effectStart={effectStart}, soundFrame={soundFrame}, PCM difference frames={differentPcmFrames}.");
        return message && transformation && returned && !runtime.Samus.InputLocked &&
            (runtime.Samus.EquippedItems & (ushort)SamusEquipmentFlags.VariaSuit) != 0 &&
            sounds == 1 && soundFrame == effectStart && differentPcmFrames > 0 ? 0 : 1;
    }
}
