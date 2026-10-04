using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPauseSelectorCompositions(ISnesAddressSpace rom)
    {
        byte[] bytes = PauseSelectorExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<PauseSelectorDocument>(bytes, MapPresentationFormat.JsonOptions)!;
        var stock = PauseSelectorPresentation.Load(new MemoryStream(bytes));
        for (int group = 0; group < 3; group++) VerifyPauseSelectorNativeCompositions(rom, stock, group);
        AssertEqual(0, stock.StoredCompositionOverrideCount, "stock selector has no phase composition table");
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
