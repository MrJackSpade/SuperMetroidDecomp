using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCeresRotation(byte[] rom)
    {
        int[] records = Oracle(rom, CeresShaftRotationDefinitions.ReferenceAddress, 69 * 3, 2, "RoomMainASM_CeresElevatorShaft");
        int exactCircleFailures = 0;
        for (int i = 0; i < 69; i++)
        {
            var actual = CeresShaftRotationDefinitions.Read((ushort)i);
            int sine = i - 34, magnitude = Math.Abs(sine);
            decimal angle = magnitude / 256m;
            int generatedSine = StableFloor(256 * FullSine(angle) + .5m, 256 * ResearchData.Error) * Math.Sign(sine);
            int generatedCosine = StableFloor(256 * FullCosine(angle) + .5m, 256 * ResearchData.Error);
            Equal((int)unchecked((short)records[3 * i + 1]), generatedSine, $"Ceres sine {i}");
            Equal(records[3 * i + 2], generatedCosine, $"Ceres cosine {i}");
            Equal(generatedCosine, CeresCosine(i), $"Ceres integer parabolic equivalent {i}");
            Equal((int)actual.Timer, records[3 * i], $"compiled Ceres timer {i}");
            Equal(actual.Sine, (ushort)records[3 * i + 1], $"compiled Ceres sine {i}");
            Equal(actual.Cosine, (ushort)records[3 * i + 2], $"compiled Ceres cosine {i}");
            // Exact integer test for rounding sqrt(256^2 - storedSine^2).
            int square = 65536 - sine * sine, rounded = 0;
            while ((2 * rounded + 1) * (2 * rounded + 1) <= 4 * square) rounded++;
            if (rounded != generatedCosine)
            {
                Equal(16, magnitude, $"Ceres exact-circle counterexample {i}");
                exactCircleFailures++;
            }
        }
        Equal(2, exactCircleFailures, "Ceres independently quantized coordinates are not an exact circle");
        CheckBounds(CeresCosine, 68);
        // Native 16-bit multiply-before-index semantics admit some high-bit aliases.
        int accepted = 0;
        for (int phase = 0; phase <= 65535; phase++)
        {
            int offset = 6 * phase & 65535;
            bool valid = offset % 6 == 0 && offset / 6 < 69;
            bool rejected = false;
            (ushort Timer, ushort Sine, ushort Cosine) row = default;
            try { row = CeresShaftRotationDefinitions.Read((ushort)phase); }
            catch (InvalidDataException) { rejected = true; }
            Equal(!valid, rejected, $"Ceres phase admission {phase}");
            if (!valid) continue;
            Equal(CeresCosine(offset / 6), (int)row.Cosine, $"Ceres aliased cosine {phase}");
            Equal(unchecked((ushort)(offset / 6 - 34)), row.Sine, $"Ceres aliased sine {phase}");
            accepted++;
        }
        Equal(138, accepted, "Ceres valid wrapped phases");
        Console.WriteLine("PASS: 138/138 Ceres rotation coefficients, independent angle quantization; all 65,536 phase aliases/rejections checked.");
    }

    private static int CeresCosine(int index)
    {
        Bound(index, 68);
        int s = index - 34;
        return 256 - (s * s + 255) / 512;
    }
}
