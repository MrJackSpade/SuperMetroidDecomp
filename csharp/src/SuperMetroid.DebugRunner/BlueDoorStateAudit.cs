using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;

/// <summary>Fires through the real frontend from #614's unmodified production save.</summary>
internal static class BlueDoorStateAudit
{
    public static int Run(string romPath, string statePath)
    {
        string directory = Path.Combine(Path.GetTempPath(), "sm-614-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string copy = Path.Combine(directory, "SuperMetroid-debug-slot-0.smstate");
        File.Copy(statePath, copy);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var store = new DebuggerSaveStateStore(romPath, bus.Rom, directory);
        DebuggerSaveStateLoadResult loaded;
        try { loaded = store.Load(0); }
        finally { File.Delete(copy); Directory.Delete(directory); }
        var game = loaded.Game;
        var runtime = game.RuntimeForVerification!;
        // These coordinates identify the preserved four-block cap, not a general
        // collision rule. Never reposition Samus or replace the captured room.
        if (runtime.LevelData!.GetCollisionBlock(1, 22).Bts != RoomBlockBehaviorValues.BlueDoorFacingRight)
            throw new InvalidDataException("#614 fixture no longer contains the reported closed cap.");
        var audio = new CartridgeAudioRenderer(ExtractedAudioAssetCatalog.Load("standalone-assets/audio"), loaded.AudioPlayer);
        for (int frame = 0; frame < 150; frame++)
        {
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            var result = game.StepCaptured(frame < 2 ? (ushort)0 : (ushort)SnesButton.X, frame + 1, 1);
            audio.RenderFrame(result.Frame.AudioCommands);
        }
        for (int row = 22; row <= 25; row++)
            if (runtime.LevelData!.GetCollisionBlock(1, row).CollisionType != RoomCollisionType.Air)
                throw new InvalidDataException($"#614: preserved blue-door cap row {row} remains closed after firing.");
        Console.WriteLine("#614: preserved blue-door cap opened after firing.");
        return 0;
    }
}
