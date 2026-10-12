using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

internal sealed class GreyDoorPlmState(
    ColoredDoorOrientation orientation,
    GreyDoorCondition condition,
    GreyDoorPrograms programs,
    GreyDoorPhase phase)
{
    public ColoredDoorOrientation Orientation { get; } = orientation;
    public GreyDoorCondition Condition { get; } = condition;
    public ushort InitialList { get; } = programs.InitialList;
    public ushort ClosedBlueList { get; } = programs.ClosedBlueList;
    public ushort ClosedGreyDraw { get; } = programs.ClosedGreyDraw;
    public ushort FlashList { get; } = programs.FlashList;
    public ushort OpeningList { get; } = programs.OpeningList;
    public GreyDoorPhase Phase { get; set; } = phase;
    public bool InitialDrawCompleted { get; set; }
    public bool HasPendingHit { get; set; }
    public SamusProjectileTypeWord PendingProjectileType { get; set; }
}
