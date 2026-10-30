using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
namespace SuperMetroid.DebugRunner.Tools;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.ScenarioFixtures;

/// <summary>Exports bounded production traces and seeds for native comparison of reported movement cases.</summary>
internal static class NativeCaptureSeeds
{
    /// <summary>
    /// Writes issue #415's production bomb-spread trace for a native comparison. Exclusive create
    /// keeps earlier evidence intact.
    /// </summary>
    internal static int WriteBombSpreadTrace(string outputPath, bool wallRoute)
    {
        using var output = new StreamWriter(new FileStream(outputPath, FileMode.CreateNew));
        output.WriteLine(BombSpreadTransitionScenario.TraceHeader);
        int frames = 0, bombRows = 0;
        foreach (bool left in new[] { false, true })
        for (int timingCase = 0; timingCase < BombSpreadTransitionScenario.TimingCaseCount(wallRoute); timingCase++)
        {
            var scenario = new BombSpreadTransitionScenario(RepositoryInstallation.CreateRuntime(), left, timingCase, wallRoute);
            for (int frame = 0; frame < scenario.FrameCount; frame++, frames++)
            {
                ControllerButton input = scenario.Input(frame);
                scenario.Runtime.StepFrame(input, new RuntimeFrameRequest());
                output.WriteLine(scenario.Row(frame, input));
                if (!scenario.RecordsBombs(frame))
                    continue;
                foreach (string bomb in scenario.BombRows())
                {
                    output.WriteLine(bomb);
                    bombRows++;
                }
            }
        }
        Console.WriteLine($"{Path.GetFullPath(outputPath)}: {frames} frames, {bombRows} bomb-slot observations.");
        return 0;
    }

    /// <summary>Exports issue #1258's room, takeoff seed and production jump rows for the native capture.</summary>
    internal static int ExportShallowWaterJumpSeed()
    {
        var scenario = new ShallowWaterJumpScenario(RepositoryInstallation.CreateRuntime(playerInvincibilityEnabled: true));
        var level = scenario.Runtime.Room.LoadedLevelData;
        string output = Path.GetFullPath("csharp/test-temp/issue-1258-water-jump");
        Directory.CreateDirectory(output);
        using (var writer = new BinaryWriter(File.Create(Path.Combine(output, "room.bin"))))
        {
            writer.Write(level.Size.X); writer.Write(level.Size.Y);
            // The native capture reads the room as the cartridge's level words and behaviour bytes.
            for (int index = 0; index < level.Size.Area; index++)
                writer.Write(RoomLevelWordEncoder.Encode(level.GetCollisionBlockByIndex(index).Terrain));
            for (int index = 0; index < level.Size.Area; index++)
                writer.Write(checked((byte)level.GetCollisionBlockByIndex(index).Behavior.Value));
        }
        File.WriteAllText(Path.Combine(output, "seed.txt"), scenario.TakeoffSeed());
        var (rows, apex) = scenario.Jump();
        File.WriteAllLines(Path.Combine(output, "managed.csv"), rows);
        Console.WriteLine($"{output}: room, takeoff seed and {rows.Count - 1} managed jump frames; apex={apex:X8}.");
        return 0;
    }
}
