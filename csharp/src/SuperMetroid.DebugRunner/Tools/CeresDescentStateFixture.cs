using System.Reflection;
using SuperMetroid.Android;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;

/// <summary>Creates a private, portable cinematic seed without changing any player's saves.</summary>
internal static class CeresDescentStateFixture
{
    public static int Export(string destination)
    {
        string root = Path.GetFullPath(destination);
        if (Directory.Exists(root))
            throw new IOException($"Refusing to overwrite an existing fixture directory: {root}");
        Directory.CreateDirectory(root);
        using var data = new AndroidSessionData(root, Path.GetFullPath("Super Metroid.smc"),
            Path.GetFullPath("standalone-assets/audio"));
        data.Game.SetupSelectedGame();
        // Seed the documented post-escape state, then let the actual frontend,
        // cinematic dispatcher and managed audio advance to the planet approach.
        data.Game.GameState = SuperMetroidGameState.CeresGoesBoom;
        for (long frame = 1; frame <= 2500; frame++)
        {
            data.Game.SetAudioAcknowledgements(data.Audio.ReadAcknowledgements());
            data.Record(0);
            var result = data.Game.StepCaptured(0, frame, data.Generation);
            data.Audio.RenderFrame(result.Frame.AudioCommands);
            if (data.Game.ceresDestruction?.Phase != CeresDestructionPhase.FlyingTowardZebesA) continue;
            Console.WriteLine(data.SaveSlot(9));
            Console.WriteLine($"Ceres descent seed: {root}");
            return 0;
        }
        throw new InvalidDataException("Ceres cinematic never reached the planet approach.");
    }
}
