using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyDynamicCollectibleStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmDynamicCollectibleArtCatalog installed)
    {
        var original = new List<RoomPlmDynamicCollectibleGraphic>();
        for (int raw = 4; raw <= 20; raw++)
        {
            ushort program = ReadCollectibleGraphicsWord(rom, 0xeed7 + raw * 4 + 2);
            ushort pointer = ReadCollectibleGraphicsWord(rom, program + 2);
            byte[] tiles = new byte[256], palettes = new byte[8];
            for (int i = 0; i < tiles.Length; i++) tiles[i] = rom.ReadByte(0x890000 | (pointer + i));
            for (int i = 0; i < palettes.Length; i++) palettes[i] = rom.ReadByte(0x840000 | (program + 4 + i));
            original.Add(new((InWorldCollectibleKind)raw, pointer, palettes, tiles));
        }
        string Hash(IEnumerable<RoomPlmDynamicCollectibleGraphic> graphics) =>
            SelectedPresentationHash.Create(nameof(RoomPlmDynamicCollectibleArtCatalog), content =>
            {
                foreach (var graphic in graphics)
                {
                    content.Append("kind", (int)graphic.Kind);
                    content.Append("characters", graphic.Tiles.Span);
                    content.Append("palette selectors", graphic.PaletteOffsets.Span);
                }
            });
        void Compare(RoomPlmDynamicCollectibleArtCatalog catalog, IEnumerable<RoomPlmDynamicCollectibleGraphic> expected)
        {
            AssertEqual(Hash(expected), catalog.ContentIdentity, "Dynamic artwork original content framing");
            foreach (var value in expected)
            {
                var actual = catalog.Resolve(value.Kind);
                AssertEqual(value.Kind, actual.Kind, "Dynamic artwork kind identity");
                AssertEqual(value.GraphicsPointer, actual.GraphicsPointer, "Dynamic artwork source identity");
                AssertTrue(value.Tiles.Span.SequenceEqual(actual.Tiles.Span), "Dynamic artwork exact native/custom characters");
                AssertTrue(value.PaletteOffsets.Span.SequenceEqual(actual.PaletteOffsets.Span), "Dynamic artwork exact native/custom palettes");
            }
        }
        var entries = original.Select(value => new RoomPlmDynamicCollectibleArtEntry(
            value.Kind, value.Tiles.ToArray(), value.PaletteOffsets.ToArray())).ToArray();
        var stock = RoomPlmDynamicCollectibleArtCatalog.Stock();
        foreach (var catalog in new[] {stock, installed, new RoomPlmDynamicCollectibleArtCatalog(entries.Reverse())})
            Compare(catalog, original);
        for (int raw = 0; raw <= byte.MaxValue; raw++)
            if (raw is < 4 or > 20)
                AssertThrows<InvalidDataException>(() => stock.Resolve((InWorldCollectibleKind)raw), "Dynamic stock rejected byte kind");
        entries[0].Tiles[0] ^= 1;
        entries[1].PaletteOffsets[0] = 7;
        var mixed = new RoomPlmDynamicCollectibleArtCatalog(entries);
        var expected = original.Select((value,index) => new RoomPlmDynamicCollectibleGraphic(
            value.Kind,value.GraphicsPointer,entries[index].PaletteOffsets.ToArray(),entries[index].Tiles.ToArray())).ToArray();
        entries[0].Tiles[0] ^= 2;
        entries[1].PaletteOffsets[0] = 6;
        entries[2].Tiles[0] ^= 4;
        entries[2].PaletteOffsets[0] = 5;
        Compare(mixed, expected);
        AssertThrows<ArgumentNullException>(() => new RoomPlmDynamicCollectibleArtCatalog(null!), "Dynamic null import");
        AssertThrows<InvalidDataException>(() => new RoomPlmDynamicCollectibleArtCatalog(entries[..1]), "Dynamic missing item");
        AssertThrows<InvalidDataException>(() => new RoomPlmDynamicCollectibleArtCatalog(entries.Append(entries[0])), "Dynamic duplicate item");
        foreach (RoomPlmDynamicCollectibleArtEntry bad in new RoomPlmDynamicCollectibleArtEntry[]
        {
            null!, new((InWorldCollectibleKind)255,new byte[256],new byte[8]),
            new(InWorldCollectibleKind.Bombs,null!,new byte[8]),
            new(InWorldCollectibleKind.Bombs,new byte[255],new byte[8]),
            new(InWorldCollectibleKind.Bombs,new byte[256],null!),
            new(InWorldCollectibleKind.Bombs,new byte[256],new byte[7]),
            new(InWorldCollectibleKind.Bombs,new byte[256],[8,0,0,0,0,0,0,0]),
        })
        {
            var invalid = entries.ToArray();
            invalid[0] = bad;
            AssertThrows<InvalidDataException>(() => new RoomPlmDynamicCollectibleArtCatalog(invalid), "Dynamic invalid item artwork");
        }
    }
}