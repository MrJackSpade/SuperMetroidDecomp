using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCollectibleStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmCollectibleVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0xa2b5,"empty"),(0xa2c7,"chozo-orb-0"),(0xa2cd,"chozo-orb-1"),(0xa2d3,"chozo-orb-2"),(0xa2d9,"chozo-orb-burst"),(0xa2df,"energy-tank-0"),(0xa2e5,"energy-tank-1"),(0xa2eb,"missile-tank-0"),(0xa2f1,"missile-tank-1"),(0xa2f7,"super-missile-tank-0"),(0xa2fd,"super-missile-tank-1"),(0xa303,"power-bomb-tank-0"),(0xa309,"power-bomb-tank-1"),(0xa30f,"dynamic-slot-0-frame-0"),(0xa315,"dynamic-slot-0-frame-1"),(0xa31b,"dynamic-slot-1-frame-0"),(0xa321,"dynamic-slot-1-frame-1"),(0xa327,"dynamic-slot-2-frame-0"),(0xa32d,"dynamic-slot-2-frame-1"),(0xa333,"dynamic-slot-3-frame-0"),(0xa339,"dynamic-slot-3-frame-1"),(0xa3dd,"shot-reveal-0"),(0xa3e3,"shot-reveal-1"),(0xa3e9,"shot-reveal-2")];
        var native = frames.ToDictionary(frame => frame.Pointer,
            frame => (ushort)(ReadCollectibleVisualWord(rom, (ushort)(frame.Pointer + 2)) & 0xfff));
        var entries = frames.Select(frame => new RoomPlmCollectibleVisualEntry(frame.Id, native[frame.Pointer])).ToArray();
        var stock = RoomPlmCollectibleVisualCatalog.Stock();
        var imported = new RoomPlmCollectibleVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmCollectibleVisualCatalog), words);
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Collectible original single-word framed identity");
            foreach (var frame in frames)
                AssertEqual(native[frame.Pointer], catalog.GetWord(frame.Pointer), "Collectible original stock word");
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer), "Collectible stock unknown pointer");
        entries[5] = entries[5] with { VisualWord = 0xc58 };
        var mixed = new RoomPlmCollectibleVisualCatalog(entries);
        var expected = frames.Select((frame, index) => (frame.Pointer, entries[index].VisualWord))
            .ToDictionary(pair => pair.Pointer, pair => pair.VisualWord);
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Collectible mixed custom/stock identity");
        entries[5] = entries[5] with { VisualWord = 0x5a };
        entries[6] = entries[6] with { VisualWord = 0x5b };
        foreach (var frame in frames)
            AssertEqual(expected[frame.Pointer], mixed.GetWord(frame.Pointer), "Collectible changed and unchanged input isolation");
        AssertThrows<ArgumentNullException>(() => new RoomPlmCollectibleVisualCatalog(null!), "Collectible null import");
        AssertThrows<InvalidDataException>(() => new RoomPlmCollectibleVisualCatalog(entries[..1]), "Collectible missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmCollectibleVisualCatalog(entries.Append(entries[0])), "Collectible duplicate frame");
        foreach (RoomPlmCollectibleVisualEntry bad in new RoomPlmCollectibleVisualEntry[]
            { null!, new("EMPTY", 0xff), new("unknown", 0xff), new("empty", 0xf0ff) })
        {
            var invalid = entries.ToArray();
            invalid[0] = bad;
            AssertThrows<InvalidDataException>(() => new RoomPlmCollectibleVisualCatalog(invalid), "Collectible invalid import");
        }
    }
}