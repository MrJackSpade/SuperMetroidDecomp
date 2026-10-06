using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMapArrowDurations(ISnesAddressSpace rom)
    {
        byte[] bytes = MapArrowExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<MapArrowDocument>(bytes,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
        var stock = MapArrowPresentation.Load(new MemoryStream(bytes));
        for (int index = 0; index < 4; index++)
        {
            int animation = ReadVerificationWord(rom, 0x81af36 + 10 * index);
            int program = 0x820000 | ReadVerificationWord(rom, 0x82c0e8 + 2 * (animation - 1));
            var visual = stock.Get((MapScrollDirection)(index + 1));
            int count = 0;
            while (rom.ReadByte(program + 3 * count) != 255)
            {
                AssertEqual(rom.ReadByte(program + 3 * count), visual.Duration(count), "original arrow duration");
                count++;
                AssertTrue(count <= 14, "original bounded arrow program");
            }
            AssertEqual(count, visual.PhaseCount, "original sentinel sets phase count");
            AssertEqual(0, visual.StoredDurationCount, "stock duration table eliminated");
            foreach (int invalid in new[] { int.MinValue, -1, count, 256, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => visual.Duration(invalid), "duration array boundary preserved");
            for (int phase = 0; phase < count; phase++)
            foreach (int delay in new[] { 1, 254 })
            {
                var entries = new Dictionary<string, MapArrowEntry>(document.Arrows);
                string name = ((MapScrollDirection)(index + 1)).ToString();
                int[] durations = (int[])entries[name].DurationTicks.Clone(); durations[phase] = delay;
                entries[name] = entries[name] with { DurationTicks = durations };
                var edited = Load(entries);
                for (int other = 1; other <= 4; other++)
                {
                    var actual = edited.Get((MapScrollDirection)other);
                    AssertEqual(other == index + 1 ? 1 : 0, actual.StoredDurationCount, "only edited phase stored");
                    int[] expected = entries[((MapScrollDirection)other).ToString()].DurationTicks;
                    for (int frame = 0; frame < expected.Length; frame++)
                        AssertEqual((byte)expected[frame], actual.Duration(frame), "every edited and unedited phase preserved");
                }
            }
        }
        foreach (int length in new[] { 1, 255 })
        {
            var entries = new Dictionary<string, MapArrowEntry>(document.Arrows);
            entries["Left"] = entries["Left"] with { DurationTicks = Enumerable.Repeat(254, length).ToArray() };
            var visual = Load(entries).Get(MapScrollDirection.Left);
            AssertEqual(length, visual.PhaseCount, "custom cycle length");
            AssertEqual(length, visual.StoredDurationCount, "fully authored cycle captured");
            for (int phase = 0; phase < length; phase++) AssertEqual((byte)254, visual.Duration(phase), "custom duration");
        }
        Suite(nameof(VerifyMapArrowDurationTicks), () => VerifyMapArrowDurationTicks(rom, stock));
        MapArrowPresentation Load(Dictionary<string, MapArrowEntry> entries)
        {
            using var json = new MemoryStream();
            MapArrowPresentation.Write(json, document with { Arrows = entries }); json.Position = 0;
            return MapArrowPresentation.Load(json);
        }
    }

    private static void VerifyMapArrowDurationTicks(ISnesAddressSpace rom, MapArrowPresentation presentation)
    {
        var actual = new FileSelectMapAnimations(new ForbiddenMapBus(), presentation);
        Array arrows = (Array)typeof(FileSelectMapAnimations).GetField("arrows", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(actual)!;
        int offset = 0, timer = 0;
        for (int tick = 0; tick < 90; tick++)
        {
            bool visible = tick is < 7 or >= 11;
            if (visible && --timer <= 0)
            {
                offset += 3;
                if (rom.ReadByte(0x82c137 + offset) == 255) offset = 0;
                timer = rom.ReadByte(0x82c137 + offset);
            }
            actual.StepArrows(_ => visible);
            foreach (object arrow in arrows)
            {
                AssertEqual(offset / 3, (int)arrow.GetType().GetField("Frame")!.GetValue(arrow)!, "actual arrow phase");
                AssertEqual(timer, (int)arrow.GetType().GetField("Timer")!.GetValue(arrow)!, "actual arrow delay and hidden pause");
            }
        }
    }
}
