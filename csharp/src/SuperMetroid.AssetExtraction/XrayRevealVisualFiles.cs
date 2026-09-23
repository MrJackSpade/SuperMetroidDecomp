using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Extracts the visual metatile operands of the retail X-ray reveal table. Rule selection,
/// copy shape, extensions, and area gating stay in the compiled collision dispatcher.
/// </summary>
public static class XrayRevealVisualFiles
{
    public const string VisualFileName = "reveals.json";
    public const string ManifestFileName = "manifest.json";
    private const int FormatVersion = 2;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCartridgeSha256);
        Directory.CreateDirectory(directory);

        var groups = new Dictionary<(RoomCollisionType Type, XrayRevealDefinition Definition),
            List<int>>();
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            XrayRevealDefinition? native = ReadNative(bus, type, unchecked((byte)bts));
            if (native != XrayRevealTable.Find(type, unchecked((byte)bts)))
                throw new InvalidDataException(
                    $"Compiled X-ray rule {type}/BTS ${bts:X2} differs from the cartridge.");
            if (native is not { } definition || !XrayRevealVisualCatalog.IsDrawable(definition.Command))
                continue;
            var key = (type, definition);
            if (!groups.TryGetValue(key, out List<int>? values))
                groups.Add(key, values = []);
            values.Add(bts);
        }

        XrayRevealVisualEntry[] entries = groups
            .Select(pair => new XrayRevealVisualEntry(
                pair.Key.Type, pair.Value.ToArray(), NameOf(pair.Key.Definition.Command),
                pair.Key.Definition.TopLeft, pair.Key.Definition.TopRight,
                pair.Key.Definition.BottomLeft, pair.Key.Definition.BottomRight))
            .OrderBy(entry => entry.CollisionType)
            .ThenBy(entry => entry.BtsValues[0])
            .ToArray();
        ushort[] itemMetatiles = Enumerable.Range(0, XrayOverlayRomData.DynamicGraphicsSlots * 2)
            .Select(slot =>
            {
                ushort pointer = ReadAbsoluteWord(bus,
                    XrayOverlayRomData.ItemDrawPointers + slot * sizeof(ushort));
                if (pointer < 0x8000 || pointer > ushort.MaxValue - 3)
                    throw new InvalidDataException($"X-ray item draw pointer ${pointer:X4} is invalid.");
                return (ushort)(ReadAbsoluteWord(bus,
                    XrayOverlayRomData.ItemBank | (pointer + 2)) & 0x0fff);
            }).ToArray();
        XrayRoomOverlayEntry[] rooms = RoomStateDefinitions.All
            .Select(state => state.XrayPointer).Where(pointer => pointer != 0)
            .Distinct().OrderBy(pointer => pointer)
            .Select(pointer => ReadRoomOverlay(bus, pointer)).ToArray();
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
            new XrayRevealVisualDocument(FormatVersion, entries, itemMetatiles, rooms),
            JsonOptions);
        using (var file = new FileStream(Path.Combine(directory, VisualFileName),
                   FileMode.CreateNew, FileAccess.Write))
            file.Write(json);
        using var manifest = new FileStream(Path.Combine(directory, ManifestFileName),
            FileMode.CreateNew, FileAccess.Write);
        JsonSerializer.Serialize(manifest,
            new XrayRevealVisualManifest(FormatVersion, sourceCartridgeSha256,
                Convert.ToHexString(SHA256.HashData(json))), JsonOptions);
    }

    public static XrayRevealVisualCatalog Load(string stockDirectory,
        string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, ManifestFileName);
        XrayRevealVisualManifest manifest = ReadJson<XrayRevealVisualManifest>(manifestPath);
        if (manifest.Version != FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"X-ray manifest {manifestPath} is incompatible.");
        string stockPath = Path.Combine(stockDirectory, VisualFileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(stockBytes)),
                manifest.VisualSha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Stock X-ray visuals {stockPath} failed the manifest hash.");
        XrayRevealVisualDocument stock = ReadJson<XrayRevealVisualDocument>(stockBytes, stockPath);
        ValidateStructure(stock, stockPath);

        string? overridePath = overrideDirectory is null ? null :
            Path.Combine(overrideDirectory, VisualFileName);
        XrayRevealVisualDocument selected = overridePath is not null && File.Exists(overridePath)
            ? ReadJson<XrayRevealVisualDocument>(overridePath)
            : stock;
        string selectedPath = overridePath is not null && File.Exists(overridePath)
            ? overridePath : stockPath;
        ValidateStructure(selected, selectedPath);
        if (selected.Entries.Length != stock.Entries.Length)
            throw new InvalidDataException($"X-ray visuals {selectedPath} changed rule count.");
        if (selected.ItemMetatiles.Length != stock.ItemMetatiles.Length ||
            selected.Rooms.Length != stock.Rooms.Length)
            throw new InvalidDataException($"X-ray visuals {selectedPath} changed overlay count.");

        var mappings = new List<(RoomCollisionType Type, byte Bts, XrayRevealVisualWords Visual)>();
        for (int index = 0; index < stock.Entries.Length; index++)
        {
            XrayRevealVisualEntry original = stock.Entries[index];
            XrayRevealVisualEntry edited = selected.Entries[index];
            if (edited.CollisionType != original.CollisionType ||
                edited.Shape != original.Shape ||
                !edited.BtsValues.SequenceEqual(original.BtsValues))
                throw new InvalidDataException(
                    $"X-ray visuals {selectedPath} changed the compiled rule at entry {index}.");
            var visual = new XrayRevealVisualWords(edited.TopLeft, edited.TopRight,
                edited.BottomLeft, edited.BottomRight);
            foreach (int bts in edited.BtsValues)
                mappings.Add((edited.CollisionType, unchecked((byte)bts), visual));
        }
        for (int index = 0; index < stock.Rooms.Length; index++)
        {
            if (selected.Rooms[index].Pointer != stock.Rooms[index].Pointer ||
                selected.Rooms[index].Tiles.Length != stock.Rooms[index].Tiles.Length)
                throw new InvalidDataException(
                    $"X-ray visuals {selectedPath} changed room-overlay structure at entry {index}.");
        }
        var overlays = new XrayOverlayVisualCatalog(selected.ItemMetatiles,
            selected.Rooms.Select(room => (room.Pointer,
                (IReadOnlyList<XrayRoomOverlayVisual>)room.Tiles)));
        return new XrayRevealVisualCatalog(mappings, overlays);
    }

    public static void ValidateStock(string directory) => _ = Load(directory, null);

    private static void ValidateStructure(XrayRevealVisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null ||
            document.ItemMetatiles is null || document.Rooms is null ||
            document.ItemMetatiles.Length != XrayOverlayRomData.DynamicGraphicsSlots * 2 ||
            document.ItemMetatiles.Any(word => word > 0x0fff) ||
            document.Rooms.Any(room => room is null))
            throw new InvalidDataException($"X-ray visuals {path} have an incompatible format.");
        ushort[] nativePointers = RoomStateDefinitions.All.Select(state => state.XrayPointer)
            .Where(pointer => pointer != 0).Distinct().OrderBy(pointer => pointer).ToArray();
        if (!document.Rooms.Select(room => room.Pointer).SequenceEqual(nativePointers))
            throw new InvalidDataException($"X-ray visuals {path} changed room-overlay identities.");
        foreach (XrayRoomOverlayEntry room in document.Rooms)
        {
            if (room.Tiles is null || room.Tiles.Length == 0 ||
                room.Tiles.Any(tile => tile.Word > 0x0fff))
                throw new InvalidDataException(
                    $"X-ray visuals {path} contain an invalid room overlay ${room.Pointer:X4}.");
        }
        foreach (XrayRevealVisualEntry entry in document.Entries)
        {
            if (entry is null || entry.BtsValues is null || entry.BtsValues.Length == 0 ||
                entry.BtsValues.Any(value => value is < 0 or > byte.MaxValue))
                throw new InvalidDataException($"X-ray visuals {path} contain an invalid BTS group.");
            ushort command = XrayRevealTable.Find(entry.CollisionType,
                unchecked((byte)entry.BtsValues[0]))?.Command ?? 0;
            if (!XrayRevealVisualCatalog.IsDrawable(command) || entry.Shape != NameOf(command))
                throw new InvalidDataException($"X-ray visuals {path} contain an invalid copy shape.");
            if (entry.TopLeft > 0x0fff || entry.TopRight > 0x0fff ||
                entry.BottomLeft > 0x0fff || entry.BottomRight > 0x0fff)
                throw new InvalidDataException($"X-ray visuals {path} contain a nonvisual metatile index.");
            if ((command is XrayRevealCodePointers.CopyOne or XrayRevealCodePointers.CopyBrinstar &&
                    (entry.TopRight != 0 || entry.BottomLeft != 0 || entry.BottomRight != 0)) ||
                (command == XrayRevealCodePointers.CopyWide &&
                    (entry.BottomLeft != 0 || entry.BottomRight != 0)) ||
                (command == XrayRevealCodePointers.CopyTall &&
                    (entry.TopRight != 0 || entry.BottomRight != 0)))
                throw new InvalidDataException($"X-ray visuals {path} set unused copy operands.");
        }
    }

    private static string NameOf(ushort command) => command switch
    {
        XrayRevealCodePointers.CopyOne => "one",
        XrayRevealCodePointers.CopyWide => "wide",
        XrayRevealCodePointers.CopyTall => "tall",
        XrayRevealCodePointers.CopySquare => "square",
        XrayRevealCodePointers.CopyBrinstar => "brinstar-only",
        _ => throw new InvalidDataException($"X-ray command $91:{command:X4} has no visual shape."),
    };

    private static XrayRevealDefinition? ReadNative(ISnesAddressSpace bus,
        RoomCollisionType type, byte bts)
    {
        int typeWord = (int)type << 12;
        for (int entry = XrayRevealCodePointers.BlockTypeTable; ; entry += 4)
        {
            ushort key = ReadWord(bus, entry);
            if (key == XrayRevealCodePointers.End) return null;
            if (key != typeWord) continue;
            for (int match = ReadWord(bus, entry + 2); ; match += 4)
            {
                ushort value = ReadWord(bus, match);
                if (value == XrayRevealCodePointers.End) return null;
                if (value != XrayRevealCodePointers.AnyBts && value != bts) continue;
                int pointer = ReadWord(bus, match + 2);
                ushort command = ReadWord(bus, pointer);
                return command switch
                {
                    XrayRevealCodePointers.HorizontalExtension or
                        XrayRevealCodePointers.VerticalExtension => new(command, 0, 0, 0, 0),
                    XrayRevealCodePointers.CopyOne or XrayRevealCodePointers.CopyBrinstar =>
                        new(command, ReadWord(bus, pointer + 2), 0, 0, 0),
                    XrayRevealCodePointers.CopyWide =>
                        new(command, ReadWord(bus, pointer + 2), ReadWord(bus, pointer + 4), 0, 0),
                    XrayRevealCodePointers.CopyTall =>
                        new(command, ReadWord(bus, pointer + 2), 0, ReadWord(bus, pointer + 4), 0),
                    XrayRevealCodePointers.CopySquare =>
                        new(command, ReadWord(bus, pointer + 2), ReadWord(bus, pointer + 4),
                            ReadWord(bus, pointer + 6), ReadWord(bus, pointer + 8)),
                    _ => throw new InvalidDataException(
                        $"Native X-ray reveal command $91:{command:X4} is unknown."),
                };
            }
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int pointer)
    {
        if (pointer < 0x8000 || pointer >= ushort.MaxValue)
            throw new InvalidDataException($"X-ray reveal table crossed bank $91 at ${pointer:X}.");
        return unchecked((ushort)(bus.ReadByte(XrayRevealCodePointers.Bank | pointer) |
            bus.ReadByte(XrayRevealCodePointers.Bank | (pointer + 1)) << 8));
    }

    private static XrayRoomOverlayEntry ReadRoomOverlay(ISnesAddressSpace bus, ushort pointer)
    {
        var tiles = new List<XrayRoomOverlayVisual>();
        for (int cursor = pointer; cursor <= ushort.MaxValue - 3; cursor += 4)
        {
            ushort coordinate = ReadAbsoluteWord(bus, XrayOverlayRomData.RoomBank | cursor);
            if (coordinate == 0)
            {
                if (tiles.Count == 0)
                    throw new InvalidDataException($"X-ray room overlay ${pointer:X4} is empty.");
                return new XrayRoomOverlayEntry(pointer, tiles.ToArray());
            }
            ushort word = ReadAbsoluteWord(bus, XrayOverlayRomData.RoomBank | (cursor + 2));
            if (word > 0x0fff)
                throw new InvalidDataException(
                    $"X-ray room overlay ${pointer:X4} has nonvisual bits ${word:X4}.");
            tiles.Add(new XrayRoomOverlayVisual((byte)coordinate,
                (byte)(coordinate >> 8), word));
        }
        throw new InvalidDataException($"X-ray room overlay ${pointer:X4} has no terminator.");
    }

    private static ushort ReadAbsoluteWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

    private static T ReadJson<T>(string path) where T : class =>
        ReadJson<T>(File.ReadAllBytes(path), path);

    private static T ReadJson<T>(byte[] bytes, string path) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions) ??
                throw new InvalidDataException($"X-ray JSON {path} is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Invalid X-ray JSON {path}.", error);
        }
    }

    private sealed record XrayRevealVisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    private sealed record XrayRevealVisualDocument(int Version, XrayRevealVisualEntry[] Entries,
        ushort[] ItemMetatiles, XrayRoomOverlayEntry[] Rooms);
    private sealed record XrayRevealVisualEntry(RoomCollisionType CollisionType, int[] BtsValues,
        string Shape, ushort TopLeft, ushort TopRight, ushort BottomLeft, ushort BottomRight);
    private sealed record XrayRoomOverlayEntry(ushort Pointer, XrayRoomOverlayVisual[] Tiles);
}
