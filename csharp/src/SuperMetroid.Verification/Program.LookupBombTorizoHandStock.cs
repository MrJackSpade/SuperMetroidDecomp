using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBombTorizoHandStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmBombTorizoHandVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0x9877,"intact"),(0x989d,"cleared")];
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
        var entries = frames.Select(frame => new RoomPlmBombTorizoHandVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = RoomPlmBombTorizoHandVisualCatalog.Stock();
        var imported = new RoomPlmBombTorizoHandVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmBombTorizoHandVisualCatalog), words);
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Hand original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetWord(frame.Pointer,run,word), "Hand original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "Hand stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "Hand stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "Hand stock unknown pointer");
        entries[0].Blocks[7] = 0xc58;
        var mixed = new RoomPlmBombTorizoHandVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Hand mixed custom/stock identity");
        entries[0].Blocks[7] = 0x5a;
        entries[1].Blocks[15] = 0x5b;
        foreach (var frame in frames)
        {
            int index = 0;
            for (int run = 0; run < native[frame.Pointer].Length; run++)
            for (int word = 0; word < native[frame.Pointer][run].Length; word++)
                AssertEqual(expected[frame.Pointer][index++], mixed.GetWord(frame.Pointer,run,word), "Hand cloned custom and unchanged stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmBombTorizoHandVisualCatalog(entries[..1]), "Hand missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmBombTorizoHandVisualCatalog(entries.Append(entries[0])), "Hand duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("INTACT", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmBombTorizoHandVisualCatalog(invalid), "Hand ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[7]);
        AssertThrows<InvalidDataException>(() => new RoomPlmBombTorizoHandVisualCatalog(invalid), "Hand exact flattened shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmBombTorizoHandVisualCatalog(entries), "Hand visual bits only");
    }
}
