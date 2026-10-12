using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>Represents colored door plm state.</summary>
internal sealed class ColoredDoorPlmState(
    ColoredDoorColor color,
    ColoredDoorOrientation orientation,
    ColoredDoorPrograms programs,
    byte hitThreshold,
    ColoredDoorPhase phase)
{
    /// <summary>Gets color.</summary>
    public ColoredDoorColor Color { get; } = color;
    /// <summary>Gets orientation.</summary>
    public ColoredDoorOrientation Orientation { get; } = orientation;
    /// <summary>Gets initial list.</summary>
    public ushort InitialList { get; } = programs.InitialList;
    /// <summary>Gets closed blue list.</summary>
    public ushort ClosedBlueList { get; } = programs.ClosedBlueList;
    /// <summary>Gets hit list.</summary>
    public ushort HitList { get; } = programs.HitList;
    /// <summary>Gets opening list.</summary>
    public ushort OpeningList { get; } = programs.OpeningList;
    /// <summary>Gets colored closed draw.</summary>
    public ushort ColoredClosedDraw { get; } = programs.ColoredClosedDraw;
    /// <summary>Gets hit threshold.</summary>
    public byte HitThreshold { get; } = hitThreshold;
    /// <summary>Gets or sets phase.</summary>
    public ColoredDoorPhase Phase { get; set; } = phase;
    /// <summary>Gets or sets hit counter.</summary>
    public byte HitCounter { get; set; }
    /// <summary>Gets or sets initial draw completed.</summary>
    public bool InitialDrawCompleted { get; set; }
    /// <summary>Gets or sets has pending hit.</summary>
    public bool HasPendingHit { get; set; }
    /// <summary>Gets or sets pending projectile type.</summary>
    public SamusProjectileTypeWord PendingProjectileType { get; set; }
}
