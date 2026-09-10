using System.Reflection;
using SuperMetroid.Android;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;

/// <summary>Portable #51 seed for the reported Blue Brinstar eye, isolated from player saves.</summary>
internal static class ReportedEyeStateFixture
{
    public static int Export(string destination)
    {
        string root = Path.GetFullPath(destination);
        if (Directory.Exists(root))
            throw new IOException($"Refusing to overwrite an existing fixture directory: {root}");
        Directory.CreateDirectory(root);
        using var data = new AndroidSessionData(root, Path.GetFullPath("Super Metroid.smc"),
            Path.GetFullPath("standalone-assets/audio"));
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroidGame).GetMethod("SetupSelectedGame", fields)!.Invoke(data.Game, null);
        var runtime = (SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime", fields)!.GetValue(data.Game)!;
        // Room identity is fixture data; normal room loading owns its population,
        // graphics, HDMA initialization, and camera-dependent render snapshots.
        typeof(SuperMetroidRuntime).GetMethod("LoadCartridgeRoomForDebug", fields)!
            .Invoke(runtime, [RoomHeaderPointers.BlueBrinstarEnergyTankRoom, (ushort)384, (ushort)512]);
        var body = runtime.Enemies.Slots[1];
        var samus = runtime.Samus!;
        samus.CollectedItems = (ushort)SamusEquipmentFlags.MorphBall;
        samus.InputLocked = true;
        samus.XPosition = (ushort)(body.XPosition - 64);
        samus.YPosition = (ushort)(body.YPosition - 32);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!
            .SetValue(data.Game, SuperMetroidGameState.MainGameplay);
        for (long frame = 1; frame <= 80; frame++)
        {
            data.Game.SetAudioAcknowledgements(data.Audio.ReadAcknowledgements());
            data.Record(0);
            var result = data.Game.StepCaptured(0, frame, data.Generation);
            data.Audio.RenderFrame(result.Frame.AudioCommands);
        }
        if (runtime.DisplayedMorphBallEyeBeam is not { Phase: MorphBallEyeBeamPhase.Full })
            throw new InvalidDataException("Reported room eye did not reach a fully published beam.");
        Console.WriteLine(data.SaveSlot(9));
        Console.WriteLine($"Reported eye seed: {root}");
        return 0;
    }
}
