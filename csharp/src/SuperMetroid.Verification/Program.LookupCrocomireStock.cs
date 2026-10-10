using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Checks that Crocomire's native PLM draw records map to the same visual words and
    /// content identity in stock, reordered, and installed catalogs, including bounds and validation.
    /// </summary>
    /// <param name="rom">Retail address space containing Crocomire's native PLM draw records.</param>
    /// <param name="installed">Visual catalog extracted from the game installation under verification.</param>
    private static void VerifyCrocomireStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmCrocomireVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0x9b5b,"clear-bridge"), (0x9b73,"crumble-bridge-block"),
            (0x9b79,"clear-bridge-block"), (0x9b7f,"clear-invisible-wall"), (0x9bbb,"create-invisible-wall")];
        var native = new Dictionary<ushort, ushort[]>();
        var widths = new Dictionary<ushort, int>();
        foreach (var frame in frames)
        {
            var words = new List<ushort>();
            int cursor = frame.Pointer;
            widths.Add(frame.Pointer, ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff);
            while (true)
            {
                int count = ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0x7fff;
                cursor += 2;
                for (int index = 0; index < count; index++, cursor += 2)
                    words.Add((ushort)(ReadSamusEaterPlmWord(rom, 0x840000 | cursor) & 0xfff));
                byte x = rom.ReadByte(0x840000 | cursor++), y = rom.ReadByte(0x840000 | cursor++);
                if (x == 0 && y == 0) break;
            }
            native.Add(frame.Pointer, words.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmCrocomireVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmCrocomireVisualCatalog.Stock();
        var imported = new RoomPlmCrocomireVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmCrocomireVisualCatalog), words);
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Crocomire original flattened hash");
            foreach (var frame in frames)
            {
                for (int index = 0; index < native[frame.Pointer].Length; index++)
                    AssertEqual(native[frame.Pointer][index], catalog.GetWord(frame.Pointer,index / widths[frame.Pointer],index % widths[frame.Pointer]), "Crocomire native stock word");
                foreach (int bad in new[] {int.MinValue,-1,native[frame.Pointer].Length / widths[frame.Pointer],int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "Crocomire run bounds");
                foreach (int bad in new[] {int.MinValue,-1,widths[frame.Pointer],int.MaxValue})
                for (int run = 0; run < native[frame.Pointer].Length / widths[frame.Pointer]; run++)
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "Crocomire block bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "Crocomire unknown pointer");
        entries[4].Blocks[23] = 0x0c58;
        entries[2].Blocks[0] = 0x0059;
        var mixed = new RoomPlmCrocomireVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Crocomire mixed custom stock hash");
        entries[4].Blocks[23] = 0x005a;
        entries[1].Blocks[0] = 0x005b;
        foreach (var frame in frames)
        for (int index = 0; index < native[frame.Pointer].Length; index++)
            AssertEqual(expected[frame.Pointer][index], mixed.GetWord(frame.Pointer,index / widths[frame.Pointer],index % widths[frame.Pointer]), "Crocomire clone isolation and stock fallback");
        AssertThrows<InvalidDataException>(() => new RoomPlmCrocomireVisualCatalog(entries[..4]), "Crocomire missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmCrocomireVisualCatalog(entries.Append(entries[0])), "Crocomire duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("CLEAR-BRIDGE", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmCrocomireVisualCatalog(invalid), "Crocomire ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[3]);
        AssertThrows<InvalidDataException>(() => new RoomPlmCrocomireVisualCatalog(invalid), "Crocomire exact frame shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmCrocomireVisualCatalog(entries), "Crocomire visual bits only");
    }

}
