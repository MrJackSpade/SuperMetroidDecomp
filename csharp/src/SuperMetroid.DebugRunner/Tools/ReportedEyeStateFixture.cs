using System.Reflection;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.DebugRunner.Tools;
using SuperMetroid.Diagnostics.Android;

/// <summary>Portable #51 seed for the reported Blue Brinstar eye, isolated from player saves.</summary>
internal static class ReportedEyeStateFixture
{
    /// <summary>Creates an isolated Blue Brinstar room seed for the reported eye,
    /// advances until its beam is published, and writes it to save slot nine.</summary>
    public static int Export(string destination)
    {
        string root = Path.GetFullPath(destination);
        if (Directory.Exists(root))
            throw new IOException($"Refusing to overwrite an existing fixture directory: {root}");
        Directory.CreateDirectory(root);
        using var data = new AndroidSessionData(root, Path.GetFullPath("Super Metroid.smc"),
            Path.GetFullPath("standalone-assets/audio"));
        data.Game.Session.SetupSelectedGame();
        var runtime = data.Game.Session.GameplayRuntime;
        // Room identity is fixture data; normal room loading owns its population,
        // graphics, HDMA initialization, and camera-dependent render snapshots.
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.BlueBrinstarETank, new Position16<RoomPixels>(384, 512));
        var body = runtime.Room.LoadedScope.Enemies.SlotTable.Slots[1];
        var samus = runtime.Player.IntroducedSamus;
        samus.CollectedItems = SamusEquipmentFlags.MorphBall;
        samus.InputLocked = true;
        samus.Position = samus.Position.WithWhole(new((ushort)(body.Position.X.Whole - 64), (ushort)(body.Position.Y.Whole - 32)));
        data.Game.GameState = SuperMetroidGameState.MainGameplay;
        for (long frame = 1; frame <= 80; frame++)
        {
            data.Record(0);
            var result = data.Game.StepCaptured(0, frame, data.Generation);
        }
        if (runtime.Display.DisplayedMorphBallEyeBeam is not { Phase: MorphBallEyeBeamPhase.Full })
            throw new InvalidDataException("Reported room eye did not reach a fully published beam.");
        Console.WriteLine(data.SaveSlot(9));
        Console.WriteLine($"Reported eye seed: {root}");
        return 0;
    }
}
