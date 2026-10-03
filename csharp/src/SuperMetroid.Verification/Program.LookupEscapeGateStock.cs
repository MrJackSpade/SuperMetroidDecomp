using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyEscapeGateStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmEscapeGateVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0x9473,"open"),(0x947f,"half-closed"),(0x948b,"closed")];
        var native = new Dictionary<ushort, ushort[]>();
        foreach (var frame in frames)
        {
            var words = new ushort[4];
            for (int row = 0; row < words.Length; row++)
                words[row] = (ushort)(ReadSamusEaterPlmWord(rom, 0x840000 | (frame.Pointer + 2 + row * 2)) & 0xfff);
            native.Add(frame.Pointer, words);
        }
        var entries = frames.Select(frame => new RoomPlmEscapeGateVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmEscapeGateVisualCatalog.Stock();
        var imported = new RoomPlmEscapeGateVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmEscapeGateVisualCatalog), words);
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Escape gate original stock identity");
            foreach (var frame in frames)
            {
                for (int row = 0; row < 4; row++)
                    AssertEqual(native[frame.Pointer][row], catalog.GetWord(frame.Pointer,row), "Escape gate native stock visual");
                foreach (int bad in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad), "Escape gate stock row bounds");
            }
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (!native.ContainsKey((ushort)raw))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)raw,0), "Escape gate unknown stock pointer");
        entries[2].Blocks[1] = 0xc58;
        entries[1].Blocks[3] = 0x59;
        var mixed = new RoomPlmEscapeGateVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer,
            frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Escape gate custom/stock hash");
        entries[2].Blocks[1] = 0x5a;
        entries[0].Blocks[0] = 0x5b;
        foreach (var frame in frames)
        for (int row = 0; row < 4; row++)
            AssertEqual(expected[frame.Pointer][row], mixed.GetWord(frame.Pointer,row), "Escape gate custom clone and stock isolation");
        AssertThrows<InvalidDataException>(() => new RoomPlmEscapeGateVisualCatalog(entries[..2]), "Escape gate missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmEscapeGateVisualCatalog(entries.Append(entries[0])), "Escape gate duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("OPEN", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmEscapeGateVisualCatalog(invalid), "Escape gate ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[3]);
        AssertThrows<InvalidDataException>(() => new RoomPlmEscapeGateVisualCatalog(invalid), "Escape gate exact shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmEscapeGateVisualCatalog(entries), "Escape gate visual bits only");
    }
}
