using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks pause-selector artwork edits and authored phase changes while preserving native OAM and timing.</summary>
    /// <param name="rom">Cartridge address space used to extract selector presentation and native sprite compositions.</param>
    private static void VerifyPauseSelectorCompositions(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseSelectorExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<PauseSelectorDocument>(bytes, MapPresentationFormat.JsonOptions)!;
        var stock = PauseSelectorPresentation.Load(new MemoryStream(bytes));
        for (int group = 0; group < 3; group++) VerifyPauseSelectorNativeCompositions(rom, stock, group);
        AssertEqual(0, stock.StoredCompositionOverrideCount, "stock selector has no phase composition table");
        AssertTrue(!stock.StoresCompositionParts, "stock selector stores no sprite part records");
        foreach (string name in new[] { "Reserve", "Beam", "Equipment" })
        {
            int category = name == "Reserve" ? 0 : name == "Beam" ? 1 : 2;
            var original = document.Frames[name];
            for (int index = 0; index < original.Length; index++)
            {
                var part = original[index];
                foreach (var change in new[] { part with { OffsetX = 17 }, part with { OffsetY = 13 },
                    part with { TileColumn = 0 }, part with { TileRow = 0 }, part with { Size = 16 },
                    part with { Priority = 0 }, part with { Palette = 7 }, part with { FlipX = true }, part with { FlipY = true } })
                {
                    var parts = (SpriteVisualPart[])original.Clone(); parts[index] = change;
                    var frames = new Dictionary<string, SpriteVisualPart[]>(document.Frames); frames[name] = parts;
                    var edited = PauseSelectorPresentation.Load(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
                        document with { Frames = frames }, MapPresentationFormat.JsonOptions)));
                    AssertTrue(edited.StoresCompositionParts, "independent selector part edit retained");
                    var expected = new OamBuffer(); var actual = new OamBuffer(); expected.BeginFrame(); actual.BeginFrame();
                    var anchor = edited.Anchor(category, 0);
                    MenuSpriteCompiler.Compile(parts, name).DrawOnScreen(expected, (ushort)anchor.X, (ushort)anchor.Y, edited.PaletteBits);
                    edited.Draw(actual, category, 0, 0);
                    expected.FinalizeFrame(); actual.FinalizeFrame();
                    AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable),
                        "all selector part fields remain independently editable");
                }
            }
        }
        for (int group = 0; group < 3; group++)
        for (int changed = 0; changed < document.Animation.Length; changed++)
        {
            var phases = (PauseSelectorPhase[])document.Animation.Clone();
            phases[changed] = group switch
            {
                0 => phases[changed] with { Reserve = "Beam" },
                1 => phases[changed] with { Beam = "Equipment" },
                _ => phases[changed] with { Equipment = "Reserve" },
            };
            using var stream = new MemoryStream(); PauseSelectorPresentation.Write(stream, document with { Animation = phases }); stream.Position = 0;
            var edited = PauseSelectorPresentation.Load(stream);
            AssertEqual(changed == 0 ? phases.Length - 1 : 1, edited.StoredCompositionOverrideCount,
                "only differences from the first authored phase are captured");
            for (int category = 0; category < 4; category++)
            foreach (int phase in Enumerable.Range(0, phases.Length).Append(phases.Length).Append(int.MaxValue))
            {
                var source = phases[phase % phases.Length];
                string name = category switch { 0 => source.Reserve, 1 => source.Beam, _ => source.Equipment };
                var composition = MenuSpriteCompiler.Compile(document.Frames[name], name);
                var anchor = edited.Anchor(category, 0);
                var expected = new OamBuffer(); var actual = new OamBuffer(); expected.BeginFrame(); actual.BeginFrame();
                composition.DrawOnScreen(expected, (ushort)anchor.X, (ushort)anchor.Y, edited.PaletteBits);
                edited.Draw(actual, category, 0, phase);
                expected.FinalizeFrame(); actual.FinalizeFrame();
                AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable),
                    "independent authored group/phase and cyclic OAM");
                AssertEqual(source.DurationTicks, edited.Duration(phase), "composition edit leaves phase timing unchanged");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Draw(new OamBuffer(), 0, 0, -1), "negative composition phase");
    }

    /// <summary>Compares one selector category's installed draws with native spritemaps across phases and OAM occupancy.</summary>
    /// <param name="rom">Cartridge address space supplying the native selector program, anchors, and spritemaps.</param>
    /// <param name="selector">Loaded presentation whose draw output is compared with cartridge rendering.</param>
    /// <param name="group">Category group whose selector anchors and compositions are verified.</param>
    private static void VerifyPauseSelectorNativeCompositions(ISnesAddressSpace rom, PauseSelectorPresentation selector, int group)
    {
        int program = 0x820000 | ReadVerificationWord(rom, 0x82c0ec);
        int bases = 0x820000 | ReadVerificationWord(rom, 0x82c1e8);
        foreach (var anchor in PauseSelectorDefinitions.Anchors().Where(a => (a.Category < 2 ? a.Category : 2) == group))
        {
            int positions = 0x820000 | ReadVerificationWord(rom, 0x82c18e + anchor.Category * 2);
            ushort x = (ushort)(ReadVerificationWord(rom, positions + anchor.Item * 4) - 1);
            ushort y = (ushort)(ReadVerificationWord(rom, positions + anchor.Item * 4 + 2) - 1);
            for (int phase = 0; phase < 14; phase++)
            foreach (int occupied in new[] { 0, 127, 128 })
            {
                int sprite = ReadVerificationWord(rom, bases + anchor.Category * 2) + rom.ReadByte(program + phase * 3 + 2);
                int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c569 + sprite * 2);
                var expected = new OamBuffer(); var actual = new OamBuffer(); expected.BeginFrame(); actual.BeginFrame();
                for (int i = 0; i < occupied; i++) { expected.AddRawSmallSprite(12, 34, 56); actual.AddRawSmallSprite(12, 34, 56); }
                DrawImportedSpritemap(rom, expected, pointer, x, y, 0x600);
                selector.Draw(actual, anchor.Category, anchor.Item, phase);
                expected.FinalizeFrame(); actual.FinalizeFrame();
                AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable),
                    "native selector composition, attributes, order and OAM capacity");
            }
        }
    }
}
