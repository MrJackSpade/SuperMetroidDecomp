using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledRoomStateSelectionDefinitions()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Room selector oracle revision");
        ushort[] rooms = File.ReadLines(Path.GetFullPath(Path.Combine("upstream-sm", "assets", "names.txt")))
            .Select(TryParseRoomHeaderPointer).Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value).Distinct().Order().ToArray();
        AssertEqual(262, rooms.Length, "Original retail room selector identities");
        var known = rooms.ToHashSet();
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            if (known.Contains(pointer)) continue;
            AssertThrows<ArgumentOutOfRangeException>(() => RoomStateSelectionDefinitions.Select(pointer, default),
                "Unknown room selector rejects before condition evaluation");
            AssertThrows<ArgumentOutOfRangeException>(() => RoomStateSelectionDefinitions.GetStatePointers(pointer),
                "Unknown room variant enumeration rejects");
        }

        int conditional = 0, clauses = 0, comparisons = 0;
        var reached = new HashSet<ushort>();
        foreach (ushort room in rooms)
        {
            // Independently decode original executable selectors. Do not use the
            // room importer: it delegates selection to the production implementation.
            var native = new List<(ushort Code, byte Operand, ushort Target)>();
            int address = 0x8f0000 | (room + 11);
            ushort Word(int at) => (ushort)(rom.ReadByte(at) | rom.ReadByte(at + 1) << 8);
            while (Word(address) != 0xe5e6)
            {
                AssertTrue(native.Count < 8, "Native selector has a bounded terminator");
                ushort code = Word(address);
                address += 2;
                byte operand = code switch
                {
                    0xe612 or 0xe629 => rom.ReadByte(address++),
                    0xe5ff => 1,
                    0xe652 or 0xe669 => 0,
                    _ => throw new InvalidDataException($"Unexpected native selector {code:X4}"),
                };
                native.Add((code, operand, Word(address)));
                address += 2;
            }
            ushort fallback = (ushort)(address + 2);
            if (native.Count != 0) conditional++;
            clauses += native.Count;
            ushort[] expectedVariants = new[] { fallback }.Concat(native.Select(clause => clause.Target)).ToArray();
            AssertTrue(RoomStateSelectionDefinitions.GetStatePointers(room).SequenceEqual(expectedVariants),
                $"Room {room:X4} default and alternatives retain native order");

            // All truth assignments of the predicates this native program actually
            // reads cover every branch and simultaneous-match priority. Truncated
            // event storage and irrelevant set bits exercise the caller boundary.
            for (int assignment = 0; assignment < 1 << native.Count; assignment++)
            foreach (int eventLength in new[] { 0, 1, 2, 3, 8 })
            foreach (bool noise in new[] { false, true })
            {
                byte[] events = Enumerable.Repeat(noise ? (byte)255 : (byte)0, eventLength).ToArray();
                byte bosses = noise ? (byte)255 : (byte)0;
                bool morph = noise, powerBombs = noise;
                for (int index = 0; index < native.Count; index++)
                {
                    var clause = native[index];
                    bool enabled = (assignment & (1 << index)) != 0;
                    if (clause.Code == 0xe612 && clause.Operand / 8 < events.Length)
                    {
                        int bit = 1 << (clause.Operand % 8);
                        ref byte storage = ref events[clause.Operand / 8];
                        storage = (byte)(enabled ? storage | bit : storage & ~bit);
                    }
                    else if (clause.Code is 0xe5ff or 0xe629)
                        bosses = (byte)(enabled ? bosses | clause.Operand : bosses & ~clause.Operand);
                    else if (clause.Code == 0xe652) morph = enabled;
                    else if (clause.Code == 0xe669) powerBombs = enabled;
                }
                ushort expected = fallback;
                foreach (var clause in native)
                {
                    bool matches = clause.Code switch
                    {
                        0xe612 => clause.Operand / 8 < events.Length &&
                            (events[clause.Operand / 8] & (1 << (clause.Operand % 8))) != 0,
                        0xe5ff or 0xe629 => (bosses & clause.Operand) != 0,
                        0xe652 => morph,
                        0xe669 => powerBombs,
                        _ => false,
                    };
                    if (matches) { expected = clause.Target; break; }
                }
                var context = new RoomStateSelectionContext(events, (BossBits)bosses, morph, powerBombs);
                AssertEqual(expected, RoomStateSelectionDefinitions.Select(room, context),
                    $"Room {room:X4} native first-match/default selection");
                AssertEqual(expected, CartridgeRoomHeader.LoadUsingCompiledSelection(room, context).State.Pointer,
                    "Production room loading uses the selected compiled state without cartridge capability");
                reached.Add(expected);
                comparisons++;
            }
        }
        AssertEqual(54, conditional, "All native conditional room programs");
        AssertEqual(61, clauses, "All native conditional branch destinations");
        AssertEqual(323, reached.Count, "All original selected room states reached by their predicate partitions");
        Console.WriteLine($"Room selectors: 262 identities, 54 conditional programs, 61 ordered branches, 323 states and {comparisons} native predicate-partition comparisons pass.");
    }
}
