using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

// Research only: this executable is not referenced by the game. Run from repo root:
// dotnet run --project csharp/tools/LookupTableResearch -- "Super Metroid.smc"
internal static partial class Program
{
    private static int Main(string[] args)
    {
        if (OperatingSystem.IsWindows())
            NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);
        try
        {
            byte[] rom = File.ReadAllBytes(args.Length == 0 ? "Super Metroid.smc" : args.Single());
            Equal(ResearchData.RomSha256, Convert.ToHexString(SHA256.HashData(rom)), "NTSC J/U v1.0 oracle identity");
            Verify(rom);
            VerifyGeometry(rom);
            VerifyAudio(rom);
            VerifySuitCurve(rom);
            VerifyPolicies(rom);
            VerifyGaussian();
            VerifyGrapple(rom);
            VerifyEffectPatterns(rom);
            VerifyCeresRotation(rom);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    private static void Verify(byte[] rom)
    {
        // The oracle is read independently from both cartridge bytes and pinned assembly.
        int[] bytes = Oracle(rom, ResearchData.ByteSine, 128, 1);
        int[] words = Oracle(rom, ResearchData.UnsignedSine, 128, 2);
        int[] signed = Oracle(rom, ResearchData.SignedSine, 320, 2);
        int[] sixteen = Oracle(rom, ResearchData.SixteenBitSine, 256, 2);
        int[] tangent = Oracle(rom, ResearchData.Tangent, 129, 2);
        int[] widths = Oracle(rom, ResearchData.Widths, 32, 1);
        int[] tops = Oracle(rom, ResearchData.TopOffsets, 32, 1);
        int[] shaktool = Oracle(rom, ResearchData.Shaktool, 320, 2);

        for (int i = 0; i < 128; i++)
        {
            Equal(bytes[i], (int)EnemyTrigonometryTables.EightBitHalfWave[i], $"compiled byte {i}");
            Equal(words[i], (int)EnemyTrigonometryTables.UnsignedHalfWave[i], $"compiled unsigned {i}");
            Equal(bytes[i], ByteSine(i), $"algorithm byte {i}");
            Equal(words[i], UnsignedSine(i), $"algorithm unsigned {i}");
        }
        for (int i = 0; i < 320; i++)
        {
            int angle = (i + 192) & 255;
            int magnitude = angle % 128 == 64 ? 256 : ByteSine(angle % 128);
            int value = angle < 128 ? magnitude : -magnitude;
            Equal(unchecked((short)signed[i]), EnemyTrigonometryTables.SignedNegativeCosineWord(i), $"compiled signed {i}");
            Equal((int)unchecked((short)signed[i]), value, $"algorithm signed {i}");
        }
        for (int i = 0; i < 256; i++)
        {
            int magnitude = UnsignedSine(i % 128) >> 1;
            Equal((int)unchecked((short)sixteen[i]), i < 128 ? magnitude : -magnitude, $"algorithm sixteen-bit {i}");
            Equal(unchecked((short)sixteen[i]), EnemyTrigonometryTables.SignedSixteenBitSine((byte)i), $"compiled sixteen-bit {i}");
            var displacement = ShaktoolOrbitTables.Displacement((byte)i);
            Equal(unchecked((short)shaktool[i]) << 8, displacement.Y, $"compiled Shaktool Y {i}");
            Equal(unchecked((short)shaktool[i + 64]) << 8, displacement.X, $"compiled Shaktool X {i}");
        }
        var tangentExceptions = new List<int>();
        for (int i = 0; i < 129; i++)
        {
            Equal(tangent[i], (int)AbsoluteTangentDefinitions.Sample(i), $"compiled tangent {i}");
            if (tangent[i] != Tangent(i)) tangentExceptions.Add(i);
            Equal(tangent[i], AuthoredTangent(i), $"reduced-pi tangent {i}");
        }
        Equal("63,65", string.Join(',', tangentExceptions), "true tangent counterexamples");
        Equal(10427, tangent[63], "native tangent counterexample");
        Equal(10428, Tangent(63), "mathematical tangent counterexample");
        for (int i = 0; i < 32; i++)
        {
            Equal(widths[i], (int)PowerBombShapeDefinitions.Widths[i], $"compiled width {i}");
            Equal(tops[i], (int)PowerBombShapeDefinitions.TopOffsets[i], $"compiled top {i}");
            Equal(widths[i], Width(i), $"algorithm width {i}");
            Equal(tops[i], Top(i), $"algorithm top {i}");
        }

        var exceptions = new List<int>();
        for (int i = 0; i < 320; i++)
        {
            int angle = (i + 192) & 255;
            int half = angle % 128;
            int magnitude = half == 64 ? 3071 : StableFloor(3072 * Sine(half), 3072 * ResearchData.Error);
            int candidate = angle < 128 ? magnitude : -magnitude;
            if (candidate != unchecked((short)shaktool[i])) exceptions.Add(i);
            Equal((int)unchecked((short)shaktool[i]), Shaktool(i), $"reduced-pi full-cycle Shaktool {i}");
        }
        Equal("37,38,61,293,294,317", string.Join(',', exceptions), "Shaktool saturated 3072*sin counterexamples");
        // The same phase has differing magnitudes across quadrants: no single odd
        // sine with uniform scale and truncation can produce this entire table.
        Equal(-1890, (int)unchecked((short)shaktool[37]), "Shaktool negative quadrant witness");
        Equal(1889, shaktool[165], "Shaktool positive quadrant witness");

        CheckBounds(ByteSine, 127);
        CheckBounds(UnsignedSine, 127);
        CheckBounds(Tangent, 128);
        CheckBounds(AuthoredTangent, 128);
        CheckBounds(Shaktool, 319);
        CheckBounds(Width, 31);
        CheckBounds(Top, 31);
        Console.WriteLine("PASS: 1,345/1,345 exact algorithm outputs: sine 832, Power Bomb 64, tangent 129, Shaktool 320.");
        Console.WriteLine("PASS: all 1,345 oracle entries match compiled definitions, ROM and pinned assembly.");
        Console.WriteLine("PASS: candidate bounds; true-pi alternatives fail at tangent 63,65 and six Shaktool words.");
        Console.WriteLine("Production tables and runtime behavior remain unchanged. Caller migration/performance are future work.");
    }

    // Decimal arithmetic avoids platform libm. Reflection confines x to [0, pi/2].
    // Twelve alternating terms through x^23/23!: omitted term <= (pi/2)^25/25!
    // < 5.2e-21. Pi rounding and decimal operation rounding are covered by 1e-19.
    // Exact endpoints bypass series error at integer quantization boundaries.
    private static decimal Sine(int halfIndex)
    {
        if ((uint)halfIndex > 128) throw new ArgumentOutOfRangeException(nameof(halfIndex));
        int n = Math.Min(halfIndex, 128 - halfIndex);
        if (n == 0) return 0;
        if (n == 64) return 1;
        decimal x = n * ResearchData.Pi / 128;
        decimal term = x, sum = x;
        for (int k = 1; k < 12; k++)
        {
            term = -term * x * x / ((2 * k) * (2 * k + 1));
            sum += term;
        }
        return sum;
    }

    private static int StableFloor(decimal value, decimal error)
    {
        // Prove quantization of the whole error interval, not just a close value.
        if (value == 0) return 0;
        decimal lower = decimal.Floor(value - error), upper = decimal.Floor(value + error);
        Equal(lower, upper, $"quantization interval around {value}");
        return (int)lower;
    }

    private static void Bound(int i, int maximum)
    {
        if ((uint)i > (uint)maximum) throw new ArgumentOutOfRangeException(nameof(i));
    }

    private static int ByteSine(int i)
    {
        Bound(i, 127);
        return i == 64 ? 255 : StableFloor(256 * Sine(i), 256 * ResearchData.Error);
    }

    private static int UnsignedSine(int i)
    {
        Bound(i, 127);
        return i == 64 ? 65535 : StableFloor(65535 * Sine(i), 65535 * ResearchData.Error);
    }

    private static int Tangent(int i)
    {
        Bound(i, 128);
        int n = Math.Min(i, 128 - i);
        if (n == 64) return 15360;
        if (n == 32) return 256;
        decimal a = Sine(n), b = Sine(64 - n), e = ResearchData.Error;
        if (n == 0) return 0;
        decimal lower = decimal.Floor(256 * (a - e) / (b + e));
        decimal upper = decimal.Floor(256 * (a + e) / (b - e));
        Equal(lower, upper, $"tangent quantization interval {i}");
        return (int)lower;
    }

    private static int Width(int i) { Bound(i, 31); return ByteSine(2 * i); }
    private static int Top(int i) { Bound(i, 31); return ByteSine(63 - 2 * i) * 3 / 4; }

    // Full-cycle evaluation is essential: reducing the INDEX to a quadrant using
    // the approximate pi erases the phase drift that explains Shaktool's values.
    // On [0,2*pi], 24 terms through x^47/47! leave < 8e-24 remainder;
    // decimal rounding is conservatively covered by the same 1e-19 interval.
    private static decimal FullSine(decimal x)
    {
        if (x < 0 || x > 2 * ResearchData.Pi) throw new ArgumentOutOfRangeException(nameof(x));
        decimal term = x, sum = x;
        for (int k = 1; k < 24; k++)
        {
            term = -term * x * x / ((2 * k) * (2 * k + 1));
            sum += term;
        }
        return sum;
    }

    private static int Shaktool(int i)
    {
        Bound(i, 319);
        int angle = (i + 192) % 256;
        if (angle == 0) return 0;
        decimal value = 3072 * FullSine(angle * ResearchData.GeneratorPi / 128);
        decimal lower = decimal.Truncate(value - 3072 * ResearchData.Error);
        decimal upper = decimal.Truncate(value + 3072 * ResearchData.Error);
        Equal(lower, upper, $"Shaktool quantization interval {i}");
        return (int)lower;
    }

    private static int AuthoredTangent(int i)
    {
        Bound(i, 128);
        int n = Math.Min(i, 128 - i);
        if (n == 0) return 0;
        if (n == 32) return 256; // Exact diagonal, not the approximate-pi quotient.
        if (n == 64) return 15360; // Authored finite infinity substitute.
        decimal x = n * ResearchData.GeneratorPi / 128;
        decimal a = FullSine(x), b = FullSine(ResearchData.Pi / 2 - x), e = ResearchData.Error;
        decimal lower = decimal.Floor(256 * (a - e) / (b + e));
        decimal upper = decimal.Floor(256 * (a + e) / (b - e));
        Equal(lower, upper, $"reduced-pi tangent quantization interval {i}");
        return (int)lower;
    }

    private static void CheckBounds(Func<int, int> candidate, int maximum)
    {
        foreach (int invalid in new[] { int.MinValue, -1, maximum + 1, int.MaxValue })
        {
            try { candidate(invalid); }
            catch (ArgumentOutOfRangeException) { continue; }
            throw new InvalidDataException($"Candidate accepted invalid index {invalid}.");
        }
    }

    private static int[] Oracle(byte[] rom, int address, int count, int size, string? label = null)
    {
        string assembly = File.ReadAllText($"upstream-disassembly/src/bank_{address >> 16:X2}.asm");
        var asmBytes = new Dictionary<int, byte>();
        if (label is not null)
        {
            int start = assembly.IndexOf(label + ':', StringComparison.Ordinal);
            if (start < 0) throw new InvalidDataException($"Missing pinned assembly label {label}.");
            int cursor = address;
            foreach (Match line in Regex.Matches(assembly[start..], @"(?m)^\s*d([bw])\s+(\$[\dA-Fa-f]{2,4}(?:,\s*\$[\dA-Fa-f]{2,4})*)"))
            {
                foreach (string token in line.Groups[2].Value.Split(','))
                {
                    int value = Convert.ToInt32(token.Trim()[1..], 16);
                    asmBytes[cursor++] = (byte)value;
                    if (line.Groups[1].Value == "w") asmBytes[cursor++] = (byte)(value >> 8);
                }
                if (cursor >= address + count * size) break;
            }
        }
        foreach (Match line in Regex.Matches(assembly, @"(?m)^\s*d([bw])\s+(\$[\dA-Fa-f]{2,4}(?:,\$[\dA-Fa-f]{2,4})*)\s*;([\dA-Fa-f]{6});"))
        {
            int cursor = Convert.ToInt32(line.Groups[3].Value, 16);
            foreach (string token in line.Groups[2].Value.Split(','))
            {
                int value = Convert.ToInt32(token[1..], 16);
                asmBytes[cursor++] = (byte)value;
                if (line.Groups[1].Value == "w") asmBytes[cursor++] = (byte)(value >> 8);
            }
        }
        var result = new int[count];
        for (int i = 0; i < count * size; i++)
        {
            int bus = address + i;
            byte value = rom[((bus >> 16 & 0x7f) << 15) | (bus & 0x7fff)];
            Equal(value, asmBytes[bus], $"ROM/assembly ${bus:X6}");
            result[i / size] |= value << (8 * (i % size));
        }
        return result;
    }

    private static void Equal<T>(T expected, T actual, string context)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidDataException($"{context}: expected {expected}, actual {actual}.");
    }

