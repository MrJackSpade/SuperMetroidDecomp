using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyMotherBrainStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmMotherBrainFakeDeathVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0x94a3,"fill-wall"),(0x94b1,"escape-door"),
            (0x9505,"background-row-2"),(0x9523,"background-row-3"),(0x9541,"background-row-4"),
            (0x955f,"background-row-5"),(0x957d,"background-row-6"),(0x959b,"background-row-7"),
            (0x95b9,"background-row-8"),(0x95d7,"background-row-9"),(0x95f5,"background-row-a"),
            (0x9613,"background-row-b"),(0x9631,"background-row-c"),(0x964f,"background-row-d"),
            (0x966d,"background-row-e-unused"),(0x968b,"background-row-f-unused"),
            (0x96a9,"clear-ceiling-block"),(0x96b1,"clear-ceiling-tube"),
            (0x96bf,"clear-bottom-middle-side-tube"),(0x96cb,"clear-bottom-middle-tubes"),
            (0x96ef,"clear-bottom-left-tube"),(0x9703,"clear-bottom-right-tube")];
        var native = new Dictionary<ushort, ushort[]>();
        var widths = new Dictionary<ushort, int[]>();
        foreach (var frame in frames)
        {
            var words = new List<ushort>();
            var counts = new List<int>();
            int cursor = frame.Pointer;
            while (true)
            {
                int count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff;
                counts.Add(count);
                cursor += 2;
                for (int index = 0; index < count; index++, cursor += 2)
                    words.Add((ushort)(ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0xfff));
                byte x = rom.ReadByte(0x840000 | cursor++), y = rom.ReadByte(0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, words.ToArray());
            widths.Add(frame.Pointer, counts.ToArray());
        }
        AssertEqual(230, native.Values.Sum(words => words.Length), "Mother Brain original visual cell count");
        var entries = frames.Select(frame => new RoomPlmMotherBrainFakeDeathVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmMotherBrainFakeDeathVisualCatalog.Stock();
        var imported = new RoomPlmMotherBrainFakeDeathVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmMotherBrainFakeDeathVisualCatalog), words);
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Mother Brain original flattened hash");
            foreach (var frame in frames)
            {
                int flat = 0;
                for (int run = 0; run < widths[frame.Pointer].Length; run++)
                {
                    for (int block = 0; block < widths[frame.Pointer][run]; block++)
                        AssertEqual(native[frame.Pointer][flat++], catalog.GetWord(frame.Pointer,run,block), "Mother Brain native stock word");
                    foreach (int bad in new[] {int.MinValue,-1,widths[frame.Pointer][run],int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "Mother Brain block bounds");
                }
                foreach (int bad in new[] {int.MinValue,-1,widths[frame.Pointer].Length,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "Mother Brain run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "MotherBrainFakeDeath unknown pointer");
        entries[21].Blocks[5] = 0x0c58;
        entries[2].Blocks[0] = 0x0059;
        var mixed = new RoomPlmMotherBrainFakeDeathVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "MotherBrainFakeDeath mixed custom stock hash");
        entries[21].Blocks[5] = 0x005a;
        entries[1].Blocks[0] = 0x005b;
        foreach (var frame in frames)
        {
            int flat = 0;
            for (int run = 0; run < widths[frame.Pointer].Length; run++)
            for (int block = 0; block < widths[frame.Pointer][run]; block++)
                AssertEqual(expected[frame.Pointer][flat++], mixed.GetWord(frame.Pointer,run,block), "Mother Brain cloned custom data and stock fallback");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainFakeDeathVisualCatalog(entries[..21]), "MotherBrainFakeDeath missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainFakeDeathVisualCatalog(entries.Append(entries[0])), "MotherBrainFakeDeath duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("FILL-WALL", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainFakeDeathVisualCatalog(invalid), "MotherBrainFakeDeath ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[4]);
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainFakeDeathVisualCatalog(invalid), "MotherBrainFakeDeath exact frame shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmMotherBrainFakeDeathVisualCatalog(entries), "MotherBrainFakeDeath visual bits only");
    }

}
