using System.Numerics;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySpcPitchBasisAlgorithm(SuperMetroidAddressSpace rom)
    {
        BigInteger scale = BigInteger.Pow(10, 28);
        BigInteger lowerRoot = BigInteger.Parse("10594630943592952645618252949");
        BigInteger upperRoot = lowerRoot + 1;
        BigInteger radicand = 2 * BigInteger.Pow(scale, 12);
        AssertEqual(true, BigInteger.Pow(lowerRoot, 12) < radicand &&
            BigInteger.Pow(upperRoot, 12) > radicand, "Decimal ratio brackets twelfth root of two");
        for (int note = 0; note <= 12; note++)
        {
            int address = SpcMusicTables.BaseNoteReferenceAddress + note * 2;
            int original = rom.ReadByte(address) | (rom.ReadByte(address + 1) << 8);
            AssertEqual(original, (int)SpcMusicTables.BaseNoteFrequency(note), $"Original pitch basis {note}");
            // Exact rational bounds establish the equal-temperament explanation without libm.
            int steps = Math.Abs(note - 9);
            BigInteger loNumerator = 440 * 8192 * BigInteger.Pow(note < 9 ? scale : lowerRoot, steps);
            BigInteger hiNumerator = 440 * 8192 * BigInteger.Pow(note < 9 ? scale : upperRoot, steps);
            BigInteger loDenominator = 1000 * BigInteger.Pow(note < 9 ? upperRoot : scale, steps);
            BigInteger hiDenominator = 1000 * BigInteger.Pow(note < 9 ? lowerRoot : scale, steps);
            AssertEqual(original, (int)(loNumerator / loDenominator), $"Lower pitch interval {note}");
            AssertEqual(original, (int)(hiNumerator / hiDenominator), $"Upper pitch interval {note}");
            // A generous 1e-6 margin dwarfs decimal rounding over at most nine operations.
            AssertEqual(true, (loNumerator - original * loDenominator) * 1000000 > loDenominator &&
                ((original + 1) * hiDenominator - hiNumerator) * 1000000 > hiDenominator,
                $"Pitch decimal rounding margin {note}");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 13, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SpcMusicTables.BaseNoteFrequency(invalid),
                $"Pitch invalid semitone {invalid}");
    }
}
