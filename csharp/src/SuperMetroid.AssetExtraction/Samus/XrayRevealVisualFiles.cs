using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Rooms.RoomObjects;
using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Extracts the blocks the retail X-ray reveal table shows. Rule selection,
/// copy shape, extensions, and area gating stay in the compiled collision dispatcher.
/// </summary>
[EncodingOwner("retail X-ray reveal table words")]
public static class XrayRevealVisualFiles
{
    /// <summary>Family-relative version-six installed JSON filename for reveal-rule operands, the extension exit block, four tank metatiles, and special-room overlay records.</summary>
    public const string VisualFileName = "reveals.json";
    /// <summary>
    /// Schema revision that includes rule operands, tank metatiles, and room-overlay records: revision 3 holds the four tanks'
    /// reveal blocks alone, every other item being revealed as its kind's own picture (#1538); revision 4 holds the block an
    /// extension chain leaving the room backward shows, which the game kept a compiled copy of (#1615); revision 5 names each
    /// room overlay by its overlay instead of the cartridge pointer of its list (#1623); revision 6 names the reactions each
    /// rule covers by their list and entry instead of the game's behaviour bytes (#1709).
    /// </summary>
    private const int FormatVersion = 6;

    /// <summary>
    /// The block the cartridge's extension traversal writes when a chain's next step leaves the room backward, at
    /// $91:CE95/$91:CED7: the tileset's last block.
    /// </summary>
    private const int ExtensionExitBlock = 255;

    /// <summary>Uses camel-case enum names and strict constructor/property admission for the installed reveal JSON.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>
    /// Checks all 16-by-256 cartridge collision-kind and behaviour reveal results and that the X-ray reveals each item that
    /// shows its own picture by its first frame, then exports drawable blocks, tank appearances, and room-specific X-ray
    /// overlays.
    /// </summary>
    /// <param name="bus">Cartridge source for bank-$91 reveal records, eight bank-$84 item draw pointers at $84:839D, and bank-$8F special-X-ray lists referenced by compiled room states.</param>
    /// <param name="directory">Family directory, created before native validation; receives a new <see cref="VisualFileName"/> file and no manifest (#1693), the document naming its own format and the installation receipt the cartridge.</param>
    /// <remarks>Drawable rules are grouped by collision kind and definition. Item and room-overlay level words are decoded to appearances by <see cref="RoomLevelWordCodec"/>; room coordinates are unsigned 16-pixel block positions. The create-new write refuses an existing output.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">A native rule's command differs from the compiled commands, a pointer/command is invalid, the X-ray reveals an item showing its own picture other than by its block pair's first frame, or a room overlay is empty, unterminated, or contains nonvisual words.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory)
    {
        Ensure.NotNull(bus);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);

