using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Replaceable metatile references for one cartridge-defined X-ray reveal.</summary>
/// <param name="TopLeft">First 16x16 visual metatile operand, used by every drawable reveal.</param>
/// <param name="TopRight">Adjacent horizontal metatile operand for wide and square copies.</param>
/// <param name="BottomLeft">Adjacent vertical metatile operand for tall and square copies.</param>
/// <param name="BottomRight">Diagonal metatile operand for square copies; other command shapes leave it unused.</param>
public readonly record struct XrayRevealVisualWords(
    ushort TopLeft,
    ushort TopRight,
    ushort BottomLeft,
    ushort BottomRight);

/// <summary>One authored room-specific X-ray overlay tile and its visual position.</summary>
/// <param name="X">Horizontal room coordinate in 16-pixel blocks, before subtracting layer-one scroll.</param>
/// <param name="Y">Vertical room coordinate in 16-pixel blocks, before subtracting layer-one scroll.</param>
/// <param name="Word">Twelve-bit visual metatile reference expanded into the X-ray BG tilemap, not a collision/BTS word.</param>
public readonly record struct XrayRoomOverlayVisual(byte X, byte Y, ushort Word);

/// <summary>Installed item and room-specific X-ray presentation, independent of PLM state.</summary>
public sealed class XrayOverlayVisualCatalog
{
    /// <summary>Eight installed X-ray item metatiles indexed by graphics slot.</summary>
    /// <remarks>
    /// Issue #1038: native $84:839D..83AC holds eight little-endian draw
    /// pointers; the visual word is at pointer + 2, masked by $0FFF.
    /// The pinned NTSC J/U v1.0 ROM yields slots 0..7 as $08E, $090,
    /// $092, $094, $04A, $04D, $04F, $050. The overlay caller bounds
    /// checks each selected slot; the independent installation verifier
    /// compared all eight stock values with the cartridge. These are
    /// authored visual choices and reveals.json may replace them, so no
    /// universal formula can replace this bounded installed array.
    /// </remarks>
    private readonly ushort[] itemMetatiles;
    /// <summary>Installed room-specific X-ray tile lists keyed by bank-$8F special-X-ray source pointer.</summary>
    /// <remarks>
    /// Issue #1039: among 323 pinned NTSC J/U v1.0 room states, only
    /// Bomb Torizo selects special-X-ray pointer $8F:986B. Its three
    /// four-byte records are (X=$0F,Y=$0A..$0C,word=$0052), followed
    /// by zero coordinates at $8F:9877. The native loop stops there;
    /// adjacent bytes are not a fourth record. The independent installation
    /// verifier compared all three stock records with the cartridge.
    /// reveals.json may change coordinates and words while retaining the
    /// pointer and record count, so the apparent Y sequence is not an
    /// algorithm for all valid installations. Retain the bounded lists
    /// and reject missing or duplicate pointers.
    /// </remarks>
    private readonly Dictionary<ushort, XrayRoomOverlayVisual[]> rooms;

    /// <summary>Copies item-slot metatiles and ordered special-room records, requiring all compiled overlay sources without changing item collection or PLM state.</summary>
    /// <param name="itemMetatiles">Exactly eight twelve-bit metatile references in native graphics-slot order.</param>
    /// <param name="rooms">Nonempty ordered tile lists keyed by bank-$8F special-X-ray source pointers, not room-state pointers; each list is copied.</param>
    /// <exception cref="ArgumentNullException">An input sequence is null.</exception>
    /// <exception cref="InvalidDataException">Item values/count, room pointers/lists, duplicate sources, or required-source coverage are invalid.</exception>
    public XrayOverlayVisualCatalog(IEnumerable<ushort> itemMetatiles,
        IEnumerable<(ushort Pointer, IReadOnlyList<XrayRoomOverlayVisual> Tiles)> rooms)
        : this(itemMetatiles, rooms, requireCompleteInstallation: true)
    {
    }

