using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>Checks pause selector timing against native bytecode and rejection of invalid durations.</summary>
internal static class PauseSelectorDurationsVerification
{
    /// <summary>Checks stock timing and invalid durations.</summary>
    internal static void VerifyPauseSelectorDurations(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseSelectorExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseSelectorDocument>(bytes, ArtworkJson.Options);
        var stock = PauseSelectorSourcePresentation.Load(new MemoryStream(bytes));
        Suite(nameof(VerifyPauseSelectorNativeDurations), () => VerifyPauseSelectorNativeDurations(rom, stock));
        foreach (int invalid in new[] { -1, int.MinValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Duration(invalid), "negative selector phase rejected");
        foreach (int invalid in new[] { 0, -1, 255 })
        {
            var phases = (PauseSelectorPhase[])document.Animation.Clone(); phases[0] = phases[0] with { DurationTicks = invalid };
            AssertThrows<InvalidDataException>(() => Load(document with { Animation = phases }), "invalid duration rejected before capture");
        }
        static PauseSelectorPresentation Load(PauseSelectorDocument document)
        {
            using var stream = new MemoryStream(); PauseSelectorSourcePresentation.Write(stream, document); stream.Position = 0;
            return PauseSelectorSourcePresentation.Load(stream);
        }
    }

    /// <summary>Compares each installed selector duration with the native timed instruction list.</summary>
    internal static void VerifyPauseSelectorNativeDurations(ISnesAddressSpace rom, PauseSelectorPresentation selector)
    {
        int program = 0x820000 | ReadVerificationWord(rom, 0x82c0ec);
        int phases = 0;
        while (rom.ReadCartridgeByte(program + phases * 3) != 255)
        {
            AssertEqual(rom.ReadCartridgeByte(program + phases * 3), selector.Duration(phases), "original selector duration");
            phases++;
            AssertTrue(phases <= 14, "original bounded selector program");
        }
        AssertEqual(phases, selector.PhaseCount, "selector native terminator");
        AssertEqual(rom.ReadCartridgeByte(0x82c10c), selector.InitialDurationTicks, "separate native initial delay");
        AssertEqual(rom.ReadCartridgeByte(program), selector.Duration(phases), "selector wraps before timing rule");
    }
}