    private static partial class NativeConsoleProcess
    {
        [LibraryImport("kernel32.dll")]
        internal static partial uint SetErrorMode(uint errorMode);
    }
}

internal static class ResearchData
{
    /// <summary>Unheadered NTSC J/U v1.0 cartridge used with the disassembly pin in README.md.</summary>
    internal const string RomSha256 = "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72";
    /// <summary>$A0:B143, SineCosineTables_8bitSine plus cosine continuation, 128 bytes.</summary>
    internal const int ByteSine = 0xa0b143;
    /// <summary>$A0:B7EE, UnsignedSineTable, 128 little-endian words.</summary>
    internal const int UnsignedSine = 0xa0b7ee;
    /// <summary>$A0:B3C3, SineCosineTables_NegativeCosine_SignExtended and full sine, 320 words.</summary>
    internal const int SignedSine = 0xa0b3c3;
    /// <summary>$A0:B1C3, SineCosineTables_16bitSine and continuations, 256 words.</summary>
    internal const int SixteenBitSine = 0xa0b1c3;
    /// <summary>$91:C9D4, AbsoluteTangentTable, 129 words including final zero.</summary>
    internal const int Tangent = 0x91c9d4;
    /// <summary>$88:A266, PowerBombExplosion_ShapeDefinitionTable_Unscaled_width, 32 bytes.</summary>
    internal const int Widths = 0x88a266;
    /// <summary>$88:A286, PowerBombExplosion_ShapeDefinitionTable_Unscaled_topOffset, 32 bytes.</summary>
    internal const int TopOffsets = 0x88a286;
    /// <summary>$AA:E03D, SineCosineTables_negativeCosine and continuations, 320 words.</summary>
    internal const int Shaktool = 0xaae03d;
    /// <summary>Pi rounded to 28 decimal places, independent of platform libm.</summary>
    internal const decimal Pi = 3.1415926535897932384626433833m;
    /// <summary>Short decimal pi constant reproducing tangent and full-cycle Shaktool samples.</summary>
    internal const decimal GeneratorPi = 3.14159m;
    /// <summary>Conservative absolute bound for the reflected twelve-term decimal sine evaluation.</summary>
    internal const decimal Error = 0.0000000000000000001m;
}
