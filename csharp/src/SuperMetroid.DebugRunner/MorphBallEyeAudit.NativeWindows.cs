using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class MorphBallEyeAudit
{
    public static int CompareNativeWindows(string romPath, string csvPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int checkedRows = 0, differingPixels = 0;
        bool exhaustive = File.ReadLines(csvPath).First().EndsWith(",originX,originY", StringComparison.Ordinal);
        var seen = new HashSet<(int Angle, int Width, int Y, int X)>();
        var caseDifferences = new SortedDictionary<(int Angle, int Width), int>();
        var captured = new Dictionary<(int Angle, int Width, int X), ScanlineColorAddRenderLayer>();
        foreach (string line in File.ReadLines(csvPath).Skip(1))
        {
            int[] v = line.Split(',').Select(int.Parse).ToArray();
            if (v.Length != (exhaustive ? 7 : 5)) throw new InvalidDataException("Malformed native eye-window row.");
            int y = v[2];
            int originX = exhaustive ? v[5] : 168;
            bool validCase = exhaustive ? v[0] is >= 0 and <= 255 && v[1] is (0 or 1 or 4) &&
                originX is (-16 or 168 or 272) && v[6] == 104 :
                v[0] is (160 or 176 or 192 or 208 or 224) && v[1] is (0 or 4);
            if (!validCase ||
                y is < 0 or >= SnesGameplayFrameRenderer.Height ||
                v[3] is < 0 or > 255 || v[4] is < 0 or > 255 ||
                !seen.Add((v[0], v[1], y, originX)))
                throw new InvalidDataException($"Unexpected or duplicate native eye-window row: {line}");
            if (y < SnesGameplayFrameRenderer.HudHeight) continue;
            var key = (v[0], v[1]);
            caseDifferences.TryAdd(key, 0);
            var captureKey = (v[0], v[1], originX);
            if (!captured.TryGetValue(captureKey, out var layer))
            {
                var beam = new MorphBallEyeBeamRenderSnapshot(MorphBallEyeBeamPhase.Full,
                    (ushort)(384 + originX), 616, SnesAngle.FromTableIndex((byte)v[0]), (ushort)v[1], 31, 31, 0);
                captured.Add(captureKey, layer = SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(bus, beam, 384, 512)!);
            }
            var actual = layer.Windows[y];
            for (int x = 0; x < 256; x++)
            {
                bool native = x >= v[3] && x <= v[4];
                bool managed = x >= actual.Left && x <= actual.Right;
                if (native == managed) continue;
                if (differingPixels < 16)
                    Console.WriteLine($"Eye mismatch: originX={originX} angle={v[0]} width={v[1]} ({x},{y}), " +
                        $"native=[{v[3]},{v[4]}], managed=[{actual.Left},{actual.Right}]");
                differingPixels++;
                caseDifferences[key]++;
            }
            checkedRows++;
        }
        foreach (var (key, count) in caseDifferences)
            if (!exhaustive || count != 0)
                Console.WriteLine($"Native eye angle={key.Angle}, width={key.Width}: {count} differing pixels.");
        int cases = exhaustive ? 256 * 3 * 3 : 10;
        if (seen.Count != cases * 224) throw new InvalidDataException($"Native eye fixture contained {seen.Count} unique rows, expected {cases * 224}.");
        if (checkedRows != cases * 192) throw new InvalidDataException($"Native eye fixture contained {checkedRows} gameplay rows, expected {cases * 192}.");
        if (differingPixels != 0)
            throw new InvalidDataException($"Eye window differs from executed ROM in {differingPixels} pixels across {checkedRows} rows.");
        Console.WriteLine($"Native eye window: {checkedRows} rows agree pixel-for-pixel.");
        return 0;
    }
}
