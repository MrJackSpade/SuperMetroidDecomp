using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Tooling;
using System.Text.Json;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks map-scroll arrow hold durations against native programs.</summary>
internal static class MapArrowDurationsVerification
{
    /// <summary>Compares four native cycles, bounds, and frame-by-frame arrow timing.</summary>
    internal static void VerifyMapArrowDurations(ISnesAddressSpace rom)
    {
        byte[] bytes = MapArrowExtractor.Extract(rom);
        var stock = MapArrowFile.Read(new MemoryStream(bytes));
        for (int index = 0; index < 4; index++)
        {
            int animation = ReadVerificationWord(rom, 0x81af36 + 10 * index);
            int program = 0x820000 | ReadVerificationWord(rom, 0x82c0e8 + 2 * (animation - 1));
            var visual = stock.Get((MapScrollDirection)(index + 1));
            int count = 0;
            while (rom.ReadCartridgeByte(program + 3 * count) != 255)
            {
                AssertEqual(rom.ReadCartridgeByte(program + 3 * count), visual.Duration(count), "original arrow duration");
                count++;
                AssertTrue(count <= 14, "original bounded arrow program");
            }
            AssertEqual(count, visual.PhaseCount, "original sentinel sets phase count");
            AssertEqual(0, visual.StoredDurationCount, "stock duration table eliminated");
            foreach (int invalid in new[] { int.MinValue, -1, count, 256, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => visual.Duration(invalid), "duration array boundary preserved");
        }
        Suite(nameof(VerifyMapArrowDurationTicks), () => VerifyMapArrowDurationTicks(rom, stock));
    }

    /// <summary>Checks visible and hidden ticks against the native left-arrow timing program.</summary>
    private static void VerifyMapArrowDurationTicks(ISnesAddressSpace rom, MapArrowPresentation presentation)
    {
        var actual = new FileSelectMapAnimations(presentation);
        var arrows = actual.arrows;
        int offset = 0, timer = 0;
        for (int tick = 0; tick < 90; tick++)
        {
            bool visible = tick is < 7 or >= 11;
            if (visible && --timer <= 0)
            {
                offset += 3;
                if (rom.ReadCartridgeByte(0x82c137 + offset) == 255) offset = 0;
                timer = rom.ReadCartridgeByte(0x82c137 + offset);
            }
            actual.StepArrows(_ => visible);
            foreach (var arrow in arrows)
            {
                AssertEqual(offset / 3, arrow.Frame, "actual arrow phase");
                AssertEqual(timer, arrow.Timer, "actual arrow delay and hidden pause");
            }
        }
    }
}
