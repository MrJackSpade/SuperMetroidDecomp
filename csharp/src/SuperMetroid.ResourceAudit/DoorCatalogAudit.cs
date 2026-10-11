using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.ResourceAudit;

/// <summary>Static definition/data analysis. Does not instantiate a runtime or advance a frame.</summary>
internal static class DoorCatalogAudit
{
    private sealed record Finding(string Code, string Owner, string Message);

    internal static int Run(string root, string output)
    {
        NativeDoorManifest native = Load(root);
        Dictionary<ushort, ushort[]> lists = CompiledLists();
        var findings = CompareLists(native, lists);
        var nativeExceptions = new List<Finding>();
        foreach (NativeDoorHeader header in native.Headers)
            if (CompareHeader(header) is string difference)
                findings.Add(new("DOOR003", $"$83:{header.Pointer:X4}", difference));
        foreach (NativeDoorRoom room in native.Rooms)
        {
            if (!RoomHeaderDefinitionsTooling.Contains(room.Room) || RoomHeaderDefinitions.Get(room.Room).DoorListPointer != room.List)
                findings.Add(new("DOOR004", room.Name, "Compiled room/list ownership differs from native header."));
            else if (!RoomStateSelectionDefinitions.GetStatePointers(room.Room).Order().SequenceEqual(room.States.Select(s => s.State).Order()))
                findings.Add(new("DOOR004", room.Name, "Compiled room-state inventory differs from native labels."));
            foreach (NativeDoorState state in room.States)
            {
                if (RoomStateDefinitions.Get(state.State).CompressedLevelDataAddress != state.Level)
                    findings.Add(new("DOOR004", room.Name, $"State $8F:{state.State:X4} selects a different level allocation."));
                foreach (int bts in state.DoorBts)
                {
                    int index = bts & 0x7f;
                    // The pinned unused room has this native invalid index and no inbound
                    // door. Preserve it as a visible data exception, never fabricate a door.
                    if (IsUnusedNativeDoorIndex(room, state, bts) &&
                        !native.Headers.Any(h => h.DestinationRoomPointer == room.Room))
                        nativeExceptions.Add(new("DOOR-NATIVE-UNUSED", room.Name,
                            "Unused room $8F:B3E1/state B3EE/level C8F40B has native BTS $01 with only one door; no native door points into this room."));
                    else if (index >= room.Doors.Length)
                        findings.Add(new("DOOR005", $"{room.Name} state $8F:{state.State:X4}",
                            $"Native level ${state.Level:X6} contains door BTS ${bts:X2} beyond its independently bounded {room.Doors.Length}-entry list. Requires native-data review; do not invent a destination."));
                    else if (!lists.TryGetValue(room.List, out ushort[]? compiled) || index >= compiled.Length || compiled[index] != room.Doors[index])
                        findings.Add(new("DOOR006", $"{room.Name} state $8F:{state.State:X4}",
                            $"Stored door BTS ${bts:X2} has a missing or incorrect compiled destination."));
                }
            }
        }
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        var report = new
        {
            native.Revision, native.RomSha256,
            Rooms = native.Rooms.Length, Headers = native.Headers.Length,
            References = native.Rooms.Sum(r => r.Doors.Length),
            States = native.Rooms.Sum(r => r.States.Length),
            StoredDoorIndexes = native.Rooms.Sum(r => r.States.Sum(s => s.DoorBts.Length)),
            Limitations = "Room-data references are imported static type-$9 blocks. Script-created or overwritten blocks and their reachability are not proven by this check. No gameplay was executed.",
            Findings = findings,
            NativeExceptions = nativeExceptions,
        };
        File.WriteAllText(output, JsonSerializer.Serialize(report, DoorCatalogManifest.Json) + "\n");
        foreach (Finding finding in findings)
            Console.Error.WriteLine($"{finding.Code} {finding.Owner}: {finding.Message}");
        foreach (Finding exception in nativeExceptions)
            Console.WriteLine($"{exception.Code}: {exception.Message}");
        Console.WriteLine($"Static door audit: {report.Rooms} rooms, {report.Headers} headers, {report.References} list entries, {report.States} states, {report.StoredDoorIndexes} stored door indexes; {findings.Count} errors, {nativeExceptions.Count} documented native exceptions.");
        return findings.Count == 0 ? 0 : 1;
    }

