using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>Represents grey door plm state.</summary>
internal sealed class GreyDoorPlmState(
    ColoredDoorOrientation orientation,
    GreyDoorCondition condition,
    GreyDoorPrograms programs,
    GreyDoorPhase phase)
{
    /// <summary>Gets orientation.</summary>
    public ColoredDoorOrientation Orientation { get; } = orientation;
    /// <summary>Gets condition.</summary>
    public GreyDoorCondition Condition { get; } = condition;
    /// <summary>Gets initial list.</summary>
    public ushort InitialList { get; } = programs.InitialList;
    /// <summary>Gets closed blue list.</summary>
    public ushort ClosedBlueList { get; } = programs.ClosedBlueList;
    /// <summary>Gets closed grey draw.</summary>
    public ushort ClosedGreyDraw { get; } = programs.ClosedGreyDraw;
    /// <summary>Gets flash list.</summary>
    public ushort FlashList { get; } = programs.FlashList;
    /// <summary>Gets opening list.</summary>
    public ushort OpeningList { get; } = programs.OpeningList;
    /// <summary>Gets or sets phase.</summary>
    public GreyDoorPhase Phase { get; set; } = phase;
    /// <summary>Gets or sets initial draw completed.</summary>
    public bool InitialDrawCompleted { get; set; }
    /// <summary>Gets or sets has pending hit.</summary>
    public bool HasPendingHit { get; set; }
    /// <summary>Gets or sets pending projectile type.</summary>
    public SamusProjectileTypeWord PendingProjectileType { get; set; }
}
