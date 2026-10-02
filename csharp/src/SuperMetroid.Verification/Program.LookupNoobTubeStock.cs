using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyNoobTubeStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmNoobTubeVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0x98d1,"intact"),(0x98d7,"damaged"),(0x98dd,"opened"),(0x98e3,"cleared"),(0x9953,"broken-late"),(0x9991,"opened-rows"),(0x99e5,"broken-full")];
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
        var entries = frames.Select(frame => new RoomPlmNoobTubeVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = RoomPlmNoobTubeVisualCatalog.Stock();
        var imported = new RoomPlmNoobTubeVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmNoobTubeVisualCatalog), words);
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Tube original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetWord(frame.Pointer,run,word), "Tube original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "Tube stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "Tube stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "Tube stock unknown pointer");
        entries[3].Blocks[24] = 0xc58;
        var mixed = new RoomPlmNoobTubeVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Tube mixed custom/stock identity");
        entries[3].Blocks[24] = 0x5a;
        entries[6].Blocks[36] = 0x5b;
        foreach (var frame in frames)
        {
            int index = 0;
            for (int run = 0; run < native[frame.Pointer].Length; run++)
            for (int word = 0; word < native[frame.Pointer][run].Length; word++)
                AssertEqual(expected[frame.Pointer][index++], mixed.GetWord(frame.Pointer,run,word), "Tube cloned custom and unchanged stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmNoobTubeVisualCatalog(entries[..1]), "Tube missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmNoobTubeVisualCatalog(entries.Append(entries[0])), "Tube duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("INTACT", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmNoobTubeVisualCatalog(invalid), "Tube ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[7]);
        AssertThrows<InvalidDataException>(() => new RoomPlmNoobTubeVisualCatalog(invalid), "Tube exact flattened shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmNoobTubeVisualCatalog(entries), "Tube visual bits only");
    }
}
