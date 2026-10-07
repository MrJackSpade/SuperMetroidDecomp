internal static partial class AssetTools
{
    /// <summary>
    /// Writes issue #415's production bomb-spread trace for a native comparison. Exclusive create
    /// keeps earlier evidence intact.
    /// </summary>
    internal static int WriteBombSpreadTrace(string outputPath, bool wallRoute)
    {
        var bus = LoadRepositoryRom();
        using var output = new StreamWriter(new FileStream(outputPath, FileMode.CreateNew));
        output.WriteLine(BombSpreadTransitionScenario.TraceHeader);
        int frames = 0, bombRows = 0;
        foreach (bool left in new[] { false, true })
        for (int timingCase = 0; timingCase < BombSpreadTransitionScenario.TimingCaseCount(wallRoute); timingCase++)
        {
            var scenario = new BombSpreadTransitionScenario(RepositoryInstallation.CreateRuntime(bus), bus, left, timingCase, wallRoute);
            for (int frame = 0; frame < scenario.FrameCount; frame++, frames++)
            {
                ushort input = scenario.Input(frame);
                scenario.Runtime.StepFrame(input);
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
        var bus = LoadRepositoryRom();
        var scenario = new ShallowWaterJumpScenario(RepositoryInstallation.CreateRuntime(bus, playerInvincibilityEnabled: true), bus);
        var level = scenario.Runtime.LevelData!;
        string output = Path.GetFullPath("csharp/test-temp/issue-1258-water-jump");
        Directory.CreateDirectory(output);
        using (var writer = new BinaryWriter(File.Create(Path.Combine(output, "room.bin"))))
        {
            writer.Write((ushort)level.WidthInBlocks); writer.Write((ushort)level.HeightInBlocks);
            foreach (ushort tile in level.ForegroundEntries.Span) writer.Write(tile);
            writer.Write(level.BehaviorBytes.Span);
        }
        File.WriteAllText(Path.Combine(output, "seed.txt"), scenario.TakeoffSeed());
        var (rows, apex) = scenario.Jump();
        File.WriteAllLines(Path.Combine(output, "managed.csv"), rows);
        Console.WriteLine($"{output}: room, takeoff seed and {rows.Count - 1} managed jump frames; apex={apex:X8}.");
        return 0;
    }
}
