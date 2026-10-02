using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyDraygonCannonStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmDraygonCannonVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0x9fcd,"right-shield-a"),(0x9fdd,"right-shield-b"),(0xa02d,"right-damaged-a"),(0xa03d,"right-damaged-b"),(0xa04d,"right-damaged-c"),(0xa05d,"right-damaged-d"),(0xa0ed,"left-shield-a"),(0xa101,"left-shield-b"),(0xa165,"left-damaged-a"),(0xa179,"left-damaged-b"),(0xa18d,"left-damaged-c"),(0xa1a1,"left-damaged-d")];
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
        var entries = frames.Select(frame => new RoomPlmDraygonCannonVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = RoomPlmDraygonCannonVisualCatalog.Stock();
        var imported = new RoomPlmDraygonCannonVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmDraygonCannonVisualCatalog), words);
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Cannon original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetWord(frame.Pointer,run,word), "Cannon original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "Cannon stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "Cannon stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "Cannon stock unknown pointer");
        entries[8].Blocks[2] = 0xc58;
        var mixed = new RoomPlmDraygonCannonVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Cannon mixed custom/stock identity");
        entries[8].Blocks[2] = 0x5a;
        entries[10].Blocks[3] = 0x5b;
        foreach (var frame in frames)
        {
            int index = 0;
            for (int run = 0; run < native[frame.Pointer].Length; run++)
            for (int word = 0; word < native[frame.Pointer][run].Length; word++)
                AssertEqual(expected[frame.Pointer][index++], mixed.GetWord(frame.Pointer,run,word), "Cannon cloned custom and unchanged stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmDraygonCannonVisualCatalog(entries[..1]), "Cannon missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmDraygonCannonVisualCatalog(entries.Append(entries[0])), "Cannon duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("RIGHT-SHIELD-A", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmDraygonCannonVisualCatalog(invalid), "Cannon ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[7]);
        AssertThrows<InvalidDataException>(() => new RoomPlmDraygonCannonVisualCatalog(invalid), "Cannon exact flattened shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmDraygonCannonVisualCatalog(entries), "Cannon visual bits only");
    }
}