        var groups = new Dictionary<XrayRevealGroupKey,
            List<BlockReaction>>();
        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int value = 0; value <= byte.MaxValue; value++)
        {
            XrayRevealDefinition? native = ReadNative(bus, type, value);
            RoomBlockBehavior behavior = RoomBlockBehaviorCodec.Decode(value);
            if (native?.Shape != XrayRevealTable.Find(type, behavior))
                throw new InvalidDataException(
                    $"Compiled X-ray command {type}/behaviour ${value:X2} differs from the cartridge.");
            if (native is not { } definition || !InstalledXrayReveals.IsDrawable(definition.Shape))
                continue;
            var key = new XrayRevealGroupKey(type, definition);
            if (!groups.TryGetValue(key, out List<BlockReaction>? reactions))
                groups.Add(key, reactions = []);
            reactions.Add(behavior.Reaction);
        }

        XrayRevealVisualEntry[] entries = groups
            .Select(pair => new XrayRevealVisualEntry(
                pair.Key.Type, [.. pair.Value.Select(reaction => new XrayRevealReactionDocument(reaction.Scope, reaction.Entry))],
                NameOf(pair.Key.Definition.Shape),
                pair.Key.Definition.TopLeft, pair.Key.Definition.TopRight,
                pair.Key.Definition.BottomLeft, pair.Key.Definition.BottomRight))
            .OrderBy(entry => entry.CollisionType)
            .ThenBy(entry => entry.Reactions[0].Scope)
            .ThenBy(entry => entry.Reactions[0].Entry)
            .ToArray();
        // The first draws reveal the item set up in each block pair as that pair's first frame, which shows the item's own
        // picture; the port reveals such an item as its kind's first frame (#1538). The tanks' draws follow, in tank order.
        for (int draw = 0; draw < CollectibleBlockPairs.InTurn.Count; draw++)
        {
            CollectibleBlockPairs.Pair pair = CollectibleBlockPairs.InTurn[draw];
            RoomBlockAppearance firstFrame = CollectibleObjectDraws.TryGetBlockPairTerrain(CollectibleObjectDraws.VisibleFrame(InWorldCollectibleKind.Bombs, 0, pair),
                out RoomTerrainBlock written) ? written.Appearance
                : throw new InvalidOperationException($"Block pair {pair}'s first frame writes no block pair's terrain.");
            if (ItemDrawAppearance(bus, draw) != firstFrame)
                throw new InvalidDataException($"The X-ray reveals the item in block pair {pair} other than by the pair's first frame.");
        }
        RoomBlockAppearance[] tankMetatiles = Enumerable.Range(CollectibleBlockPairs.InTurn.Count, InstalledXrayOverlays.TankCount)
            .Select(index => ItemDrawAppearance(bus, index)).ToArray();
        XrayRoomOverlayEntry[] rooms = XrayRoomOverlayDefinitions.All
            .Select(overlay => ReadRoomOverlay(bus, overlay)).ToArray();
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(
            new XrayRevealVisualDocument(FormatVersion, entries, ExtensionExitBlock, tankMetatiles, rooms),
            JsonOptions);
        using var file = new FileStream(Path.Combine(directory, VisualFileName), FileMode.CreateNew, FileAccess.Write);
        file.Write(json);
    }

    /// <summary>Validates the installed version-six X-ray presentation as the player left it, retaining native rule selection, copy geometry, extensions, and area gating.</summary>
    /// <param name="directory">Family directory containing the installed reveal JSON.</param>
    /// <returns>A ROM-independent catalog with copied rule operands, the extension exit block, four tank reveals, and ordered special-room tile lists.</returns>
    /// <remarks>Loading does not repeat cartridge reads. Strict JSON rejects unknown/missing constructor properties. Every entry must name a compiled drawable rule with its shape, every drawable rule must be covered once, and the special rooms keep their identities; blocks, appearances, tile lists and byte-range room coordinates may change.</remarks>
    /// <exception cref="ArgumentException"><paramref name="directory"/> is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">The format, strict JSON schema, rule coverage/shape, unused operands, or overlay identities are invalid.</exception>
    /// <exception cref="IOException">The installed reveal JSON cannot be read.</exception>
    public static InstalledXrayReveals Load(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string path = Path.Combine(directory, VisualFileName);
        XrayRevealVisualDocument document = ReadJson<XrayRevealVisualDocument>(path);
        ValidateStructure(document, path);

        var mappings = new List<XrayRevealVisualDefinition>();
        foreach (XrayRevealVisualEntry entry in document.Entries)
        {
            var visual = new XrayRevealVisualBlocks(entry.TopLeft, entry.TopRight, entry.BottomLeft, entry.BottomRight);
            foreach (XrayRevealReactionDocument reaction in entry.Reactions)
                mappings.Add(new XrayRevealVisualDefinition(entry.CollisionType, reaction.Reaction, visual));
        }
        var overlays = new InstalledXrayOverlays(document.TankMetatiles,
            document.Rooms.Select(room => new XrayRoomOverlayTiles(OverlayNamed(room.Overlay),
                room.Tiles.Select(tile =>
                    new XrayRoomOverlayVisual(tile.Block, RoomBlockAppearance.Of(tile.Appearance.Block, tile.Appearance.Mirroring))).ToArray())));
        return new InstalledXrayReveals(mappings, overlays, document.ExtensionExitBlock);
    }

    /// <summary>Checks exact room identities, drawable rule shapes, reaction groups, and unused operand zeros.</summary>
    private static void ValidateStructure(XrayRevealVisualDocument document, string path)
    {
        if (document.Version != FormatVersion || document.Entries is null ||
            document.TankMetatiles is null || document.Rooms is null ||
            document.TankMetatiles.Length != InstalledXrayOverlays.TankCount ||
            document.Rooms.Any(room => room is null))
            throw new InvalidDataException($"X-ray visuals {path} have an incompatible format.");
        if (!document.Rooms.Select(room => room.Overlay).SequenceEqual(XrayRoomOverlayDefinitions.All.Select(overlay => overlay.Name), StringComparer.Ordinal))
            throw new InvalidDataException($"X-ray visuals {path} changed room-overlay identities.");
        foreach (XrayRoomOverlayEntry room in document.Rooms)
        {
            if (room.Tiles is null || room.Tiles.Length == 0 ||
                room.Tiles.Any(tile => tile is null))
                throw new InvalidDataException(
                    $"X-ray visuals {path} contain an invalid room overlay {room.Overlay}.");
        }
        foreach (XrayRevealVisualEntry entry in document.Entries)
        {
            if (entry is null || entry.Reactions is null || entry.Reactions.Length == 0 ||
                entry.Reactions.Any(reaction => reaction is null || !Enum.IsDefined(reaction.Scope) ||
                    reaction.Entry is < 0 or > BlockReaction.LastEntry))
                throw new InvalidDataException($"X-ray visuals {path} contain an invalid reaction group.");
            if (XrayRevealTable.Find(entry.CollisionType, RoomBlockBehavior.Reacting(entry.Reactions[0].Reaction)) is not { } command ||
                !InstalledXrayReveals.IsDrawable(command) || entry.Shape != NameOf(command))
                throw new InvalidDataException($"X-ray visuals {path} contain an invalid copy shape.");
            if ((command is XrayRevealShape.CopyOne or XrayRevealShape.CopyBrinstar &&
                    (entry.TopRight != 0 || entry.BottomLeft != 0 || entry.BottomRight != 0)) ||
                (command == XrayRevealShape.CopyWide &&
                    (entry.BottomLeft != 0 || entry.BottomRight != 0)) ||
                (command == XrayRevealShape.CopyTall &&
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
    /// accepts stock shapes and rejects an edit changing a shape.
    /// Retain this finite named mapping because pointer arithmetic cannot
    /// derive the serialization labels more clearly or losslessly.
    /// </remarks>
    private static string NameOf(XrayRevealShape command) => command switch
    {
        XrayRevealShape.CopyOne => "one",
        XrayRevealShape.CopyWide => "wide",
        XrayRevealShape.CopyTall => "tall",
        XrayRevealShape.CopySquare => "square",
        XrayRevealShape.CopyBrinstar => "brinstar-only",
        XrayRevealShape.HorizontalExtension or XrayRevealShape.VerticalExtension =>
            throw new InvalidDataException($"X-ray command {command} has no visual shape."),
        _ => throw new InvalidOperationException($"Undefined {nameof(XrayRevealShape)} {command}."),
    };

    /// <summary>Walks the bank-$91 collision-kind and behaviour dispatch tables and decodes only the operands owned by the selected copy shape.</summary>
    /// <remarks>A collision-kind table key is a level word holding only the kind's collision bits, decoded by <see cref="RoomLevelWordCodec"/>.</remarks>
    private static XrayRevealDefinition? ReadNative(ISnesAddressSpace bus,
        RoomCollisionType type, int bts)
    {
        for (int entry = SamusSourceDefinitions.XrayRevealBlockTypeTable; ; entry += 4)
        {
            int key = ReadWord(bus, entry);
            if ((RevealTableMarker)key == RevealTableMarker.TableEnd) return null;
            if (RoomLevelWordCodec.Decode(checked((ushort)key)).Collision != type) continue;
            for (int match = ReadWord(bus, entry + 2); ; match += 4)
            {
                int value = ReadWord(bus, match);
                if ((RevealTableMarker)value == RevealTableMarker.TableEnd) return null;
                if ((RevealTableMarker)value != RevealTableMarker.AnyBts && value != bts) continue;
                int pointer = ReadWord(bus, match + 2);
                XrayRevealShape command = SamusIdentityWords.XrayRevealShapes.Decode(
                    ReadWord(bus, pointer), "native X-ray reveal command");
                return command switch
                {
                    XrayRevealShape.HorizontalExtension or
                        XrayRevealShape.VerticalExtension => new(command, 0, 0, 0, 0),
                    XrayRevealShape.CopyOne or XrayRevealShape.CopyBrinstar =>
                        new(command, ReadWord(bus, pointer + 2), 0, 0, 0),
                    XrayRevealShape.CopyWide =>
                        new(command, ReadWord(bus, pointer + 2), ReadWord(bus, pointer + 4), 0, 0),
                    XrayRevealShape.CopyTall =>
                        new(command, ReadWord(bus, pointer + 2), 0, ReadWord(bus, pointer + 4), 0),
                    XrayRevealShape.CopySquare =>
                        new(command, ReadWord(bus, pointer + 2), ReadWord(bus, pointer + 4),
                            ReadWord(bus, pointer + 6), ReadWord(bus, pointer + 8)),
                    _ => throw new InvalidOperationException($"Undefined {nameof(XrayRevealShape)} {command}."),
                };
            }
        }
    }

    /// <summary>The block item draw <paramref name="index"/> of the X-ray's item draw table reveals.</summary>
    /// <exception cref="InvalidDataException">The draw pointer is invalid.</exception>
    private static RoomBlockAppearance ItemDrawAppearance(ISnesAddressSpace bus, int index)
    {
        int pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.ItemDrawPointers.Value + index * sizeof(ushort)));
        if (pointer is < SnesBusLayout.RomWindowStart or > ushort.MaxValue - 3)
            throw new InvalidDataException($"X-ray item draw pointer ${pointer:X4} is invalid.");
        return RoomLevelWordCodec.DecodeAppearance(CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.XrayItemDrawBank.Value | (pointer + 2))));
    }

    /// <summary>Reads one fixed-bank $91 word only after rejecting addresses outside the native table bank.</summary>
    private static ushort ReadWord(ISnesAddressSpace bus, int pointer)
    {
        if (pointer is < SnesBusLayout.RomWindowStart or >= ushort.MaxValue)
            throw new InvalidDataException($"X-ray reveal table crossed bank $91 at ${pointer:X}.");
        return CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(SamusSourceDefinitions.XrayRevealTableBank.Value | pointer);
    }

    /// <summary>The marker words of the reveal tables, beside the block types and BTS values they hold.</summary>
    private enum RevealTableMarker
    {
        /// <summary>The BTS wildcard: matches any BTS in the selected block family.</summary>
        AnyBts = 0xff00,

        /// <summary>The end marker, closing both the block-type and BTS tables.</summary>
        TableEnd = 0xffff,
    }

    /// <summary>
    /// Reads <paramref name="overlay"/>'s ordered block-coordinate/metatile pairs from its bank-$8F list until the required
    /// zero terminator.
    /// </summary>
    private static XrayRoomOverlayEntry ReadRoomOverlay(ISnesAddressSpace bus, XrayRoomOverlayDefinition overlay)
    {
        int pointer = XrayRoomOverlaySources.PointerOf(overlay);
        var tiles = new List<XrayRoomOverlayTileDocument>();
        for (int cursor = pointer; cursor <= ushort.MaxValue - 3; cursor += 4)
        {
            int coordinate = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.XrayRoomOverlayBank.Value | cursor));
            if (coordinate == 0)
            {
                if (tiles.Count == 0)
                    throw new InvalidDataException($"X-ray room overlay ${pointer:X4} is empty.");
                return new XrayRoomOverlayEntry(overlay.Name, tiles.ToArray());
            }
            int word = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusSourceDefinitions.XrayRoomOverlayBank.Value | (cursor + 2)));
            RoomTerrainBlock decoded = RoomLevelWordCodec.Decode(checked((ushort)word));
            if (decoded.Collision != RoomCollisionType.Air)
                throw new InvalidDataException(
                    $"X-ray room overlay ${pointer:X4} carries collision {decoded.Collision} in ${word:X4}.");
            tiles.Add(new XrayRoomOverlayTileDocument((byte)coordinate,
                LittleEndianBytes.HighByte(coordinate), decoded.Appearance));
        }
        throw new InvalidDataException($"X-ray room overlay ${pointer:X4} has no terminator.");
    }

    /// <summary>The retail overlay an installed room entry names, which <see cref="ValidateStructure"/> has required.</summary>
    private static XrayRoomOverlayDefinition OverlayNamed(string name) =>
        XrayRoomOverlayDefinitions.All.Single(overlay => string.Equals(overlay.Name, name, StringComparison.Ordinal));

    /// <summary>Reads an installed file before applying strict X-ray JSON deserialization.</summary>
    private static T ReadJson<T>(string path) where T : class =>
        ReadJson<T>(File.ReadAllBytes(path), path);

    /// <summary>Deserializes known JSON bytes with strict property and constructor rules and path-aware errors.</summary>
    private static T ReadJson<T>(byte[] bytes, string path) where T : class
    {
        using var stream = new MemoryStream(bytes, writable: false);
        return JsonAssetDocument.Read<T>(stream, JsonOptions, $"X-ray {path}");
    }

    /// <summary>Complete editable rule, extension exit, tank, and room-overlay families for the installed format.</summary>
    /// <param name="Version">The schema revision.</param>
    /// <param name="Entries">Every drawable rule's blocks.</param>
    /// <param name="ExtensionExitBlock">The block an extension chain leaving the room backward shows.</param>
    /// <param name="TankMetatiles">The four tanks' reveal appearances.</param>
    /// <param name="Rooms">The special rooms' overlays.</param>
    private sealed record XrayRevealVisualDocument(int Version, XrayRevealVisualEntry[] Entries, int ExtensionExitBlock,
        RoomBlockAppearance[] TankMetatiles, XrayRoomOverlayEntry[] Rooms);
    /// <summary>One compiled collision-kind and reaction rule's editable blocks with its fixed copy shape.</summary>
    /// <param name="CollisionType">The rule's collision kind.</param>
    /// <param name="Reactions">The reactions the rule covers.</param>
    /// <param name="Shape">The rule's copy shape.</param>
    /// <param name="TopLeft">The top-left block number.</param>
    /// <param name="TopRight">The top-right block number, or zero when the shape has none.</param>
    /// <param name="BottomLeft">The bottom-left block number, or zero when the shape has none.</param>
    /// <param name="BottomRight">The bottom-right block number, or zero when the shape has none.</param>
    private sealed record XrayRevealVisualEntry(RoomCollisionType CollisionType, XrayRevealReactionDocument[] Reactions,
        string Shape, int TopLeft, int TopRight, int BottomLeft, int BottomRight);

    /// <summary>One reaction a rule covers, named by its list and its entry in that list.</summary>
    /// <param name="Scope">The list the reaction is an entry of: <c>Common</c> or <c>AreaSpecific</c>.</param>
    /// <param name="Entry">The reaction's entry in its list, 0 through 127.</param>
    private sealed record XrayRevealReactionDocument(BlockReactionScope Scope, int Entry)
    {
        /// <summary>The reaction, once <see cref="ValidateStructure"/> has admitted the list and entry.</summary>
        [JsonIgnore]
        public BlockReaction Reaction => new(Scope, Entry);
    }
    /// <summary>One room-specific overlay, named by its overlay, and its ordered editable block records.</summary>
    /// <param name="Overlay">The overlay's name in <see cref="XrayRoomOverlayDefinitions"/>.</param>
    /// <param name="Tiles">The overlay's block records, in order.</param>
    private sealed record XrayRoomOverlayEntry(string Overlay, XrayRoomOverlayTileDocument[] Tiles);

    /// <summary>Requires explicit X/Y JSON bytes so omitted coordinates cannot become a legal default block.</summary>
    /// <remarks>Keep file admission separate from the runtime value type. A missing coordinate must not become the
    /// struct's legal zero/default value during deserialization.</remarks>
    [SpatialComponents("External format: reveals.json stores each room-overlay tile's block coordinates as x/y bytes.")]
    private sealed record XrayRoomOverlayTileDocument(int X, int Y, RoomBlockAppearance Appearance)
    {
        /// <summary>The tile's block column, 0 through 255.</summary>
        public int X { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, X);

        /// <summary>The tile's block row, 0 through 255.</summary>
        public int Y { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Y);


        /// <summary>Combines validated external coordinates into the runtime room-block position.</summary>
        [JsonIgnore]
        public Position16<RoomBlocks> Block => new(checked((ushort)X), checked((ushort)Y));
    }
}
