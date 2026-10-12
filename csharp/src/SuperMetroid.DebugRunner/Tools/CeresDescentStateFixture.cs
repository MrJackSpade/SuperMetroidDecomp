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
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        var setup = typeof(SuperMetroidGame).GetMethod("SetupSelectedGame", fields)
            ?? throw new MissingMethodException("Game setup");
        setup.Invoke(data.Game, null);
        // Seed the documented post-escape state, then let the actual frontend,
        // cinematic dispatcher and managed audio advance to the planet approach.
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!
            .SetValue(data.Game, SuperMetroidGameState.CeresGoesBoom);
        var sceneField = typeof(SuperMetroidGame).GetField("ceresDestruction", fields)!;
        for (long frame = 1; frame <= 2500; frame++)
        {
            data.Game.SetAudioAcknowledgements(data.Audio.ReadAcknowledgements());
            data.Record(0);
            var result = data.Game.StepCaptured(0, frame, data.Generation);
            data.Audio.RenderFrame(result.Frame.AudioCommands);
            object? scene = sceneField.GetValue(data.Game);
            string? phase = scene?.GetType().GetProperty("Phase")?.GetValue(scene)?.ToString();
            if (phase != "FlyingTowardZebesA") continue;
            Console.WriteLine(data.SaveSlot(9));
            Console.WriteLine($"Ceres descent seed: {root}");
            return 0;
        }
        throw new InvalidDataException("Ceres cinematic never reached the planet approach.");
    }
}
