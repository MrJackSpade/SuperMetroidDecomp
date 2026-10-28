using SuperMetroid.Core.Assets;
using System.Text;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.Core.Samus.Abilities.Artwork;

/// <summary>Installed item and room-specific X-ray presentation, independent of PLM state.</summary>
public sealed class InstalledXrayOverlays
{
    /// <summary>Item graphics slots that rotate with the room's items; the same number of fixed ammo and tank slots follow.</summary>
    public const int DynamicGraphicsSlots = 4;

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
    /// <summary>Installed room-specific X-ray tile lists keyed by overlay.</summary>
    /// <remarks>
    /// Issue #1039: among 323 pinned NTSC J/U v1.0 room states, only
    /// Bomb Torizo selects special-X-ray pointer $8F:986B. Its three
    /// four-byte records are (X=$0F,Y=$0A..$0C,word=$0052), followed
    /// by zero coordinates at $8F:9877. The native loop stops there;
    /// adjacent bytes are not a fourth record. The independent installation
    /// verifier compared all three stock records with the cartridge.
    /// reveals.json may change coordinates and words while retaining the
    /// overlay and record count, so the apparent Y sequence is not an
    /// algorithm for all valid installations. Retain the bounded lists
    /// and reject missing or duplicate overlays.
    /// </remarks>
    private readonly Dictionary<XrayRoomOverlayDefinition, XrayRoomOverlayVisual[]> rooms;

    /// <summary>Copies item-slot metatiles and ordered special-room records, requiring all compiled overlay sources without changing item collection or PLM state.</summary>
    /// <param name="itemMetatiles">Exactly eight twelve-bit metatile references in native graphics-slot order.</param>
    /// <param name="rooms">Nonempty ordered tile lists keyed by overlay; each list is copied.</param>
    /// <exception cref="ArgumentNullException">An input sequence or overlay is null.</exception>
    /// <exception cref="InvalidDataException">Item values/count, room lists, duplicate overlays, or required-overlay coverage are invalid.</exception>
    public InstalledXrayOverlays(IEnumerable<ushort> itemMetatiles,
        IEnumerable<XrayRoomOverlayTiles> rooms)
        : this(itemMetatiles, rooms, requiredOverlays: XrayRoomOverlayDefinitions.All)
    {
    }

    /// <summary>Copies validated item and room overlays and requires each of <paramref name="requiredOverlays"/>.</summary>
    internal InstalledXrayOverlays(IEnumerable<ushort> itemMetatiles,
        IEnumerable<XrayRoomOverlayTiles> rooms,
        IEnumerable<XrayRoomOverlayDefinition> requiredOverlays)
    {
        Ensure.NotNull(itemMetatiles);
        Ensure.NotNull(rooms);
        this.itemMetatiles = itemMetatiles.ToArray();
        if (this.itemMetatiles.Length != DynamicGraphicsSlots * 2 ||
            this.itemMetatiles.Any(word => !RoomLevelWord.IsValidVisualWord(word)))
            throw new InvalidDataException("X-ray item visuals require eight valid metatiles.");
        this.rooms = new Dictionary<XrayRoomOverlayDefinition, XrayRoomOverlayVisual[]>();
        foreach ((XrayRoomOverlayDefinition overlay, IReadOnlyList<XrayRoomOverlayVisual> tiles) in rooms)
        {
            Ensure.NotNull(overlay);
            if (tiles is null || tiles.Count == 0 ||
                tiles.Any(tile => !RoomLevelWord.IsValidVisualWord(tile.Metatile.Raw)) ||
                !this.rooms.TryAdd(overlay, tiles.ToArray()))
                throw new InvalidDataException($"Invalid X-ray room overlay {overlay.Name}.");
        }
        foreach (XrayRoomOverlayDefinition overlay in Ensure.NotNull(requiredOverlays))
            if (!this.rooms.ContainsKey(overlay))
                throw new InvalidDataException($"Missing required X-ray room overlay {overlay.Name}.");
    }

    /// <summary>Selects the installed item reveal metatile by native graphics-slot index, independently of whether an item has been collected.</summary>
    /// <param name="graphicsSlot">Zero-based slot 0..7: four dynamic item graphics followed by the four fixed collectible graphics.</param>
    /// <returns>The selected twelve-bit visual metatile reference.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The slot is outside the installed eight-entry array.</exception>
    public ushort ItemMetatile(int graphicsSlot) => itemMetatiles[graphicsSlot];

    /// <summary>Identity of selected item metatiles and room overlay coordinates/tiles, in native record order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledXrayOverlays), content =>
    {
        content.AppendWords("item metatiles", itemMetatiles);
        foreach ((XrayRoomOverlayDefinition overlay, XrayRoomOverlayVisual[] tiles) in rooms.OrderBy(pair => pair.Key.Name, StringComparer.Ordinal))
        {
            content.Append("room overlay", Encoding.UTF8.GetBytes(overlay.Name));
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

    /// <summary>Returns a room overlay's installed records in native draw order.</summary>
    /// <param name="overlay">The active room state's overlay; a state without one is handled by the caller.</param>
    /// <returns>The shared copied record list; coordinates are room-block positions and words are visual metatile references.</returns>
    /// <exception cref="InvalidDataException">No installed list exists for the overlay.</exception>
    public IReadOnlyList<XrayRoomOverlayVisual> RoomTiles(XrayRoomOverlayDefinition overlay) =>
        rooms.TryGetValue(Ensure.NotNull(overlay), out XrayRoomOverlayVisual[]? tiles) ? tiles :
        throw new InvalidDataException($"Missing installed X-ray room overlay {overlay.Name}.");
}
