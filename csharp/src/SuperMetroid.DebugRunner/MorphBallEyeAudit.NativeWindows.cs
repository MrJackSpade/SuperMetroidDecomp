using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class MorphBallEyeAudit
{
    public static int CompareNativeWindows(string romPath, string csvPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int checkedRows = 0, differingPixels = 0;
        var seen = new HashSet<(int Angle, int Width, int Y)>();
        var caseDifferences = new SortedDictionary<(int Angle, int Width), int>();
        foreach (string line in File.ReadLines(csvPath).Skip(1))
        {
            int[] v = line.Split(',').Select(int.Parse).ToArray();
            if (v.Length != 5) throw new InvalidDataException("Malformed native eye-window row.");
            int y = v[2];
            if (v[0] is not (160 or 176 or 192 or 208 or 224) || v[1] is not (0 or 4) ||
                y is < 0 or >= SnesGameplayFrameRenderer.Height ||
                v[3] is < 0 or > 255 || v[4] is < 0 or > 255 ||
                !seen.Add((v[0], v[1], y)))
                throw new InvalidDataException($"Unexpected or duplicate native eye-window row: {line}");
            if (y < SnesGameplayFrameRenderer.HudHeight) continue;
            var key = (v[0], v[1]);
            caseDifferences.TryAdd(key, 0);
            var beam = new MorphBallEyeBeamRenderSnapshot(MorphBallEyeBeamPhase.Full,
                552, 616, SnesAngle.FromTableIndex((byte)v[0]), (ushort)v[1], 31, 31, 0);
            var actual = SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(bus, beam, 384, 512)!
                .Windows[y];
            for (int x = 0; x < 256; x++)
            {
                bool native = x >= v[3] && x <= v[4];
                bool managed = x >= actual.Left && x <= actual.Right;
                if (native == managed) continue;
                if (differingPixels < 16)
                    Console.WriteLine($"Eye mismatch: angle={v[0]} width={v[1]} ({x},{y}), " +
                        $"native=[{v[3]},{v[4]}], managed=[{actual.Left},{actual.Right}]");
                differingPixels++;
                caseDifferences[key]++;
            }
            checkedRows++;
        }
        foreach (var (key, count) in caseDifferences)
            Console.WriteLine($"Native eye angle={key.Angle}, width={key.Width}: {count} differing pixels.");
        if (seen.Count != 2240) throw new InvalidDataException($"Native eye fixture contained {seen.Count} unique rows, expected 2240.");
        if (checkedRows != 1920) throw new InvalidDataException($"Native eye fixture contained {checkedRows} gameplay rows, expected 1920.");
        if (differingPixels != 0)
            throw new InvalidDataException($"Eye window differs from executed ROM in {differingPixels} pixels across {checkedRows} rows.");
        Console.WriteLine($"Native eye window: {checkedRows} rows agree pixel-for-pixel.");
        return 0;
    }
}
