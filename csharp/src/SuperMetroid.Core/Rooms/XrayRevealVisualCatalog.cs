namespace SuperMetroid.Core.Rooms;

/// <summary>Replaceable metatile references for one cartridge-defined X-ray reveal.</summary>
public readonly record struct XrayRevealVisualWords(
    ushort TopLeft,
    ushort TopRight,
    ushort BottomLeft,
    ushort BottomRight);

/// <summary>One authored room-specific X-ray overlay tile and its visual position.</summary>
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
    private readonly Dictionary<ushort, XrayRoomOverlayVisual[]> rooms;

    public XrayOverlayVisualCatalog(IEnumerable<ushort> itemMetatiles,
        IEnumerable<(ushort Pointer, IReadOnlyList<XrayRoomOverlayVisual> Tiles)> rooms)
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
    }

    public ushort ItemMetatile(int graphicsSlot) => itemMetatiles[graphicsSlot];

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

    /// <summary>Substitutes visual operands without altering the compiled native command.</summary>
    public XrayRevealDefinition Apply(RoomCollisionType type, byte bts,
        XrayRevealDefinition native)
    {
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
    public static bool IsDrawable(ushort command) => command is
        XrayRevealCodePointers.CopyOne or XrayRevealCodePointers.CopyWide or
        XrayRevealCodePointers.CopyTall or XrayRevealCodePointers.CopySquare or
        XrayRevealCodePointers.CopyBrinstar;
}
