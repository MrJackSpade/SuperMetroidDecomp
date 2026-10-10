using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Verifies stock and installed downward-gate visual catalogs against the native bank-$84
    /// frames, including run and word lookup bounds and catalog validation.
    /// </summary>
    /// <param name="rom">ROM address space supplying the native downward-gate PLM frame data.</param>
    /// <param name="installed">Catalog loaded from the current installation for comparison with stock data.</param>
    private static void VerifyDownwardGateStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmDownwardGateVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0xa517,"column-frame-0"),(0xa525,"column-frame-1"),
            (0xa533,"column-frame-2"),(0xa541,"column-frame-3"),(0xa54f,"column-frame-4"),(0xa55d,"column-frame-5"),
            (0xa5d7,"blue-left-trigger"),(0xa5e3,"blue-right-trigger"),(0xa5eb,"green-left-trigger"),
            (0xa5f7,"green-right-trigger"),(0xa5ff,"red-left-trigger"),(0xa60b,"red-right-trigger"),
            (0xa613,"yellow-left-trigger"),(0xa61f,"yellow-right-trigger")];
        var native = new Dictionary<ushort, ushort[][]>();
        foreach (var frame in frames)
        {
            var runs = new List<ushort[]>();
            int cursor = frame.Pointer;
            while (true)
            {
                int count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                var words = new ushort[count];
                for (int word = 0; word < count; word++, cursor += 2)
                    words[word] = (ushort)(ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0xfff);
                runs.Add(words);
                byte x = rom.ReadByte(0x840000 | cursor++), y = rom.ReadByte(0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmDownwardGateVisualEntry(frame.Id,
            native[frame.Pointer].Select(run => run.ToArray()).ToArray())).ToArray();
        var stock = RoomPlmDownwardGateVisualCatalog.Stock();
        var imported = new RoomPlmDownwardGateVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[][]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmDownwardGateVisualCatalog), words);
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Gate original run-framed hash");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetWord(frame.Pointer,run,word), "Gate original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "Gate stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "Gate stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "Gate stock unknown pointer");
        entries[6].Runs[1][0] = 0xc58;
        entries[5].Runs[0][4] = 0x59;
        var mixed = new RoomPlmDownwardGateVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer,
            frame => entries.Single(entry => entry.Id == frame.Id).Runs.Select(run => run.ToArray()).ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Gate mixed stock/custom hash");
        entries[6].Runs[1][0] = 0x5a;
        entries[0].Runs[0][0] = 0x5b;
        foreach (var frame in frames)
        for (int run = 0; run < expected[frame.Pointer].Length; run++)
        for (int word = 0; word < expected[frame.Pointer][run].Length; word++)
            AssertEqual(expected[frame.Pointer][run][word], mixed.GetWord(frame.Pointer,run,word), "Gate clone isolation and stock fallback");
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(entries[..13]), "Gate missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(entries.Append(entries[0])), "Gate duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("COLUMN-FRAME-0",entries[0].Runs);
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(invalid), "Gate ordinal identity");
        invalid[0] = new(entries[0].Id,[new ushort[4]]);
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(invalid), "Gate exact word shape");
        invalid[0] = new(entries[0].Id,[new ushort[5],new ushort[1]]);
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(invalid), "Gate exact run shape");
        entries[0].Runs[0][0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmDownwardGateVisualCatalog(entries), "Gate visual bits only");
    }
}