    private static bool IsUnusedNativeDoorIndex(NativeDoorRoom room, NativeDoorState state, int bts) =>
        room.Room == 0xb3e1 && room.List == 0xb408 && room.Doors.SequenceEqual(new ushort[] { 0x991e }) &&
        state.State == 0xb3ee && state.Level == 0xc8f40b && bts == 1;

    private static List<Finding> CompareLists(NativeDoorManifest native, Dictionary<ushort, ushort[]> lists)
    {
        var findings = new List<Finding>();
        foreach (NativeDoorRoom room in native.Rooms)
        {
            string owner = $"{room.Name} room $8F:{room.Room:X4} list $8F:{room.List:X4}";
            if (!lists.TryGetValue(room.List, out ushort[]? actual))
            {
                findings.Add(new("DOOR001", owner, "Missing independently labeled native list."));
                continue;
            }
            if (actual.Length != room.Doors.Length)
                findings.Add(new("DOOR001", owner, $"Expected {room.Doors.Length} entries from native label boundaries; compiled {actual.Length}."));
            for (int i = 0; i < Math.Min(actual.Length, room.Doors.Length); i++)
                if (actual[i] != room.Doors[i])
                    findings.Add(new("DOOR002", owner, $"BTS ${i:X2}: expected $83:{room.Doors[i]:X4}, compiled $83:{actual[i]:X4}."));
        }
        foreach (ushort extra in lists.Keys.Except(native.Rooms.Select(r => r.List)))
            findings.Add(new("DOOR001", $"$8F:{extra:X4}", "Compiled list has no native room owner."));
        return findings;
    }

