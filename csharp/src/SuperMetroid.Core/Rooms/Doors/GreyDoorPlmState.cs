using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Resident grey-door state spanning room-entry closure, condition polling, flashing, and opening.</summary>
internal sealed class GreyDoorPlmState(
    ColoredDoorOrientation orientation,
    GreyDoorCondition condition,
    GreyDoorPrograms programs,
    GreyDoorPhase phase)
{
    /// <summary>Facing direction used when an already-open door becomes a blue-door block on reload.</summary>
    public ColoredDoorOrientation Orientation { get; } = orientation;
    /// <summary>Boss, statue, event, or enemy-quota gate polled while the door remains locked.</summary>
    public GreyDoorCondition Condition { get; } = condition;
    /// <summary>Instruction-list boundary that restores the resident actor after room-entry closure.</summary>
    public ushort InitialList { get; } = programs.InitialList;
    /// <summary>Closed-blue program read when a previously opened door is restored on room entry.</summary>
    public ushort ClosedBlueList { get; } = programs.ClosedBlueList;
    /// <summary>Draw instruction used once to place the closed grey cap.</summary>
    public ushort ClosedGreyDraw { get; } = programs.ClosedGreyDraw;
    /// <summary>Repeating flash list selected after the unlock condition becomes true.</summary>
    public ushort FlashList { get; } = programs.FlashList;
    /// <summary>Opening list handed to the common interpreter after a later shot.</summary>
    public ushort OpeningList { get; } = programs.OpeningList;
    /// <summary>Tracks which stage owns the resident PLM and whether common instructions should run.</summary>
    public GreyDoorPhase Phase { get; set; } = phase;
    /// <summary>Prevents a second closed-cap draw after initial placement or room-entry closure.</summary>
    public bool InitialDrawCompleted { get; set; }
    /// <summary>A shot waiting for locked-state rejection or unlocked-state opening.</summary>
    public bool HasPendingHit { get; set; }
    /// <summary>Type of the latest pending projectile, cleared when the hit is consumed.</summary>
    public SamusProjectileTypeWord PendingProjectileType { get; set; }
}
