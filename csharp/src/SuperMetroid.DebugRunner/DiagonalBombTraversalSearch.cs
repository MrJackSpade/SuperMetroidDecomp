using SuperMetroid.Core.Input;

/// <summary>Searches controller timings around Sweetnumb's demonstrated three-bomb crossing.</summary>
internal static class DiagonalBombTraversalSearch
{
    public static int WriteAdjacentMiss(string inputPath, string outputPath)
    {
        var inputs = File.ReadLines(inputPath).Skip(1).Select(line => ushort.Parse(line.Split(',')[1], System.Globalization.NumberStyles.HexNumber)).ToArray();
        if (inputs.Length != 180 || inputs[102] != (ushort)SnesButton.Right || inputs[103] != 0)
            throw new InvalidDataException("Expected the captured frame-102 forward tap.");
        // Delay the forward turn by one frame, extending the preceding return
        // input. All bomb placements and later crossing inputs remain untouched.
        inputs[102] = (ushort)SnesButton.Left;
        inputs[103] = (ushort)SnesButton.Right;
        using var writer = new StreamWriter(new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write));
        writer.WriteLine("frame,input");
        for (int frame = 0; frame < inputs.Length; frame++) writer.WriteLine($"{frame},{inputs[frame]:X4}");
        return 0;
    }

    public static int Run(string rom, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        int candidates = 0;
        int bestLaunchCount = 0;
        for (int initialTap = 1; initialTap <= 3; initialTap++)
        for (int second = 50; second <= 55; second++)
        for (int third = 68; third <= 83; third++)
        for (int returnFrames = 10; returnFrames <= 35; returnFrames++)
        {
            var runtime = BombTraversalFixture.Create(rom, ceiling: false);
            var samus = runtime.Samus!;
            var inputs = new List<ushort>();
            var launchFrames = new List<int>();
            int floorReturn = -1;
            for (int frame = 0; frame < 180; frame++)
            {
                ushort input = frame == 0 || frame == second || frame == third ? runtime.ControllerBindings.Shoot : (ushort)0;
                // The guide moves beside bomb one, leaves the first boost alone,
                // returns after bomb three, taps forward, then waits neutrally
                // for bomb two to catch before committing to the crossing.
                if (frame >= 1 && frame <= initialTap || frame == third + returnFrames + 1 || launchFrames.Count >= 2)
                    input |= (ushort)SnesButton.Right;
                else if (frame > third && frame <= third + returnFrames)
                    input |= (ushort)SnesButton.Left;
                inputs.Add(input);
                runtime.StepFrame(input);
                if (runtime.LastBombJumpMovement is { Started: true }) launchFrames.Add(frame);
                if (frame > 60 && samus.YPosition >= 249 && floorReturn < 0) floorReturn = frame;
            }
            if (launchFrames.Count > bestLaunchCount)
            {
                bestLaunchCount = launchFrames.Count;
                Console.WriteLine($"Best launches={string.Join(',', launchFrames)}, tap={initialTap}, bombs={second},{third}, return={returnFrames}, floor={floorReturn}, x={samus.XPosition}");
            }
            if (launchFrames.Count < 3 || floorReturn >= 0 && launchFrames[2] >= floorReturn || samus.XPosition < 180) continue;
            string path = Path.Combine(outputDirectory, $"diagonal-tap-{initialTap}-bombs-{second}-{third}-return-{returnFrames}.csv");
            using var writer = new StreamWriter(new FileStream(path, FileMode.CreateNew, FileAccess.Write));
            writer.WriteLine("frame,input");
            for (int frame = 0; frame < inputs.Count; frame++) writer.WriteLine($"{frame},{inputs[frame]:X4}");
            Console.WriteLine($"Candidate {Path.GetFileName(path)}: launches={string.Join(',', launchFrames)}, floorReturn={floorReturn}, x={samus.XPosition}, y={samus.YPosition}");
            candidates++;
        }
        Console.WriteLine($"Demonstrated-policy candidates: {candidates}; independent cartridge replay still required.");
        return candidates > 0 ? 0 : 1;
    }
}
