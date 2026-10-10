using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Verifies Mother Brain glass animation words and content identity against native PLM
    /// runs, then checks lookup bounds, catalog isolation, and rejection of malformed visuals.
    /// </summary>
    /// <param name="rom">Retail address space containing the native glass PLM draw records.</param>
    /// <param name="installed">Glass visual catalog loaded from the game installation.</param>
    private static void VerifyMotherBrainGlassStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmMotherBrainGlassVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0x9717,"initial"),(0x971d,"pane-damage-1"),(0x9731,"pane-damage-2"),(0x9745,"pane-transition"),(0x974f,"shifted-pane-1"),(0x9769,"shifted-pane-2"),(0x9781,"shifted-pane-3"),(0x978f,"shatter-1"),(0x97b7,"shatter-2"),(0x97e7,"shatter-3"),(0x9817,"cleared")];
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
        var entries = frames.Select(frame => new RoomPlmMotherBrainGlassVisualEntry(frame.Id,
            native[frame.Pointer].SelectMany(run => run).ToArray())).ToArray();
        var flat = frames.ToDictionary(frame => frame.Pointer, frame => native[frame.Pointer].SelectMany(run => run).ToArray());
        var stock = RoomPlmMotherBrainGlassVisualCatalog.Stock();
        var imported = new RoomPlmMotherBrainGlassVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmMotherBrainGlassVisualCatalog), words);
        foreach (var catalog in new[] {stock,imported,installed})
        {
            AssertEqual(Hash(flat), catalog.ContentIdentity, "Glass original flattened identity");
            foreach (var frame in frames)
            {
                var runs = native[frame.Pointer];
                for (int run = 0; run < runs.Length; run++)
                {
                    for (int word = 0; word < runs[run].Length; word++)
                        AssertEqual(runs[run][word], catalog.GetWord(frame.Pointer,run,word), "Glass original stock word");
                    foreach (int bad in new[] {int.MinValue,-1,runs[run].Length,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "Glass stock word bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,runs.Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "Glass stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "Glass stock unknown pointer");
        entries[8].Blocks[12] = 0xc58;
        var mixed = new RoomPlmMotherBrainGlassVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Glass mixed custom/stock identity");
        entries[8].Blocks[12] = 0x5a;
        entries[10].Blocks[15] = 0x5b;
        foreach (var frame in frames)
        {
            int index = 0;
            for (int run = 0; run < native[frame.Pointer].Length; run++)
            for (int word = 0; word < native[frame.Pointer][run].Length; word++)
                AssertEqual(expected[frame.Pointer][index++], mixed.GetWord(frame.Pointer,run,word), "Glass cloned custom and unchanged stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainGlassVisualCatalog(entries[..1]), "Glass missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainGlassVisualCatalog(entries.Append(entries[0])), "Glass duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("INITIAL", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainGlassVisualCatalog(invalid), "Glass ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[7]);
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainGlassVisualCatalog(invalid), "Glass exact flattened shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainGlassVisualCatalog(entries), "Glass visual bits only");
    }
}
