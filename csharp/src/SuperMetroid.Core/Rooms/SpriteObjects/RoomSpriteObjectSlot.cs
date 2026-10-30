using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>
/// One of the room's sprite-object slots. The pool is shared by enemy bodies, attack afterimages, explosions and room
/// effects; modelling it once keeps the pool's allocation order and finite capacity for every family that uses it.
/// </summary>
public sealed class RoomSpriteObjectSlot
{
    /// <summary>Creates the slot with its ordinal in the pool.</summary>
    internal RoomSpriteObjectSlot(int slotIndex) => SlotIndex = slotIndex;

    /// <summary>The slot's ordinal in the pool, zero first; the pool processes the highest ordinal first.</summary>
    public int SlotIndex { get; }

    /// <summary>Selector retained from allocation for owner/debugger identity; clearing assigns <see cref="RoomSpriteObjectKind.None"/>.</summary>
    public RoomSpriteObjectKind Kind { get; internal set; } = RoomSpriteObjectKind.None;

    /// <summary>Whether the slot runs a program; this alone determines occupancy, even for hidden or update-disabled objects.</summary>
    public bool IsActive => Cursor is not null;

    /// <summary>
    /// The 16.16 room position, converted to screen coordinates by subtracting the layer-one camera when drawn. Most
    /// sprite objects move only in whole pixels and leave the fractions zero; Draygon's six burial Evirs move by
    /// fractional speeds, so discarding the fractions visibly changes their fan-in.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>The sheet characters and colours the object draws with.</summary>
    public SpriteGraphics Graphics { get; internal set; } = SpriteGraphics.Emptied;

    /// <summary>Where the object's program resumes, or null for a free slot.</summary>
    internal AnimationProgramCursor<RoomSpriteObjectProgram>? Cursor { get; set; }

    /// <summary>Updates left showing <see cref="Frame"/>; a held frame reloads the longest countdown.</summary>
    public WrappingCountdown Timer { get; internal set; }

    /// <summary>The frame the object shows.</summary>
    internal RoomSpriteObjectFrame Frame { get; set; }

    /// <summary>Suppresses program updates but leaves drawing and occupancy intact.</summary>
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
