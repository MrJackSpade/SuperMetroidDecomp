using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyMapArrowCases(ISnesAddressSpace rom)
    {
        byte[] bytes = MapArrowExtractor.Extract(rom);
        var original = MapArrowPresentation.Load(new MemoryStream(bytes));
        VerifyMapArrowXSelection(rom, original);
        VerifyMapArrowYSelection(rom, original);
        VerifyMapArrowShapeCases(rom);
        var document = JsonSerializer.Deserialize<MapArrowDocument>(bytes,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
        for (int selected = 1; selected <= 4; selected++)
        {
            var direction = (MapScrollDirection)selected;
            var entries = new Dictionary<string, MapArrowEntry>(document.Arrows);
            entries[direction.ToString()] = new() { X = 250 + selected, Y = 220 - selected, DurationTicks = [selected, 254] };
            using var json = new MemoryStream();
            MapArrowPresentation.Write(json, document with { Arrows = entries }); json.Position = 0;
            var edited = MapArrowPresentation.Load(json);
            for (int other = 1; other <= 4; other++)
            {
                var actual = edited.Get((MapScrollDirection)other);
                var expected = entries[((MapScrollDirection)other).ToString()];
                AssertEqual((ushort)expected.X, actual.X, "independent direction X edit");
                AssertEqual((ushort)expected.Y, actual.Y, "independent direction Y edit");
                AssertEqual(expected.DurationTicks.Length, actual.PhaseCount, "selected direction phase count");
                for (int phase = 0; phase < actual.PhaseCount; phase++)
                    AssertEqual((byte)expected.DurationTicks[phase], actual.Duration(phase), "selected direction durations");
                AssertTrue(ReferenceEquals(actual, edited.Get((MapScrollDirection)other)), "direction selection preserves visual identity");
            }
        }
        foreach (int invalid in new[] { int.MinValue, -1, 0, 5, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => original.Get((MapScrollDirection)invalid), "invalid arrow direction");
        foreach (string name in document.Arrows.Keys)
        {
            var missing = new Dictionary<string, MapArrowEntry>(document.Arrows);
            missing.Remove(name);
            AssertThrows<InvalidDataException>(() => MapArrowPresentation.Write(new MemoryStream(), document with { Arrows = missing }),
                "all four named directions remain required");
        }
    }

    private static void VerifyMapArrowXSelection(ISnesAddressSpace rom, MapArrowPresentation presentation)
    {
        for (int index = 0; index < 4; index++)
            AssertEqual(ReadVerificationWord(rom, 0x81af32 + 10 * index),
                presentation.Get((MapScrollDirection)(index + 1)).X, "original arrow X selected by direction");
    }

    private static void VerifyMapArrowYSelection(ISnesAddressSpace rom, MapArrowPresentation presentation)
    {
        for (int index = 0; index < 4; index++)
            AssertEqual((ushort)(ReadVerificationWord(rom, 0x81af34 + 10 * index) - 1),
                presentation.Get((MapScrollDirection)(index + 1)).Y, "original arrow Y selected by direction with native OAM bias");
    }

    private static void VerifyMapArrowShapeCases(ISnesAddressSpace rom)
    {
        for (int index = 0; index < 4; index++)
        {
            int animation = ReadVerificationWord(rom, 0x81af36 + 10 * index);
            int variants = ReadVerificationWord(rom, 0x82c1e4 + 2 * (animation - 1));
            AssertEqual(ReadVerificationWord(rom, 0x820000 | variants),
                MapArrowDefinitions.SpriteBase((MapScrollDirection)(index + 1)), "original direction shape selector");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 0, 5, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MapArrowDefinitions.SpriteBase((MapScrollDirection)invalid),
                "unsupported arrow shape direction");
    }
}
