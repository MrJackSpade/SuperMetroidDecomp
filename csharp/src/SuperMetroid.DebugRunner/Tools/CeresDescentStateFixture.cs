using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Areas.Ceres;
namespace SuperMetroid.DebugRunner.Tools;
using SuperMetroid.Diagnostics.Android;

/// <summary>Creates a private, portable cinematic seed without changing any player's saves.</summary>
internal static class CeresDescentStateFixture
{
    /// <summary>
    /// Creates a new private fixture directory, seeds the post-escape game state, then
    /// advances the real frontend, cinematic dispatcher, and audio until the planet
    /// approach before saving slot nine. Existing destinations are rejected.
    /// </summary>
    public static int Export(string destination)
    {
        string root = Path.GetFullPath(destination);
        if (Directory.Exists(root))
            throw new IOException($"Refusing to overwrite an existing fixture directory: {root}");
        Directory.CreateDirectory(root);
        using var data = new AndroidSessionData(root, Path.GetFullPath("Super Metroid.smc"),
            Path.GetFullPath("standalone-assets/audio"));
        data.Game.SetupSelectedGame();
        data.Game.GameState = SuperMetroidGameState.CeresGoesBoom;
        for (long frame = 1; frame <= 2500; frame++)
        {
            data.Record(0);
            var result = data.Game.StepCaptured(0, frame, data.Generation);
            if (data.Game.ceresDestruction?.Phase != CeresDestructionPhase.FlyingTowardZebesA) continue;
            Console.WriteLine(data.SaveSlot(9));
            Console.WriteLine($"Ceres descent seed: {root}");
            return 0;
        }
        throw new InvalidDataException("Ceres cinematic never reached the planet approach.");
    }
}
