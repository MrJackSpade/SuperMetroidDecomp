using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Resident colored-door state for shot filtering, hit accumulation, and the cartridge's flash and opening lists.</summary>
internal sealed class ColoredDoorPlmState(
    ColoredDoorColor color,
    ColoredDoorOrientation orientation,
    ColoredDoorPrograms programs,
    byte hitThreshold,
    ColoredDoorPhase phase)
{
    /// <summary>Determines which projectile families can advance this door's hit counter.</summary>
    public ColoredDoorColor Color { get; } = color;
    /// <summary>Facing used to install the correct blue-door BTS after an already-open door reloads.</summary>
    public ColoredDoorOrientation Orientation { get; } = orientation;
    /// <summary>Native cursor that receives control after a room-entry closing list falls through.</summary>
    public ushort InitialList { get; } = programs.InitialList;
    /// <summary>Program holding the blue-door behavior byte and draw for an opened door on reload.</summary>
    public ushort ClosedBlueList { get; } = programs.ClosedBlueList;
    /// <summary>Native hit program containing the threshold and nonfatal flash entry.</summary>
    public ushort HitList { get; } = programs.HitList;
    /// <summary>Program selected once the accepted-hit threshold is reached.</summary>
    public ushort OpeningList { get; } = programs.OpeningList;
    /// <summary>Draw command used once to install the closed colored cap.</summary>
    public ushort ColoredClosedDraw { get; } = programs.ColoredClosedDraw;
    /// <summary>Accepted-hit count required before the door's persistent opened bit is set.</summary>
    public byte HitThreshold { get; } = hitThreshold;
    /// <summary>Distinguishes resident waiting from shared flash, opening, reload, and closing work.</summary>
    public ColoredDoorPhase Phase { get; set; } = phase;
    /// <summary>Unsigned accepted-hit count; a red-door super missile seeds $77 before incrementing.</summary>
    public byte HitCounter { get; set; }
    /// <summary>Prevents the closed cap from being drawn twice after setup or room-entry closure.</summary>
    public bool InitialDrawCompleted { get; set; }
    /// <summary>Marks a shot awaiting color-family filtering on the next door step.</summary>
    public bool HasPendingHit { get; set; }
    /// <summary>Preserves the pending shot's family until the door accepts or rejects it.</summary>
    public SamusProjectileTypeWord PendingProjectileType { get; set; }
}