    private static Dictionary<ushort, ushort[]> CompiledLists()
    {
        // Inventory the complete calculated identity domain, including unexpected
        // additions. No runtime, room loading, or gameplay is involved.
        var lists = new Dictionary<ushort, ushort[]>();
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            DoorListDefinition list;
            try { list = DoorDefinitionsTooling.GetList((ushort)value); }
            catch (ArgumentOutOfRangeException) { continue; }
            lists.Add(list.Pointer, list.DoorPointers.ToArray());
        }
        return lists;
    }

    private static NativeDoorManifest Load(string root)
    {
        string text = File.ReadAllText(Path.Combine(root, DoorCatalogManifest.RelativePath)).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))) != DoorCatalogManifest.ManifestHash)
            throw new InvalidDataException("Native door oracle changed. Regenerate from pinned sources and review its digest; do not derive it from compiled catalogs.");
        var native = JsonSerializer.Deserialize<NativeDoorManifest>(text)
            ?? throw new InvalidDataException("Missing native door manifest.");
        if (native.Revision != DoorCatalogManifest.Revision || native.RomSha256 != DoorCatalogManifest.RomHash ||
            native.Rooms.Length != 262 || native.Headers.Length != 599 || native.Rooms.Select(r => r.List).Distinct().Count() != 262 ||
            native.Rooms.Sum(r => r.Doors.Length) != 606 || native.Rooms.Sum(r => r.States.Length) != 323)
            throw new InvalidDataException("Native door manifest provenance or inventory is inconsistent.");
        return native;
    }

    /// <summary>
    /// A physical record must equal its compiled header after decoding at the production
    /// boundary. A pseudo-door must be accepted only as a door-list entry, never as a header.
    /// </summary>
    private static string? CompareHeader(NativeDoorHeader header)
    {
        if (header.IsElevatorPseudoDoor)
        {
            if (!Enum.IsDefined((ElevatorPseudoDoorPointer)header.Pointer))
                return "Native elevator pseudo-door is not a compiled pseudo-door entry.";
            DoorListEntry.ElevatorPseudoDoor((ElevatorPseudoDoorPointer)header.Pointer);
            try
            {
                DoorDefinitions.Get(header.Pointer);
                return "Native elevator pseudo-door is compiled as a physical header.";
            }
            catch (ArgumentOutOfRangeException) { return null; }
        }
        CartridgeDoorHeader expected;
        try { expected = header.Decode(); }
        catch (InvalidDataException) { return $"Native physical header has undecodable orientation ${header.Orientation:X2}."; }
        try
        {
            return DoorDefinitions.Get(header.Pointer) == expected ? null : "Compiled header differs from the pinned cartridge.";
        }
        catch (ArgumentOutOfRangeException) { return "Missing native physical header."; }
    }

    internal static void SelfCheck(string root)
    {
        NativeDoorManifest native = Load(root);
        var correct = native.Rooms.ToDictionary(r => r.List, r => r.Doors.ToArray());
        Require(CompareLists(native, correct).Count == 0, "native catalog accepted");
        Check(0xdad5, correct[0xdad5][..2], "DOOR001", "reported Tourian truncation before unknown sentinel");
        Check(0xd332, correct[0xd332][..3], "DOOR001", "Maridia sentinel omission");
        Check(0xdad5, [.. correct[0xdad5], 0xa99c], "DOOR001", "extra entry");
        ushort[] reordered = correct[0xdad5].ToArray();
        (reordered[0], reordered[1]) = (reordered[1], reordered[0]);
        Check(0xdad5, reordered, "DOOR002", "wrong BTS ordering");
        Check(0xdad5, [0xa984, 0xa990, 0xffff, 0xa99c], "DOOR002", "unknown pointer cannot terminate a list");
        var missing = new Dictionary<ushort, ushort[]>(correct);
        missing.Remove(0xdad5);
        Require(CompareLists(native, missing).Any(f => f.Code == "DOOR001"), "missing entire list");
        var extra = new Dictionary<ushort, ushort[]>(correct) { [0xffff] = [0xa99c] };
        Require(CompareLists(native, extra).Any(f => f.Code == "DOOR001"), "unexpected entire list");
        var unused = native.Rooms.Single(r => r.Room == 0xb3e1);
        Require(IsUnusedNativeDoorIndex(unused, unused.States.Single(), 1), "exact native unused-room exception");
        Require(!IsUnusedNativeDoorIndex(unused, unused.States.Single(), 2), "other invalid indexes are not exempt");
        Require(!IsUnusedNativeDoorIndex(unused with { Room = 0xdaae }, unused.States.Single(), 1), "other rooms are not exempt");
        NativeDoorHeader physical = native.Headers.First(h => !h.IsElevatorPseudoDoor);
        NativeDoorHeader pseudo = native.Headers.First(h => h.IsElevatorPseudoDoor);
        Require(CompareHeader(physical) is null && CompareHeader(pseudo) is null, "native headers accepted");
        Require(CompareHeader(physical with { PlmX = (byte)(physical.PlmX ^ 1) }) is not null, "changed physical field");
        Require(CompareHeader(physical with { Orientation = 0x0c }) is not null, "undecodable physical orientation");
        Require(CompareHeader(pseudo with { DestinationRoomPointer = 0x91f8 }) is not null, "pseudo-door identity cannot be a physical header");
        Require(CompareHeader(physical with { DestinationRoomPointer = 0 }) is not null, "physical record cannot pass as a pseudo-door");
        Console.WriteLine("Door audit omission contracts passed: Tourian/Maridia truncation, extra/missing entries, ordering, unresolved sentinel.");

        void Check(ushort pointer, ushort[] replacement, string code, string description)
        {
            var mutated = new Dictionary<ushort, ushort[]>(correct) { [pointer] = replacement };
            Require(CompareLists(native, mutated).Any(f => f.Code == code), description);
        }
        static void Require(bool condition, string description)
        {
            if (!condition) throw new InvalidDataException("Door audit self-check failed: " + description);
        }
    }
}
