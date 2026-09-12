using System.Globalization;
using SuperMetroid.Core.Input;

/// <summary>Changes only the initial steering pulse; preserves all bomb timestamps.</summary>
internal static class BombTraversalBoundarySearch
{
    public static int Run(string rom, string inputPath, string outputDirectory)
    {
        var original = File.ReadLines(inputPath).Skip(1).Select(line => ushort.Parse(line.Split(',')[1], NumberStyles.HexNumber)).ToArray();
        if (original.Length != 600) throw new InvalidDataException("Expected the fixed 600-frame ceiling recording.");
        if (original[170] != (ushort)SnesButton.Right || original[171] != (ushort)SnesButton.Right)
            throw new InvalidDataException("Baseline must have the captured two-frame initial steering pulse.");
        Directory.CreateDirectory(outputDirectory);
        // Keep this local boundary search within the constructed room. Larger
        // perturbations can leave its left edge; they are not valid chain fixtures.
        for (int start = 169; start <= 171; start++)
        for (int duration = 1; duration <= 3; duration++)
        {
            var inputs = (ushort[])original.Clone();
            inputs[170] &= unchecked((ushort)~(ushort)SnesButton.Right);
            inputs[171] &= unchecked((ushort)~(ushort)SnesButton.Right);
            for (int frame = start; frame < start + duration; frame++) inputs[frame] |= (ushort)SnesButton.Right;
            var runtime = BombTraversalFixture.Create(rom, ceiling: true);
            var samus = runtime.Samus!;
            int launches = 0, floorContacts = 0;
            for (int frame = 0; frame < inputs.Length; frame++)
            {
                runtime.StepFrame(inputs[frame]);
                if (runtime.LastBombJumpMovement is { Started: true }) launches++;
                if (frame > 170 && samus.YPosition >= 249) floorContacts++;
            }
            Console.WriteLine($"Pulse start={start}, duration={duration}: launches={launches}, floor={floorContacts}, X={samus.Kinematics.XFixed:X8}, Y={samus.Kinematics.YFixed:X8}");
            string path = Path.Combine(outputDirectory, $"ceiling-pulse-{start}-{duration}.csv");
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            using var writer = new StreamWriter(stream);
            writer.WriteLine("frame,input");
            for (int frame = 0; frame < inputs.Length; frame++) writer.WriteLine($"{frame},{inputs[frame]:X4}");
        }
        return 0;
    }
}
