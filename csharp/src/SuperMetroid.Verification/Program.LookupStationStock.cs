using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Reconstructs native station PLM graphics runs and checks stock, imported, and installed catalogs for identity, indexing, copy isolation, and validation.</summary>
    /// <param name="rom">Cartridge address space containing the native station PLM lists.</param>
    /// <param name="installed">Installed visual catalog compared with the native and generated catalog views.</param>
    private static void VerifyStationStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmStationVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0x9f25,"map-frame-0"),(0x9f6d,"energy-frame-0"),(0x9f91,"missile-frame-0"),
            (0x9f31,"map-frame-1"),(0x9f79,"energy-frame-1"),(0x9f9d,"missile-frame-1"),
            (0x9f3d,"map-frame-2"),(0x9f85,"energy-frame-2"),(0x9fa9,"missile-frame-2"),
            (0x9a3f,"save-idle"),(0x9a9f,"save-active-a"),(0x9a6f,"save-active-b"),
            (0x9f49,"map-right-retracted"),(0x9f55,"map-right-extended"),(0x9f5b,"map-left-retracted"),(0x9f67,"map-left-extended"),
            (0x9fb5,"resource-right-retracted"),(0x9fbb,"resource-right-extended"),(0x9fc1,"resource-left-retracted"),(0x9fc7,"resource-left-extended")];
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
        var entries = frames.Select(frame => new RoomPlmStationVisualEntry(frame.Id,
            native[frame.Pointer].Select(run => run.ToArray()).ToArray())).ToArray();
        var original = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].Select(run => run.ToArray()).ToArray());
        var stock = RoomPlmStationVisualCatalog.Stock();
        var imported = new RoomPlmStationVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[][]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmStationVisualCatalog), words);
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(original), catalog.ContentIdentity, "Station original run-framed identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetWord(frame.Pointer,run,word), "Station original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "Station stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "Station stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "Station stock unknown pointer");
        entries[10].Runs[3][1] = 0xc58;
        var mixed = new RoomPlmStationVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Runs.Select(run => run.ToArray()).ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Station mixed custom/stock identity");
        entries[10].Runs[3][1] = 0x5a;
        entries[11].Runs[5][0] = 0x5b;
        foreach (var frame in frames)
        {
            for (int run = 0; run < native[frame.Pointer].Length; run++)
            for (int word = 0; word < native[frame.Pointer][run].Length; word++)
                AssertEqual(expected[frame.Pointer][run][word], mixed.GetWord(frame.Pointer,run,word), "Station cloned custom and unchanged stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmStationVisualCatalog(entries[..1]), "Station missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmStationVisualCatalog(entries.Append(entries[0])), "Station duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("MAP-FRAME-0", entries[0].Runs);
        AssertThrows<InvalidDataException>(() => new RoomPlmStationVisualCatalog(invalid), "Station ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[][] { new ushort[7] });
        AssertThrows<InvalidDataException>(() => new RoomPlmStationVisualCatalog(invalid), "Station exact run shape");
        entries[0].Runs[0][0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmStationVisualCatalog(entries), "Station visual bits only");
    }
}
