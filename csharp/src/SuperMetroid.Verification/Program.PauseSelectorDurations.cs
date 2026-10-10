using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks editable pause-selector timing, cyclic phase lookup, initial-delay independence, and invalid-duration rejection.</summary>
    /// <param name="rom">Address space used to extract and compare the stock selector program and timing data.</param>
    private static void VerifyPauseSelectorDurations(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseSelectorExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<PauseSelectorDocument>(bytes, MapPresentationFormat.JsonOptions)!;
        var stock = PauseSelectorPresentation.Load(new MemoryStream(bytes));
        Suite(nameof(VerifyPauseSelectorNativeDurations), () => VerifyPauseSelectorNativeDurations(rom, stock));
        AssertEqual(0, stock.StoredDurationCount, "selector stock timing stores no phase values");
        for (int editedPhase = 0; editedPhase < stock.PhaseCount; editedPhase++)
        foreach (int delay in new[] { 1, 254 })
        {
            var phases = (PauseSelectorPhase[])document.Animation.Clone();
            phases[editedPhase] = phases[editedPhase] with { DurationTicks = delay };
            var edited = Load(document with { Animation = phases });
            AssertEqual(1, edited.StoredDurationCount, "only authored selector duration is stored");
            for (int phase = 0; phase < phases.Length * 2; phase++)
                AssertEqual(phases[phase % phases.Length].DurationTicks, edited.Duration(phase), "edited selector cyclic duration");
            AssertEqual(phases[int.MaxValue % phases.Length].DurationTicks, edited.Duration(int.MaxValue), "maximum cyclic phase");
            AssertEqual(stock.InitialDurationTicks, edited.InitialDurationTicks, "phase edit leaves initial timer intact");
        }
        foreach (int length in new[] { 1, 255 })
        {
            var phases = Enumerable.Range(0, length).Select(_ => document.Animation[0] with { DurationTicks = 254 }).ToArray();
            var edited = Load(document with { Animation = phases, InitialDurationTicks = 1 });
            AssertEqual(length, edited.PhaseCount, "custom selector cycle length");
            AssertEqual(length, edited.StoredDurationCount, "fully custom timing captured");
            AssertEqual(1, edited.InitialDurationTicks, "initial duration independently editable");
            for (int phase = 0; phase <= length; phase++) AssertEqual(254, edited.Duration(phase), "custom selector timing and wrap");
        }
        foreach (int invalid in new[] { -1, int.MinValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Duration(invalid), "negative selector phase rejected");
        foreach (int invalid in new[] { 0, -1, 255 })
        {
            var phases = (PauseSelectorPhase[])document.Animation.Clone(); phases[0] = phases[0] with { DurationTicks = invalid };
            AssertThrows<InvalidDataException>(() => Load(document with { Animation = phases }), "invalid duration rejected before capture");
        }
        static PauseSelectorPresentation Load(PauseSelectorDocument document)
        {
            using var stream = new MemoryStream(); PauseSelectorPresentation.Write(stream, document); stream.Position = 0;
            return PauseSelectorPresentation.Load(stream);
        }
    }

    /// <summary>Compares stock selector phase durations, the native terminator, and the separate initial delay with cartridge bytes.</summary>
    /// <param name="rom">Address space containing the native pause-selector program and initial-delay byte.</param>
    /// <param name="selector">Stock selector presentation whose timing values are checked against the cartridge.</param>
    private static void VerifyPauseSelectorNativeDurations(ISnesAddressSpace rom, PauseSelectorPresentation selector)
    {
        int program = 0x820000 | ReadVerificationWord(rom, 0x82c0ec);
        int phases = 0;
        while (rom.ReadByte(program + phases * 3) != 255)
        {
            AssertEqual((int)rom.ReadByte(program + phases * 3), selector.Duration(phases), "original selector duration");
            phases++;
            AssertTrue(phases <= 14, "original bounded selector program");
        }
        AssertEqual(phases, selector.PhaseCount, "selector native terminator");
        AssertEqual((int)rom.ReadByte(0x82c10c), selector.InitialDurationTicks, "separate native initial delay");
        AssertEqual((int)rom.ReadByte(program), selector.Duration(phases), "selector wraps before timing rule");
    }
}
