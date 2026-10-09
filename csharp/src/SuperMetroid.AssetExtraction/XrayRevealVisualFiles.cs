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
    /// <summary>Family-relative version-two stock/override JSON filename for reveal-rule operands, eight item metatiles, and special-room overlay records.</summary>
    public const string VisualFileName = "reveals.json";
    /// <summary>Stock manifest filename containing version-two format, caller-supplied cartridge provenance, and the reveal JSON's SHA-256.</summary>
    public const string ManifestFileName = "manifest.json";
    /// <summary>Schema version required by both X-ray visual JSON documents.</summary>
    private const int FormatVersion = 2;

    /// <summary>Strict camel-case JSON settings with collision types serialized by name.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Checks all 16-by-256 native collision/BTS reveal results, then exports drawable operands, item-slot metatiles, and room-specific X-ray overlays.</summary>
    /// <param name="bus">Cartridge source for bank-$91 reveal records, eight bank-$84 item draw pointers at $84:839D, and bank-$8F special-X-ray lists referenced by compiled room states.</param>
    /// <param name="directory">Family directory, created before native validation; receives new <see cref="VisualFileName"/> and <see cref="ManifestFileName"/> files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank caller-supplied provenance hash, recorded without recomputation; stock load requires the supported cartridge identity.</param>
    /// <remarks>Drawable rules are grouped by collision type and native definition. Item words are masked to twelve visual bits; room coordinates are unsigned 16-pixel block positions. Create-new writes refuse existing outputs and are not an atomic JSON/manifest transaction.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">A native rule differs from compiled definitions, a pointer/command is invalid, or a room overlay is empty, unterminated, or contains nonvisual words.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
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

    /// <summary>Validates version-two stock X-ray presentation and selects an optional complete replacement while retaining native rule selection, copy geometry, extensions, and area gating.</summary>
    /// <param name="stockDirectory">Family directory containing the required stock reveal JSON and provenance/hash manifest.</param>
    /// <param name="overrideDirectory">Optional family directory; null or missing reveal JSON selects validated stock.</param>
    /// <returns>A ROM-independent catalog with copied rule operands, eight item slots, and ordered special-room tile lists.</returns>
    /// <remarks>Stock provenance, byte hash, and structure are checked even with overrides; loading does not repeat cartridge reads or compare every stock operand with cartridge bytes. Strict JSON rejects unknown/missing constructor properties. Overrides need no manifest but must retain stock rule order, collision types, BTS groups, shapes, room identities, and record counts; twelve-bit words and byte-range room coordinates may change.</remarks>
    /// <exception cref="ArgumentException"><paramref name="stockDirectory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Provenance, integrity, strict JSON schema, rule coverage/shape, unused operands, overlay identities/counts, or visual words are invalid.</exception>
    /// <exception cref="IOException">Required stock or selected override files cannot be read.</exception>
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
                (IReadOnlyList<XrayRoomOverlayVisual>)room.Tiles.Select(tile =>
                    new XrayRoomOverlayVisual(tile.X, tile.Y, tile.Word)).ToArray())));
        return new XrayRevealVisualCatalog(mappings, overlays);
    }

    /// <summary>Runs stock provenance, hash, strict-schema, and catalog-coverage checks through <see cref="Load"/> without overrides, cartridge reads, or writes.</summary>
    /// <param name="directory">X-ray family directory containing both required version-two stock files.</param>
    /// <exception cref="InvalidDataException">Stock validation fails.</exception>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Rejects visual documents whose schema, room identities, or metatile operands are invalid.</summary>
    /// <param name="document">Parsed reveal rules and room overlays to validate.</param>
    /// <param name="path">Source path included in validation errors.</param>
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
                room.Tiles.Any(tile => tile is null || tile.Word > 0x0fff))
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

    /// <summary>Names one native copy command in the installed visual JSON format.</summary>
    /// <remarks>
    /// Issue #1040: the pinned NTSC J/U v1.0 ROM command bodies at
    /// $91:CF36, CF3E, CF4E, CF62, and CF6F copy one, Brinstar-only one,
    /// wide, tall, and square metatile operands respectively. Those five
    /// identities map to the authored JSON labels below; the labels are
    /// an installation format contract, not cartridge bytes. The two
    /// extension commands have no visual shape and every other ushort
    /// fails. The independent 16-by-256 native reveal oracle covers all
    /// reachable command identities; the room installation verifier
    /// accepts stock shapes and rejects an override changing a shape.
    /// Retain this finite named mapping because pointer arithmetic cannot
    /// derive the serialization labels more clearly or losslessly.
    /// </remarks>
    private static string NameOf(ushort command) => command switch
    {
        XrayRevealCodePointers.CopyOne => "one",
        XrayRevealCodePointers.CopyWide => "wide",
        XrayRevealCodePointers.CopyTall => "tall",
        XrayRevealCodePointers.CopySquare => "square",
        XrayRevealCodePointers.CopyBrinstar => "brinstar-only",
        _ => throw new InvalidDataException($"X-ray command $91:{command:X4} has no visual shape."),
    };

    /// <summary>Reads the retail reveal-table result for one collision type and BTS value.</summary>
    /// <param name="bus">Cartridge address space containing the bank-$91 tables and command operands.</param>
    /// <param name="type">Collision type used to select a reveal-table group.</param>
    /// <param name="bts">Block-type-specific value matched within that group.</param>
    /// <returns>The native copy command and its metatile words, or <see langword="null"/> when no rule matches.</returns>
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

    /// <summary>Reads a little-endian word from the bank-$91 reveal table.</summary>
    /// <param name="bus">Cartridge address space supplying the bytes.</param>
    /// <param name="pointer">Bank-local address of the low byte.</param>
    /// <returns>The decoded 16-bit value.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, int pointer)
    {
        if (pointer < 0x8000 || pointer >= ushort.MaxValue)
            throw new InvalidDataException($"X-ray reveal table crossed bank $91 at ${pointer:X}.");
        return unchecked((ushort)(bus.ReadCartridgeByte(XrayRevealCodePointers.Bank | pointer) |
            bus.ReadCartridgeByte(XrayRevealCodePointers.Bank | (pointer + 1)) << 8));
    }

    /// <summary>Reads one terminated special-room X-ray overlay list.</summary>
    /// <param name="bus">Cartridge address space containing room-bank overlay records.</param>
    /// <param name="pointer">Bank-local address of the first coordinate/word pair.</param>
    /// <returns>The room pointer and its visual tile entries.</returns>
    private static XrayRoomOverlayEntry ReadRoomOverlay(ISnesAddressSpace bus, ushort pointer)
    {
        var tiles = new List<XrayRoomOverlayTileDocument>();
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
            tiles.Add(new XrayRoomOverlayTileDocument((byte)coordinate,
                (byte)(coordinate >> 8), word));
        }
        throw new InvalidDataException($"X-ray room overlay ${pointer:X4} has no terminator.");
    }

    /// <summary>Reads a little-endian word from an absolute cartridge address.</summary>
    /// <param name="bus">Cartridge address space supplying the bytes.</param>
    /// <param name="address">Address of the low byte.</param>
    /// <returns>The decoded 16-bit value.</returns>
    private static ushort ReadAbsoluteWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadCartridgeByte(address) | bus.ReadCartridgeByte(address + 1) << 8));

    /// <summary>Reads a strict X-ray JSON document from a file.</summary>
    /// <typeparam name="T">The document model to deserialize.</typeparam>
    /// <param name="path">Path to the JSON file.</param>
    /// <returns>The parsed document.</returns>
    private static T ReadJson<T>(string path) where T : class =>
        ReadJson<T>(File.ReadAllBytes(path), path);

    /// <summary>Deserializes strict X-ray JSON from in-memory bytes.</summary>
    /// <typeparam name="T">The document model to deserialize.</typeparam>
    /// <param name="bytes">JSON document contents.</param>
    /// <param name="path">Logical filename used in parse errors.</param>
    /// <returns>The parsed document.</returns>
    private static T ReadJson<T>(byte[] bytes, string path) where T : class
    {
        using var stream = new MemoryStream(bytes, writable: false);
        return JsonAssetDocument.Read<T>(stream, JsonOptions, $"X-ray {path}");
    }

    /// <summary>Stock provenance and integrity metadata for the reveal visual document.</summary>
    /// <param name="Version">Required X-ray JSON schema version.</param>
    /// <param name="SourceCartridgeSha256">SHA-256 of the source cartridge revision.</param>
    /// <param name="VisualSha256">SHA-256 of the stock reveal visual JSON bytes.</param>
    private sealed record XrayRevealVisualManifest(int Version, string SourceCartridgeSha256,
        string VisualSha256);
    /// <summary>Editable metatile operands, rule descriptions, and special-room overlays.</summary>
    /// <param name="Version">Required X-ray JSON schema version.</param>
    /// <param name="Entries">Visual words keyed by the fixed native collision/BTS rule order.</param>
    /// <param name="ItemMetatiles">Eight visual metatile words used for item reveal slots.</param>
    /// <param name="Rooms">Room-specific overlay records keyed by native room pointer.</param>
    private sealed record XrayRevealVisualDocument(int Version, XrayRevealVisualEntry[] Entries,
        ushort[] ItemMetatiles, XrayRoomOverlayEntry[] Rooms);
    /// <summary>One authored visual mapping for a fixed native collision/BTS reveal rule.</summary>
    /// <param name="CollisionType">Native block collision type selected by the rule.</param>
    /// <param name="BtsValues">BTS values that share the same native command and visual shape.</param>
    /// <param name="Shape">Stable JSON label for the native copy-command shape.</param>
    /// <param name="TopLeft">Twelve-bit top-left metatile index.</param>
    /// <param name="TopRight">Twelve-bit top-right metatile index when used by the shape.</param>
    /// <param name="BottomLeft">Twelve-bit bottom-left metatile index when used by the shape.</param>
    /// <param name="BottomRight">Twelve-bit bottom-right metatile index when used by the shape.</param>
    private sealed record XrayRevealVisualEntry(RoomCollisionType CollisionType, int[] BtsValues,
        string Shape, ushort TopLeft, ushort TopRight, ushort BottomLeft, ushort BottomRight);
    /// <summary>Visual tiles associated with one native special-room X-ray list.</summary>
    /// <param name="Pointer">Bank-local native room-overlay pointer.</param>
    /// <param name="Tiles">Visual tiles in the same order as the native list.</param>
    private sealed record XrayRoomOverlayEntry(ushort Pointer, XrayRoomOverlayTileDocument[] Tiles);

    // Keep file admission separate from the runtime value type. A missing coordinate
    // must not become the struct's legal zero/default value during deserialization.
    /// <summary>One serialized room-overlay coordinate and its visual metatile word.</summary>
    /// <param name="X">Unsigned horizontal block coordinate.</param>
    /// <param name="Y">Unsigned vertical block coordinate.</param>
    /// <param name="Word">Twelve-bit metatile index drawn at the coordinate.</param>
    private sealed record XrayRoomOverlayTileDocument(byte X, byte Y, ushort Word);
}
