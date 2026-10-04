using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPauseSelectorAnchors(ISnesAddressSpace rom)
    {
        // Original names from the preconversion definition table, kept only as the oracle.
        string[][] names = [ ["Reserve.Mode", "Reserve.Transfer"],
            ["Beam.Charge", "Beam.Ice", "Beam.Wave", "Beam.Spazer", "Beam.Plasma"],
            ["Equipment.Varia", "Equipment.Gravity", "Equipment.MorphBall", "Equipment.Bombs", "Equipment.SpringBall", "Equipment.ScrewAttack"],
            ["Boots.HiJump", "Boots.SpaceJump", "Boots.SpeedBooster"] ];
        var expectedNames = names.SelectMany((group, category) => group.Select((name, item) => (category, item, name))).ToArray();
        AssertTrue(expectedNames.SequenceEqual(PauseSelectorDefinitions.Anchors()), "original named identity order");
        foreach (var (category, item, name) in expectedNames)
            AssertEqual(name, PauseSelectorDefinitions.Anchor(category, item), "original named identity case");
        byte[] bytes = PauseSelectorExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<PauseSelectorDocument>(bytes, MapPresentationFormat.JsonOptions)!;
        var stock = PauseSelectorPresentation.Load(new MemoryStream(bytes));
        AssertEqual(0, stock.StoredAnchorComponentCount, "stock selector stores no coordinate table");
        VerifyPauseSelectorAnchorField(rom, document, stock, horizontal: true);
        VerifyPauseSelectorAnchorField(rom, document, stock, horizontal: false);
        foreach (int category in new[] { int.MinValue, -1, 4, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Anchor(category, 0), "unsupported selector category");
        for (int category = 0; category < names.Length; category++)
        foreach (int item in new[] { int.MinValue, -1, names[category].Length, 256, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Anchor(category, item), "unsupported selector item");
            AssertThrows<ArgumentOutOfRangeException>(() => PauseSelectorDefinitions.StockAnchor(category, item), "geometry guards item before arithmetic");
        }
    }

    private static void VerifyPauseSelectorAnchorField(ISnesAddressSpace rom, PauseSelectorDocument document,
        PauseSelectorPresentation stock, bool horizontal)
    {
        foreach (var anchor in PauseSelectorDefinitions.Anchors())
        {
            int pointer = 0x820000 | ReadVerificationWord(rom, 0x82c18e + anchor.Category * 2);
            int original = ReadVerificationWord(rom, pointer + anchor.Item * 4 + (horizontal ? 0 : 2)) - 1;
            var basis = PauseSelectorDefinitions.StockAnchor(anchor.Category, anchor.Item);
            var point = stock.Anchor(anchor.Category, anchor.Item);
            AssertEqual(original, horizontal ? basis.X : basis.Y, "native selector geometry field");
            AssertEqual(original, horizontal ? point.X : point.Y, "native installed selector coordinate field");
            foreach (int value in new[] { 0, horizontal ? 255 : 223 })
            {
                var anchors = new Dictionary<string, MapLabelPoint>(document.Anchors);
                var previous = anchors[anchor.Name];
                anchors[anchor.Name] = horizontal ? new(value, previous.Y) : new(previous.X, value);
                using var stream = new MemoryStream(); PauseSelectorPresentation.Write(stream, document with { Anchors = anchors }); stream.Position = 0;
                var edited = PauseSelectorPresentation.Load(stream);
                AssertEqual(1, edited.StoredAnchorComponentCount, "only one independently authored coordinate captured");
                foreach (var other in PauseSelectorDefinitions.Anchors())
                    AssertEqual(anchors[other.Name], edited.Anchor(other.Category, other.Item), "all edited and unedited coordinates");
            }
        }
        foreach (int invalid in new[] { -1, horizontal ? 256 : 224 })
        {
            var anchors = new Dictionary<string, MapLabelPoint>(document.Anchors);
            var first = anchors["Reserve.Mode"];
            anchors["Reserve.Mode"] = horizontal ? new(invalid, first.Y) : new(first.X, invalid);
            AssertThrows<InvalidDataException>(() => PauseSelectorPresentation.Write(new MemoryStream(), document with { Anchors = anchors }),
                "invalid authored coordinate");
        }
    }
}