    private XrayOverlayVisualCatalog(IEnumerable<ushort> itemMetatiles,
        IEnumerable<(ushort Pointer, IReadOnlyList<XrayRoomOverlayVisual> Tiles)> rooms,
        bool requireCompleteInstallation)
    {
        ArgumentNullException.ThrowIfNull(itemMetatiles);
        ArgumentNullException.ThrowIfNull(rooms);
        this.itemMetatiles = itemMetatiles.ToArray();
        if (this.itemMetatiles.Length != XrayOverlayRomData.DynamicGraphicsSlots * 2 ||
            this.itemMetatiles.Any(word => word > 0x0fff))
            throw new InvalidDataException("X-ray item visuals require eight valid metatiles.");
        this.rooms = new Dictionary<ushort, XrayRoomOverlayVisual[]>();
        foreach ((ushort pointer, IReadOnlyList<XrayRoomOverlayVisual> tiles) in rooms)
        {
            if (pointer < 0x8000 || tiles is null || tiles.Count == 0 ||
                tiles.Any(tile => tile.Word > 0x0fff) ||
                !this.rooms.TryAdd(pointer, tiles.ToArray()))
                throw new InvalidDataException($"Invalid X-ray room overlay ${pointer:X4}.");
        }
        if (requireCompleteInstallation)
            foreach (ushort pointer in XrayRoomOverlaySourceDefinitions.All)
                if (!this.rooms.ContainsKey(pointer))
                    throw new InvalidDataException($"Missing required X-ray room overlay ${pointer:X4}.");
    }

    /// <summary>Selects the installed item reveal metatile by native graphics-slot index, independently of whether an item has been collected.</summary>
    /// <param name="graphicsSlot">Zero-based slot 0..7: four dynamic item graphics followed by the four fixed collectible graphics.</param>
    /// <returns>The selected twelve-bit visual metatile reference.</returns>
    /// <exception cref="IndexOutOfRangeException">The slot is outside the installed eight-entry array.</exception>
    public ushort ItemMetatile(int graphicsSlot) => itemMetatiles[graphicsSlot];

    /// <summary>Identity of selected item metatiles and room overlay coordinates/tiles, in native record order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(XrayOverlayVisualCatalog), content =>
    {
        content.AppendWords("item metatiles", itemMetatiles);
        foreach ((ushort pointer, XrayRoomOverlayVisual[] tiles) in rooms.OrderBy(pair => pair.Key))
        {
            content.Append("room overlay", pointer);
            content.Append("tiles", tiles.Length);
            foreach (XrayRoomOverlayVisual tile in tiles)
            {
                content.Append("x", tile.X);
                content.Append("y", tile.Y);
                content.Append("word", tile.Word);
            }
        }
    });

    /// <summary>Returns a special room's installed overlay records in native draw order without interpreting their source pointer as memory.</summary>
    /// <param name="pointer">Bank-$8F special-X-ray list identity from the active room state; zero/no-overlay is handled by the caller.</param>
    /// <returns>The shared copied record list; coordinates are room-block positions and words are visual metatile references.</returns>
    /// <exception cref="InvalidDataException">No installed special-room list exists for the pointer.</exception>
    public IReadOnlyList<XrayRoomOverlayVisual> RoomTiles(ushort pointer) =>
        rooms.TryGetValue(pointer, out XrayRoomOverlayVisual[]? tiles) ? tiles :
        throw new InvalidDataException($"Missing installed X-ray room overlay ${pointer:X4}.");
}

/// <summary>
/// Installed visual choices for X-ray blocks. The cartridge's collision/BTS lookup,
/// copy dimensions, extension traversal, and Brinstar-only condition remain compiled.
/// </summary>
public sealed class XrayRevealVisualCatalog
{
    /// <summary>Installed visual operands indexed by collision nibble and BTS byte.</summary>
    /// <remarks>
    /// Issue #1037: the exact index is (type &lt;&lt; 8) | bts over 16 native
    /// collision nibbles and 256 unsigned BTS values. Of 4096 possible
    /// slots, the pinned NTSC J/U v1.0 ROM has 305 drawable pairs; the
    /// constructor requires exactly those pairs and rejects duplicates.
    /// Stock operands come from bank-$91 reveal command records and all
    /// 305 installed values match the independent cartridge oracle.
    /// The visual operands can also be replaced through reveals.json while
    /// the native command and copy dimensions remain fixed. Therefore no
    /// single ROM-derived formula can reproduce every valid installation:
    /// retain this bounded indexed store for the selected visual data.
    /// </remarks>
    private readonly XrayRevealVisualWords?[] words = new XrayRevealVisualWords?[16 * 256];

