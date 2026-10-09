using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks stock pause-selector anchor identities, native coordinates, override round-trips, and label identity admission.</summary>
    /// <param name="rom">Address space used by the native selector and label extractors and coordinate oracle.</param>
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
        AssertTrue(expectedNames.Where(entry => entry.category != 0).SequenceEqual(PauseEquipmentLabelDefinitions.Labels()),
            "label identity view contains exactly the fourteen ordinary controls in order");
        AssertEqual(0, PauseEquipmentLabelDefinitions.ItemCount(0), "reserve controls have no inventory labels");
        for (int category = 1; category < names.Length; category++)
        {
            AssertEqual(names[category].Length, PauseEquipmentLabelDefinitions.ItemCount(category), "label category capacity");
            for (int item = 0; item < names[category].Length; item++)
                AssertEqual(names[category][item], PauseEquipmentLabelDefinitions.Key(category, item), "label alias identity");
        }
        foreach (int category in new[] { int.MinValue, -1, 0, 4, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => PauseEquipmentLabelDefinitions.Key(category, 0), "no label for invalid or reserve category");
        for (int category = 1; category < names.Length; category++)
        foreach (int item in new[] { int.MinValue, -1, names[category].Length, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => PauseEquipmentLabelDefinitions.Key(category, item), "label category bounds");
        Suite(nameof(VerifyPauseLabelIdentityAdmission), () => VerifyPauseLabelIdentityAdmission(rom, expectedNames.Where(entry => entry.category != 0).Select(entry => entry.name)
            .Append("Beam.Hyper").ToArray()));
        byte[] bytes = PauseSelectorExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<PauseSelectorDocument>(bytes, MapPresentationFormat.JsonOptions)!;
        var stock = PauseSelectorPresentation.Load(new MemoryStream(bytes));
        AssertEqual(0, stock.StoredAnchorComponentCount, "stock selector stores no coordinate table");
        Suite(nameof(VerifyPauseSelectorAnchorField), () => VerifyPauseSelectorAnchorField(rom, document, stock, horizontal: true));
        Suite(nameof(VerifyPauseSelectorAnchorField), () => VerifyPauseSelectorAnchorField(rom, document, stock, horizontal: false));
        foreach (int category in new[] { int.MinValue, -1, 4, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Anchor(category, 0), "unsupported selector category");
        for (int category = 0; category < names.Length; category++)
        foreach (int item in new[] { int.MinValue, -1, names[category].Length, 256, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Anchor(category, item), "unsupported selector item");
            AssertThrows<ArgumentOutOfRangeException>(() => PauseSelectorDefinitions.StockAnchor(category, item), "geometry guards item before arithmetic");
        }
    }

    /// <summary>Verifies label serialization accepts exactly the extracted identities and rejects missing, substituted, or extra keys.</summary>
    /// <param name="rom">Address space used to extract the native pause equipment labels.</param>
    /// <param name="names">Expected case-sensitive label keys derived from the selector identity catalog.</param>
    private static void VerifyPauseLabelIdentityAdmission(ISnesAddressSpace rom, string[] names)
    {
        byte[] source = PauseEquipmentLabelExtractor.Extract(rom);
        var document = JsonSerializer.Deserialize<PauseEquipmentLabelDocument>(source, MapPresentationFormat.JsonOptions)!;
        AssertTrue(document.Labels.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(names), "native extractor supplies exact label identities");
        _ = PauseEquipmentLabelPresentation.Load(new MemoryStream(source));
        foreach (string name in names)
        {
            var labels = new Dictionary<string, PauseEquipmentLabel>(document.Labels);
            PauseEquipmentLabel original = labels[name];
            labels.Remove(name);
            Reject(labels);
            labels.Add("Absent.Key", original);
            Reject(labels);
            labels.Remove("Absent.Key");
            labels.Add(name.ToLowerInvariant(), original);
            Reject(labels);
        }
        var extra = new Dictionary<string, PauseEquipmentLabel>(document.Labels) { ["Absent.Key"] = document.Labels[names[0]] };
        Reject(extra);
        void Reject(Dictionary<string, PauseEquipmentLabel> labels) =>
            AssertThrows<InvalidDataException>(() => PauseEquipmentLabelPresentation.Write(new MemoryStream(), document with { Labels = labels }),
                "label loader rejects missing, substituted, case-changed or extra identities");
    }

    /// <summary>Checks one coordinate axis against native anchor geometry and verifies edited values survive serialization within bounds.</summary>
    /// <param name="rom">Address space containing the native selector geometry tables.</param>
    /// <param name="document">Extracted selector document whose authored anchors are varied for the round-trip checks.</param>
    /// <param name="stock">Loaded stock selector used to compare default coordinates and edited results.</param>
    /// <param name="horizontal"><see langword="true"/> checks X coordinates; <see langword="false"/> checks Y coordinates.</param>
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
