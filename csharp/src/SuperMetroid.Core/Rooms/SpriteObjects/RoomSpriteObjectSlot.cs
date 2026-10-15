namespace SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>
/// One of the 32 physical bank-$B4 sprite-object slots. The retail pool is shared by enemy
/// bodies, attack afterimages, explosions, and room effects; modeling it once preserves its
/// descending allocation order and finite-capacity behavior for every translated family.
/// </summary>
public sealed class RoomSpriteObjectSlot
{
    /// <summary>Creates a slot at its fixed position in the 32-entry bank-$B4 pool.</summary>
    internal RoomSpriteObjectSlot(int slotIndex) => SlotIndex = slotIndex;

    /// <summary>Zero-based physical pool slot, zero through 31; allocation and update/draw traversal search from the highest slot downward.</summary>
    public int SlotIndex { get; }
    /// <summary>Native word-array byte offset, twice <see cref="SlotIndex"/>, ranging from $00 through $3E.</summary>
    public ushort NativeIndex => unchecked((ushort)(SlotIndex * 2));
    /// <summary>Selector retained from allocation for owner/debugger identity; clearing assigns <see cref="RoomSpriteObjectKind.None"/>.</summary>
    public RoomSpriteObjectKind Kind { get; internal set; } = RoomSpriteObjectKind.None;
    /// <summary>Whether the native instruction-list pointer is nonzero; this alone determines occupancy, even for hidden or update-disabled objects.</summary>
    public bool IsActive => InstructionPointer != 0;
    /// <summary>
    /// Native $7E:F0F8/$7E:F1F8 whole words and their fractional words (Y fraction at $7E:F278)
    /// plus the slot offset: the 16.16 room position, converted to screen coordinates by
    /// subtracting the layer-one camera when drawn. Most sprite objects move only in whole pixels
    /// and leave the fractions zero; Draygon's six burial Evirs consume the exact ROM subspeed
    /// table, so discarding these words visibly changes their fan-in.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }
    /// <summary>Native $7E:F078 plus the slot offset: packed OBJ attribute base; drawing extracts palette bits $0E00 and base tile number $01FF.</summary>
    public ushort GraphicsIndex { get; internal set; }
    /// <summary>Native $7E:EF78 plus the slot offset: bank-$B4 program cursor; timed records occupy four bytes, and zero marks a free slot.</summary>
    public ushort InstructionPointer { get; internal set; }
    /// <summary>Native $7E:EFF8 plus the slot offset: frame-duration countdown or high-bit-set opcode address; repeat-last installs $7FFF while preserving the visible frame.</summary>
    public ushort InstructionTimer { get; internal set; }
    /// <summary>Bank-$B4 visual-frame pointer resolved from the current timed record's second word; drawing combines its tile offsets with <see cref="GraphicsIndex"/>.</summary>
    public ushort SpritemapPointer { get; internal set; }
    /// <summary>Native $7E:F2F8 plus the slot offset: bit zero suppresses instruction updates but leaves active-object drawing and slot occupancy intact.</summary>
    public ushort DisableFlags { get; internal set; }

    /// <summary>Releases occupancy and resets every retained native word before the slot can be reused.</summary>
    internal void Clear()
    {
        Kind = RoomSpriteObjectKind.None;
        Position = FixedPosition<RoomPixels>.Zero;
        GraphicsIndex = 0;
        InstructionPointer = InstructionTimer = SpritemapPointer = DisableFlags = 0;
    }
}
