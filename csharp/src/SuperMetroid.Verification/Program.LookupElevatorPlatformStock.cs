using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Reconstructs the native elevator-platform tilemap runs and checks stock, imported, and installed catalogs for matching identity, indexing, copy isolation, and validation.</summary>
    /// <param name="rom">Cartridge address space containing the native platform PLM lists.</param>
    /// <param name="installed">Installed visual catalog compared with the native and generated catalog views.</param>
    private static void VerifyElevatorPlatformStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmElevatorPlatformVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0xaa97,"first-frame"),(0xaaaf,"second-frame"),(0xaac7,"third-frame")];
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
        var entries = frames.Select(frame => new RoomPlmElevatorPlatformVisualEntry(frame.Id,
            native[frame.Pointer].Select(run => run.ToArray()).ToArray())).ToArray();
        var original = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].Select(run => run.ToArray()).ToArray());
        var stock = RoomPlmElevatorPlatformVisualCatalog.Stock();
        var imported = new RoomPlmElevatorPlatformVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[][]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmElevatorPlatformVisualCatalog), words);
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(original), catalog.ContentIdentity, "Elevator platform original run-framed identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetWord(frame.Pointer,run,word), "Elevator platform original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "Elevator platform stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "Elevator platform stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "Elevator platform stock unknown pointer");
        entries[1].Runs[2][2] = 0xc58;
        var mixed = new RoomPlmElevatorPlatformVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Runs.Select(run => run.ToArray()).ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Elevator platform mixed custom/stock identity");
        entries[1].Runs[2][2] = 0x5a;
        entries[2].Runs[2][3] = 0x5b;
        foreach (var frame in frames)
        {
            for (int run = 0; run < native[frame.Pointer].Length; run++)
            for (int word = 0; word < native[frame.Pointer][run].Length; word++)
                AssertEqual(expected[frame.Pointer][run][word], mixed.GetWord(frame.Pointer,run,word), "Elevator platform cloned custom and unchanged stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmElevatorPlatformVisualCatalog(entries[..1]), "Elevator platform missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmElevatorPlatformVisualCatalog(entries.Append(entries[0])), "Elevator platform duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("FIRST-FRAME", entries[0].Runs);
        AssertThrows<InvalidDataException>(() => new RoomPlmElevatorPlatformVisualCatalog(invalid), "Elevator platform ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[][] { new ushort[7] });
        AssertThrows<InvalidDataException>(() => new RoomPlmElevatorPlatformVisualCatalog(invalid), "Elevator platform exact run shape");
        entries[0].Runs[0][0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmElevatorPlatformVisualCatalog(entries), "Elevator platform visual bits only");
    }
}
