using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Animation;
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
        PauseSelectorPresentation stock = PauseSelectorSourcePresentation.Load(new MemoryStream(bytes)).Presentation;
        Suite(nameof(VerifyPauseSelectorNativeDurations), () => VerifyPauseSelectorNativeDurations(rom, stock));
        // A phase lasts at least one tick; the import leaves the cartridge's byte limit to its decoder (#2563).
        foreach (int invalid in new[] { 0, -1 })
        {
            var phases = (PauseSelectorPhase[])document.Animation.Clone(); phases[0] = phases[0] with { DurationTicks = invalid };
            AssertThrows<InvalidDataException>(() => Load(document with { Animation = phases }), "invalid duration rejected before capture");
        }
        static PauseSelectorPresentation Load(PauseSelectorDocument document)
        {
            using var stream = new MemoryStream(); PauseSelectorSourcePresentation.Write(stream, document); stream.Position = 0;
            return PauseSelectorSourcePresentation.Load(stream).Presentation;
        }
    }

    /// <summary>Compares each installed selector duration with the native timed instruction list.</summary>
    internal static void VerifyPauseSelectorNativeDurations(ISnesAddressSpace rom, PauseSelectorPresentation selector)
    {
        int program = 0x820000 | ReadVerificationWord(rom, 0x82c0ec);
        int phases = 0;
        int[] durations = [.. selector.Segments.Beats(PauseSelectorSegment.Cycle)
            .Select(beat => AssertIs<AnimationBeat<PauseSelectorLook, NoAnimationCue>.Frame>(beat, "each selector step shows a look").Duration)];
        while (rom.ReadCartridgeByte(program + phases * 3) != 255)
        {
            AssertEqual(rom.ReadCartridgeByte(program + phases * 3), durations[phases], "original selector duration");
            phases++;
            AssertTrue(phases <= 14, "original bounded selector program");
        }
        AssertEqual(phases, durations.Length, "selector native terminator");
        AssertEqual(phases, selector.Looks.Count, "selector shows one look a step");
        AssertEqual(rom.ReadCartridgeByte(0x82c10c), selector.InitialDurationTicks, "separate native initial delay");
        AssertEqual(new AnimationEnd.Loop(0), selector.Cycling.End, "selector wraps to its first step after its last");
    }
}
