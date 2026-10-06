using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.ResourceAudit;

internal sealed record NativeDoorRoom(string Name, ushort Room, ushort List, ushort[] Doors,
    NativeDoorState[] States);
internal sealed record NativeDoorState(ushort State, int Level, int Blocks, int[] DoorBts);
internal sealed record NativeDoorManifest(string Revision, string RomSha256,
    NativeDoorRoom[] Rooms, CartridgeDoorHeader[] Headers);

/// <summary>Import-only native oracle: label boundaries, never compiled catalog membership.</summary>
internal static class DoorCatalogManifest
{
    internal const string Revision = "362be646929cf8e483f692b73a6561cfc2dc1d0d";
    internal const string RomHash = "12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72";
    internal const string RelativePath = "csharp/src/SuperMetroid.ResourceAudit/Data/native-door-catalog.json";
    // LF-normalized digest of the independently imported oracle. Catalog edits cannot
    // silently weaken its boundary/operand inventory; regenerations require review.
    internal const string ManifestHash = "DD25D46C08A04D7354F76AB4E2BDFEF62A6C67976DEF37743C84406640C9ED8A";
    internal static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    internal static int Generate(string disassembly, string rom, string output)
    {
        string bank8f = ReadPinned(Path.Combine(disassembly, "src/bank_8F.asm"),
            "65E10B8626E9DAB88EFEF53B7514FE0640767B47A1314B9A860409D473BF77CF");
        string bank83 = ReadPinned(Path.Combine(disassembly, "src/bank_83.asm"),
            "4421EA74AC219B27F1E17391B31D214B87D4A8C04DA33932E549EE609E31DE29");
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(rom))) != RomHash)
            throw new InvalidDataException("Door oracle requires the pinned unheadered J/U NTSC 1.0 cartridge.");
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath(rom));
        // Every label ends the previous allocation, even when its address is not a door.
        var labels83 = Labels(bank83);
        var symbols = labels83.Where(item => item.Name.StartsWith("Door_", StringComparison.Ordinal) ||
                item.Name.StartsWith("UNUSED_Door_", StringComparison.Ordinal))
            .Where(item => !item.Name.Contains("Debug", StringComparison.Ordinal))
            .ToDictionary(item => item.Name, item => Address(item.Body, "83"));
        var labels8f = Labels(bank8f);
        var nativeLists = new Dictionary<string, (ushort Pointer, ushort[] Entries)>();
        foreach (var label in labels8f.Where(item => (item.Name.StartsWith("RoomDoors_", StringComparison.Ordinal) || item.Name.StartsWith("UNUSED_RoomDoors_", StringComparison.Ordinal)) &&
                     item.Name != "RoomDoors_Debug"))
        {
            var entries = new List<ushort>();
            ushort start = 0;
            foreach (string line in label.Body.Split('\n'))
            {
                string code = line.Split(';')[0].Trim();
                if (code.Length == 0 || code == "endif") continue;
                Match entry = Regex.Match(line, @"^\s*dw\s+(\w+)\s*;8F([0-9A-F]{4});");
                if (!entry.Success) throw new InvalidDataException($"Unresolved native list syntax: {label.Name}: {line}");
                ushort address = Convert.ToUInt16(entry.Groups[2].Value, 16);
                if (entries.Count == 0) start = address;
                if (address != start + entries.Count * 2)
                    throw new InvalidDataException($"Noncontiguous native list {label.Name}.");
                ushort pointer = symbols[entry.Groups[1].Value];
                if (Word(0x8f0000 | address) != pointer)
                    throw new InvalidDataException($"Disassembly/cartridge disagreement in {label.Name}.");
                entries.Add(pointer);
            }
            if (entries.Count == 0) throw new InvalidDataException($"Empty native list {label.Name}.");
            nativeLists.Add(label.Name, (start, entries.ToArray()));
        }
        var rooms = new List<NativeDoorRoom>();
        var roomSections = Regex.Matches(bank8f, @"^(?:UNUSED_)?RoomHeader_(\w+):(?<body>.*?)(?=^(?:UNUSED_)?RoomHeader_|\z)",
            RegexOptions.Multiline | RegexOptions.Singleline);
        foreach (Match section in roomSections)
        {
            string name = section.Groups[1].Value;
            if (name == "Debug") continue;
            string body = section.Groups["body"].Value;
            ushort room = name.StartsWith("8F", StringComparison.Ordinal) ? Convert.ToUInt16(name[2..], 16) : Address(body, "8F");
            string listName = Regex.Match(body, @"%doorList\((\w+)\)").Groups[1].Value;
            var list = nativeLists[listName];
            if (Word(0x8f0000 | (room + 9)) != list.Pointer)
                throw new InvalidDataException($"Native room/list disagreement for {name}.");
            var states = new List<NativeDoorState>();
            foreach (Match state in Regex.Matches(body, @"^(?:UNUSED_)?RoomState_\w+:\s*;8F([0-9A-F]{4});", RegexOptions.Multiline))
            {
                ushort pointer = Convert.ToUInt16(state.Groups[1].Value, 16);
                var header = CartridgeRoomStateImporter.Load(bus, pointer);
                byte[] data = RomDataReader.Decompress(bus, header.CompressedLevelDataAddress);
                int bytes = BinaryPrimitives.ReadUInt16LittleEndian(data);
                int blocks = bytes / 2;
                if ((bytes & 1) != 0 || data.Length < 2 + bytes + blocks)
                    throw new InvalidDataException($"Malformed native level ${header.CompressedLevelDataAddress:X6}.");
                var bts = new SortedSet<byte>();
                for (int i = 0; i < blocks; i++)
                    if ((BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(2 + i * 2)) >> 12) == 9)
                        bts.Add(data[2 + bytes + i]);
                states.Add(new(pointer, header.CompressedLevelDataAddress, blocks, bts.Select(value => (int)value).ToArray()));
            }
            if (states.Count == 0) throw new InvalidDataException($"No independently labeled states for {name}.");
            rooms.Add(new(name, room, list.Pointer, list.Entries, states.ToArray()));
        }
        if (rooms.Count != 262 || nativeLists.Count != 262 || symbols.Count != 599)
            throw new InvalidDataException($"Native inventory changed: {rooms.Count} rooms, {nativeLists.Count} lists, {symbols.Count} headers.");
        var manifest = new NativeDoorManifest(Revision, RomHash, rooms.OrderBy(item => item.Room).ToArray(),
            symbols.Values.Order().Select(pointer => CartridgeDoorHeaderImporter.Load(bus, pointer)).ToArray());
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
        File.WriteAllText(output, JsonSerializer.Serialize(manifest, Json) + "\n");
        Console.WriteLine($"Imported {rooms.Count} independently bounded door lists, {manifest.Headers.Length} headers and {rooms.Sum(r => r.States.Length)} state data summaries. No gameplay executed.");
        return 0;

        ushort Word(int address) => (ushort)(bus.ReadCartridgeByte(address) | bus.ReadCartridgeByte(address + 1) << 8);
    }

    private static string ReadPinned(string path, string hash)
    {
        byte[] bytes = File.ReadAllBytes(path);
        if (Convert.ToHexString(SHA256.HashData(bytes)) != hash)
            throw new InvalidDataException($"Pinned disassembly source changed: {path}");
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    private static ushort Address(string body, string bank)
    {
        Match address = Regex.Match(body, ";" + bank + @"([0-9A-F]{4});");
        if (!address.Success) throw new InvalidDataException("Missing native address annotation.");
        return Convert.ToUInt16(address.Groups[1].Value, 16);
    }

    private static (string Name, string Body)[] Labels(string source) =>
        Regex.Matches(source, @"^(\w+):(?<body>.*?)(?=^\w+:|\z)", RegexOptions.Multiline | RegexOptions.Singleline)
            .Select(match => (match.Groups[1].Value, match.Groups["body"].Value)).ToArray();
}
