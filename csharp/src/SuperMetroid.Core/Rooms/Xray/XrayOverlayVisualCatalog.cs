using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Rooms.Xray;

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

    /// <summary>Copies validated item and room overlays, optionally requiring every retail special-room source.</summary>
    internal XrayOverlayVisualCatalog(IEnumerable<ushort> itemMetatiles,
        IEnumerable<(ushort Pointer, IReadOnlyList<XrayRoomOverlayVisual> Tiles)> rooms,
        bool requireCompleteInstallation)
    {
        Ensure.NotNull(itemMetatiles);
        Ensure.NotNull(rooms);
        this.itemMetatiles = itemMetatiles.ToArray();
        if (this.itemMetatiles.Length != XrayOverlayRomData.DynamicGraphicsSlots * 2 ||
            this.itemMetatiles.Any(word => word > 0x0fff))
            throw new InvalidDataException("X-ray item visuals require eight valid metatiles.");
        this.rooms = new Dictionary<ushort, XrayRoomOverlayVisual[]>();
        foreach ((ushort pointer, IReadOnlyList<XrayRoomOverlayVisual> tiles) in rooms)
        {
            if (pointer < 0x8000 || tiles is null || tiles.Count == 0 ||
                tiles.Any(tile => tile.Metatile.Raw > 0x0fff) ||
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
                // Identity hashes the authored byte coordinates.
                content.Append("x", checked((byte)tile.Block.X));
                content.Append("y", checked((byte)tile.Block.Y));
                content.Append("word", tile.Metatile.Raw);
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
