using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Assets;

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

    /// <summary>Selector retained from allocation for owner/debugger identity; clearing assigns <see cref="RoomSpriteObjectKind.None"/>.</summary>
    public RoomSpriteObjectKind Kind { get; internal set; } = RoomSpriteObjectKind.None;

    /// <summary>Whether the slot runs a program; this alone determines occupancy, even for hidden or update-disabled objects.</summary>
    public bool IsActive => Cursor is not null;

    /// <summary>
    /// Native $7E:F0F8/$7E:F1F8 whole words and their fractional words (Y fraction at $7E:F278)
    /// plus the slot offset: the 16.16 room position, converted to screen coordinates by
    /// subtracting the layer-one camera when drawn. Most sprite objects move only in whole pixels
    /// and leave the fractions zero; Draygon's six burial Evirs consume the exact ROM subspeed
    /// table, so discarding these words visibly changes their fan-in.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>Native $7E:F078 plus the slot offset: the sheet characters and colours the object draws with.</summary>
    public SpriteGraphics Graphics { get; internal set; } = SpriteGraphics.Emptied;

    /// <summary>Where the object's program resumes, or null for a free slot.</summary>
    internal AnimationProgramCursor<RoomSpriteObjectProgram>? Cursor { get; set; }

    /// <summary>Native $7E:EFF8 plus the slot offset: updates left showing <see cref="Frame"/>; a held frame reloads $7FFF.</summary>
    public WrappingCountdown Timer { get; internal set; }

    /// <summary>The frame the object shows.</summary>
    internal RoomSpriteObjectFrame Frame { get; set; }

    /// <summary>Native $7E:F2F8 bit zero plus the slot offset: suppresses program updates but leaves drawing and occupancy intact.</summary>
    public bool UpdatesDisabled { get; internal set; }

    /// <summary>Releases occupancy and resets every retained field before the slot can be reused.</summary>
    internal void Clear()
    {
        Kind = RoomSpriteObjectKind.None;
        Position = FixedPosition<RoomPixels>.Zero;
        Graphics = SpriteGraphics.Emptied;
        Cursor = null;
        Timer = default;
        Frame = default;
        UpdatesDisabled = false;
    }
}
