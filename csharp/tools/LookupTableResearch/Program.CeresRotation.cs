using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyCeresRotation(byte[] rom)
    {
        int[] records = Oracle(rom, 0x89ad5f, 69 * 3, 2, "RoomMainASM_CeresElevatorShaft");
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
            Equal(records[offset / 2], (int)row.Timer, $"Ceres aliased timer {phase}");
            Equal(records[offset / 2 + 2], (int)row.Cosine, $"Ceres aliased cosine {phase}");
            Equal((ushort)records[offset / 2 + 1], row.Sine, $"Ceres aliased sine {phase}");
            accepted++;
        }
        Equal(138, accepted, "Ceres valid wrapped phases");
        Console.WriteLine("PASS: 69/69 Ceres delay ramps, 138/138 rotation coefficients, independent angle quantization; all 65,536 phase aliases/rejections checked.");
    }
}
