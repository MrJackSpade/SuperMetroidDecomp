using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyChargeFlareCompositions(SuperMetroidAddressSpace bus)
    {
        byte[] json = ChargeFlareSpriteExtractor.Extract(bus);
        var stock = ChargeFlareSpriteCatalog.Load(new MemoryStream(json));
        int cases = 0;
        for (ushort selector = 0; selector < ChargeFlareSpriteDefinitions.Selectors.Length; selector++)
        {
            AssertEqual(RomDataReader.ReadWordFixedBank(bus, ChargeFlareSpriteDefinitions.SelectorTable + selector * 2),
                ChargeFlareSpriteDefinitions.Selectors[selector], "Compiled charge-flare selector matches pinned cartridge");
            foreach (ushort x in new ushort[] { 0, 1, 127, 255, 256, 511, 65535 })
            foreach (ushort y in new ushort[] { 0, 1, 127, 255, 256, 511, 65535 })
            foreach (int occupied in new[] { 0, 126, 127 })
            {
                var native = new OamBuffer(); var actual = new OamBuffer();
                for (int i = 0; i < occupied; i++)
                {
                    native.AddProjectileSpritePart(default, 0, default, 10, 20);
                    actual.AddProjectileSpritePart(default, 0, default, 10, 20);
                }
                native.AddFlareSpritemap(bus, selector, x, y);
                stock.Draw(selector, actual, x, y);
                AssertTrue(native.LowTable.SequenceEqual(actual.LowTable) && native.HighTable.SequenceEqual(actual.HighTable),
                    "Extracted flare matches complete native OAM, including signed placement, high bits and capacity wrap");
                AssertEqual(native.NextByteOffset, actual.NextByteOffset, "Flare composition retains native OAM cursor");
                cases++;
            }
        }
        var document = JsonNode.Parse(json)!;
        string first = ProjectileSpriteDefinitions.Name(ChargeFlareSpriteDefinitions.Selectors[0]);
        int oldX = document["frames"]![first]![0]!["offsetX"]!.GetValue<int>();
        document["frames"]![first]![0]!["offsetX"] = oldX + 7;
        var edited = Load(document);
        var before = new OamBuffer(); var after = new OamBuffer();
        stock.Draw(0, before, 100, 100); edited.Draw(0, after, 100, 100);
        AssertEqual((before.GetEntry(0).X + 7) & 511, after.GetEntry(0).X, "Composition edit changes emitted flare position");
        AssertEqual(before.GetEntry(0).Y, after.GetEntry(0).Y, "X-only composition edit preserves Y");
        document["frames"]!.AsObject().Remove(first);
        AssertThrows<InvalidDataException>(() => Load(document), "Missing charge-flare composition rejected");
        document = JsonNode.Parse(json)!; document["damage"] = 1;
        AssertThrows<InvalidDataException>(() => Load(document), "Charge-flare composition rejects mechanics fields");
        document = JsonNode.Parse(json)!; document["frames"]![first]![0]!["palette"] = 8;
        AssertThrows<InvalidDataException>(() => Load(document), "Invalid flare palette rejected");
        AssertThrows<InvalidDataException>(() => ChargeFlareSpriteCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))), "Duplicate flare JSON rejected");
        AssertThrows<InvalidDataException>(() => stock.Draw(54, new OamBuffer(), 0, 0), "Non-charge selector is not silently substituted");
        AssertThrows<InvalidDataException>(() => stock.Draw(ushort.MaxValue, new OamBuffer(), 0, 0), "Invalid selector is not clamped");
        Console.WriteLine($"Charge-flare compositions: {cases} native OAM comparisons, editable displacement and invalid-resource checks pass.");
        static ChargeFlareSpriteCatalog Load(JsonNode node) => ChargeFlareSpriteCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(node.ToJsonString())));
    }
}
