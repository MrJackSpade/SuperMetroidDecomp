using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyProjectileCompositions(SuperMetroidAddressSpace rom)
    {
        byte[] json = ProjectileSpriteExtractor.Extract(rom);
        var content = ProjectileSpriteCatalog.Load(new MemoryStream(json));
        int draws = 0;
        foreach (ushort id in ProjectileSpriteDefinitions.NativePointers)
        foreach (ushort origin in new ushort[] { 0, 1, 127, 255, 256, 0x7fff, 0xffff })
        foreach (int preceding in new[] { 0, 127 })
        {
            var native = new OamBuffer(); var authored = new OamBuffer();
            for (int i = 0; i < preceding; i++)
            {
                native.AddProjectileSpritePart(new(0), 0, new(0), 0, 0);
                authored.AddProjectileSpritePart(new(0), 0, new(0), 0, 0);
            }
            native.AddProjectileSpritemap(rom, id, origin, origin);
            content.Draw(id, authored, origin, origin);
            AssertTrue(native.LowTable.SequenceEqual(authored.LowTable), "Extracted composition retains all low OAM bytes");
            AssertTrue(native.HighTable.SequenceEqual(authored.HighTable), "Extracted composition retains all high OAM bits");
            AssertEqual(native.NextByteOffset, authored.NextByteOffset, "Extracted composition retains native capacity wrapping");
            draws++;
        }
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<ProjectileSpriteDocument>(json, options)!;
        ushort editedId = ProjectileSpriteDefinitions.NativePointers.ToArray().First(id => document.Frames[ProjectileSpriteDefinitions.Name(id)].Length > 0);
        string name = ProjectileSpriteDefinitions.Name(editedId);
        var original = document.Frames[name][0];
        document.Frames[name][0] = original with { OffsetX = original.OffsetX == 255 ? 254 : original.OffsetX + 1 };
        byte[] editedJson = JsonSerializer.SerializeToUtf8Bytes(document, options);
        var edited = ProjectileSpriteCatalog.Load(new MemoryStream(editedJson));
        var baselineOam = new OamBuffer(); var editedOam = new OamBuffer();
        content.Draw(editedId, baselineOam, 100, 100); edited.Draw(editedId, editedOam, 100, 100);
        AssertEqual(unchecked((byte)(baselineOam.LowTable[0] + (original.OffsetX == 255 ? -1 : 1))), editedOam.LowTable[0], "Edited offset reaches emitted OAM");
        AssertTrue(!baselineOam.LowTable.SequenceEqual(editedOam.LowTable), "Visual edit is observable");
        document.Frames[name][0] = original;
        editedOam = new OamBuffer(); edited.Draw(editedId, editedOam, 100, 100);
        AssertTrue(!baselineOam.LowTable.SequenceEqual(editedOam.LowTable), "Loaded composition owns immutable compiled parts");

        void Reject(ProjectileSpriteDocument bad) => AssertThrows<InvalidDataException>(() => ProjectileSpriteCatalog.Load(new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(bad, options))), "Malformed composition rejected");
        Reject(document with { Version = 2 });
        foreach (var badPart in new[] { original with { Palette = null }, original with { TileColumn = 16 }, original with { TileRow = 32 }, original with { OffsetX = 256 }, original with { OffsetY = -129 }, original with { Size = 32 } })
        {
            document.Frames[name][0] = badPart; Reject(document);
        }
        document.Frames.Remove(name); Reject(document);
        foreach (string field in new[] { "\"version\":1,", "\"damage\":10," })
        {
            byte[] malformed = System.Text.Encoding.UTF8.GetBytes(System.Text.Encoding.UTF8.GetString(json).Insert(1, field));
            AssertThrows<InvalidDataException>(() => ProjectileSpriteCatalog.Load(new MemoryStream(malformed)), "Duplicate or unknown mechanics fields rejected");
        }
        AssertThrows<InvalidDataException>(() => content.Draw(0, new OamBuffer(), 0, 0), "Unknown sprite is loud");
        Console.WriteLine($"Projectile compositions: 417 extracted sprites/{draws} native OAM comparisons, observable edits, immutable load and invalid-field rejection pass without a draw-time ROM.");
    }
}
