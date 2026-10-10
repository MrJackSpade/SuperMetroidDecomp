using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks Kraid's ten PLM visual roles, their block mappings and stable identity, and rejection of malformed or out-of-range selections.</summary>
    private static void VerifyKraidRoomVisualSelection()
    {
        // Independent published pointer/ID/shape contract, not production enumeration.
        (ushort Pointer, string Id, int Count)[] definitions =
        [
            (0x9367, "crumble-first", 1), (0x936d, "crumble-second", 1),
            (0x9373, "crumble-third", 1), (0x9379, "ceiling-background-one", 1),
            (0x937f, "ceiling-background-two", 1), (0x9385, "ceiling-background-three", 1),
            (0x9391, "spike-first", 1), (0x9397, "spike-second", 1),
            (0x939d, "clear-ceiling", 15), (0x93bf, "clear-spikes", 22),
        ];
        var expected = new Dictionary<ushort, ushort[]>();
        var entries = new List<RoomPlmKraidVisualEntry>();
        for (int frame = 0; frame < definitions.Length; frame++)
        {
            var definition = definitions[frame];
            ushort[] words = Enumerable.Range(0, definition.Count)
                .Select(block => (ushort)(0x100 + 32 * frame + block)).ToArray();
            expected.Add(definition.Pointer, words);
            entries.Add(new(definition.Id, words.ToArray()));
        }
        var catalog = new RoomPlmKraidVisualCatalog(entries.AsEnumerable().Reverse());
        string expectedHash = SelectedPresentationHash.FromWordFrames(nameof(RoomPlmKraidVisualCatalog), expected);
        AssertEqual(expectedHash, catalog.ContentIdentity, "Kraid visual selection preserves sorted legacy identity");
        AssertEqual(expectedHash, new RoomPlmKraidVisualCatalog(entries).ContentIdentity,
            "Kraid visual selection ignores input ordering");
        foreach (RoomPlmKraidVisualEntry entry in entries)
            Array.Fill(entry.Blocks, (ushort)0);
        foreach (var definition in definitions)
        {
            for (int block = 0; block < definition.Count; block++)
                AssertEqual(expected[definition.Pointer][block], catalog.GetWord(definition.Pointer, 0, block),
                    "Kraid named frame preserves every independently supplied block and defensive copy");
            foreach (int block in new[] { int.MinValue, -1, definition.Count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(definition.Pointer, 0, block),
                    "Kraid visual block bounds");
            foreach (int run in new[] { int.MinValue, -1, 1, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(definition.Pointer, run, 0),
                    "Kraid visual run bounds");
        }
        AssertEqual(expectedHash, catalog.ContentIdentity, "Kraid copied frames preserve identity after input mutation");
        for (int pointer = 0x9365; pointer <= 0x93ef; pointer++)
            if (!expected.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => catalog.GetWord((ushort)pointer, 0, 0),
                    "Kraid visual selector rejects holes and neighboring draws");
        foreach (ushort pointer in new ushort[] { 0, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => catalog.GetWord(pointer, 0, 0),
                "Kraid visual selector rejects distant pointers");
        for (int index = 0; index < entries.Count; index++)
        {
            AssertThrows<InvalidDataException>(
                () => new RoomPlmKraidVisualCatalog(entries.Where((_, ordinal) => ordinal != index)),
                "Kraid visual selection rejects each missing role");
            AssertThrows<InvalidDataException>(
                () => new RoomPlmKraidVisualCatalog(entries.Append(entries[index])),
                "Kraid visual selection rejects each duplicate role");
        }
        AssertThrows<InvalidDataException>(() => new RoomPlmKraidVisualCatalog(
            entries.Append(new("unknown", [0]))), "Kraid visual selection rejects unknown ID");
        AssertThrows<InvalidDataException>(() => new RoomPlmKraidVisualCatalog(
            entries.Skip(1).Append(new("crumble-first", []))), "Kraid visual selection rejects empty frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmKraidVisualCatalog(
            entries.Skip(1).Append(new("crumble-first", [0xf000]))), "Kraid visual selection rejects physical bits");
        AssertThrows<ArgumentNullException>(() => new RoomPlmKraidVisualCatalog(null!),
            "Kraid visual selection rejects null source");
        Console.WriteLine("Kraid room visual selection: ten roles, all 45 blocks, legacy identity, copied data and validation pass.");
    }
}