    /// <summary>Creates a complete visual replacement for every drawable native rule.</summary>
    /// <param name="entries">Exactly one set of visual operands for each drawable collision-nibble/BTS pair; native command selection and copy geometry remain fixed.</param>
    /// <param name="overlays">Installed item/special-room visuals, or null for a noninstalled fixture.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry has no drawable rule, duplicates another pair, or leaves native drawable coverage incomplete.</exception>
    public XrayRevealVisualCatalog(IEnumerable<(RoomCollisionType Type, byte Bts,
        XrayRevealVisualWords Visual)> entries, XrayOverlayVisualCatalog? overlays = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Overlays = overlays;
        foreach ((RoomCollisionType type, byte bts, XrayRevealVisualWords visual) in entries)
        {
            XrayRevealDefinition? native = XrayRevealTable.Find(type, bts);
            if (native is not { } definition || !IsDrawable(definition.Command))
                throw new InvalidDataException(
                    $"X-ray visual {type}/BTS ${bts:X2} has no drawable cartridge rule.");
            int index = ((int)type << 8) | bts;
            if (words[index] is not null)
                throw new InvalidDataException(
                    $"X-ray visual {type}/BTS ${bts:X2} appears more than once.");
            words[index] = visual;
        }

        foreach (RoomCollisionType type in Enum.GetValues<RoomCollisionType>())
        for (int bts = 0; bts <= byte.MaxValue; bts++)
        {
            bool drawable = XrayRevealTable.Find(type, unchecked((byte)bts)) is { } definition &&
                IsDrawable(definition.Command);
            if (drawable != (words[((int)type << 8) | bts] is not null))
                throw new InvalidDataException(
                    $"X-ray visual catalog does not cover cartridge rule {type}/BTS ${bts:X2}.");
        }
    }

    /// <summary>Installed item and special-room visuals; null only in noninstalled fixtures.</summary>
    public XrayOverlayVisualCatalog? Overlays { get; }

    /// <summary>Identity of every selected drawable rule and its installed overlays, excluding command mechanics.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(XrayRevealVisualCatalog), content =>
    {
        for (int index = 0; index < words.Length; index++)
            if (words[index] is { } visual)
            {
                content.Append("type and bts", index);
                content.AppendWords("visual operands",
                    [visual.TopLeft, visual.TopRight, visual.BottomLeft, visual.BottomRight]);
            }
        content.Append("has overlays", Overlays is null ? 0 : 1);
        if (Overlays is not null)
            content.Append("overlays", Convert.FromHexString(Overlays.ContentIdentity));
    });

    /// <summary>
    /// Resolves the compiled command and its installed visual operands from the same
    /// collision/BTS identity. Callers cannot pair a drawable command with another
    /// block's absent artwork; unowned pairs retain the native no-reveal result.
    /// </summary>
    public XrayRevealDefinition? Apply(RoomCollisionType type, byte bts)
    {
        if (XrayRevealTable.Find(type, bts) is not { } native) return null;
        if (!IsDrawable(native.Command)) return native;
        XrayRevealVisualWords visual = words[((int)type << 8) | bts] ??
            throw new InvalidDataException($"Missing X-ray visual {type}/BTS ${bts:X2}.");
        return native with
        {
            TopLeft = visual.TopLeft,
            TopRight = visual.TopRight,
            BottomLeft = visual.BottomLeft,
            BottomRight = visual.BottomRight,
        };
    }

    /// <summary>Whether this compiled command carries authored visual metatile operands.</summary>
    /// <remarks>
    /// Issue #1036: the pinned NTSC J/U v1.0 ROM has seven reachable
    /// revealed-block commands. $91:CF36, CF3E, CF4E, CF62, and CF6F
    /// copy one, Brinstar-only one, wide, tall, and square operand tiles;
    /// $91:CE79 and CEBB traverse vertical and horizontal extensions
    /// instead. The exact bounded classification is membership in those
    /// five copy identities, with false for every other ushort. The
    /// independent 16-by-256 native reveal oracle finds all seven commands;
    /// its drawable BTS groups total 305, matching the installed visual
    /// catalog's exhaustive coverage assertion. Named membership is
    /// clearer than a pointer interval, since the two extension routines
    /// precede the copy commands but are not drawable.
    /// </remarks>
    public static bool IsDrawable(XrayRevealCommand command) => command is
        XrayRevealCommand.CopyOne or XrayRevealCommand.CopyWide or
        XrayRevealCommand.CopyTall or XrayRevealCommand.CopySquare or
        XrayRevealCommand.CopyBrinstar;
}
