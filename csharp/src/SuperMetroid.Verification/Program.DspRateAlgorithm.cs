using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Text.RegularExpressions;
using SuperMetroid.Core.Audio;

internal static class DspRateAlgorithmVerification
{
    internal static void VerifyDspRateAlgorithm()
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

    [VerificationCommand("--lookup-dsp-rate-algorithm")]
    internal static int LookupDspRateAlgorithmCommand()
    {
        Suite(nameof(VerifyDspRateAlgorithm), () => VerifyDspRateAlgorithm());
        Console.WriteLine("DSP rates: all 32 original hardware periods and invalid bounds pass.");
        return 0;
    }
}
