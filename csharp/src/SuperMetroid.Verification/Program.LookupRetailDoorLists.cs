using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRetailDoorListMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] rooms = File.ReadLines(Path.GetFullPath(Path.Combine("upstream-sm", "assets", "names.txt")))
            .Select(TryParseRoomHeaderPointer).Where(pointer => pointer.HasValue)
            .Select(pointer => pointer!.Value).Distinct().Order().ToArray();
        var physical = EnumerateRetailDoorPointers().ToHashSet();
        physical.Add(0x88fc);
        physical.Add(0xa18a);
        var originalLists = new HashSet<ushort>();
        var referenced = new HashSet<ushort>();
        int total = 0, elevatorReferences = 0;
        foreach (ushort room in rooms)
        {
            ushort pointer = ReadVerificationWord(rom, (0x8f0000 | room) + 9);
            AssertTrue(originalLists.Add(pointer), "Each retail room has a distinct original door-list identity");
            var original = new List<ushort>();
            int address = 0x8f0000 | pointer;
            // Independently derive native length; never use the production count
            // to decide how much oracle data belongs to this list.
            for (int index = 0; ; index++)
            {
                AssertTrue(index < 128, "Native door list has a bounded end before the BTS domain ends");
                ushort target = ReadVerificationWord(rom, address + index * 2);
                if (!physical.Contains(target)) break;
                original.Add(target);
                referenced.Add(target);
                if (target is 0x88fc or 0xa18a) elevatorReferences++;
            }
            DoorListDefinition actual = DoorDefinitionsTooling.GetList(pointer);
            AssertEqual(pointer, actual.Pointer, "Materialized door list preserves source identity");
            AssertTrue(actual.DoorPointers.Span.SequenceEqual(original.ToArray()),
                $"Door list {pointer:X4} count and all original targets match");
            for (int behavior = 0; behavior <= byte.MaxValue; behavior++)
            {
                byte bts = (byte)behavior;
                int index = behavior & 0x7f;
                if (index < original.Count)
                    AssertEqual(original[index], DoorDefinitions.Resolve(pointer, bts).Pointer,
                        $"Door list {pointer:X4} BTS {behavior:X2} preserves physical/elevator target");
                else
                    AssertThrows<InvalidDataException>(() => DoorDefinitions.Resolve(pointer, bts),
                        "Every out-of-list BTS rejects, including high-bit aliases");
            }
            total += original.Count;
        }
        AssertEqual(262, originalLists.Count, "All original retail door lists");
        AssertEqual(606, total, "All original room-door references");
        AssertEqual(14, elevatorReferences, "All original shared elevator insertions");
        AssertEqual(594, referenced.Count, "All unique physical targets plus shared elevator");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            if (originalLists.Contains(pointer)) continue;
            AssertThrows<ArgumentOutOfRangeException>(() => DoorDefinitionsTooling.GetList(pointer), "Unknown list rejects materialization");
            AssertThrows<ArgumentOutOfRangeException>(() => DoorDefinitions.Resolve(pointer, 0), "Unknown list rejects resolution");
        }
        Console.WriteLine("Retail door lists: 262 identities, 606 original references, 14 elevator insertions, all256 BTS values and every unknown list identity pass.");
    }
}
