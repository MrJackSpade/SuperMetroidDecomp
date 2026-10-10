using System.Text.RegularExpressions;
using SuperMetroid.Core.Audio;

internal static partial class Program
{
    /// <summary>Compares all 32 DSP rate periods with the pinned upstream initializer and checks invalid selector behavior.</summary>
    private static void VerifyDspRateAlgorithm()
    {
        // Independent hardware oracle: upstream-sm 578f90b3cc49557bb70060ad033bb90b8cf8ac50.
        // Read the original initializer, never reconstruct expected periods from the rule.
        string source = File.ReadAllText("upstream-sm/src/snes/dsp.c");
        Match table = Regex.Match(source, @"static const int rateValues\[32\] = \{([^}]+)\}");
        AssertEqual(true, table.Success, "Original DSP rate initializer exists");
        int[] original = Regex.Matches(table.Groups[1].Value, @"\d+")
            .Select(match => int.Parse(match.Value, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        AssertEqual(32, original.Length, "Original DSP rate count");
        for (int selector = 0; selector < original.Length; selector++)
            AssertEqual(original[selector], (int)SnesDspTables.RatePeriod(selector),
                $"Original DSP period selector {selector}");
        foreach (int invalid in new[] { int.MinValue, -1, 32, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SnesDspTables.RatePeriod(invalid),
                $"DSP rate invalid selector {invalid}");
    }
}
