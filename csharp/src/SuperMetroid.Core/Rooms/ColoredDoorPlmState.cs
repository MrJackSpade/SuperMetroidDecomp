using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

internal sealed class ColoredDoorPlmState(
    ColoredDoorColor color,
    ColoredDoorOrientation orientation,
    ColoredDoorPrograms programs,
    byte hitThreshold,
    ColoredDoorPhase phase)
{
    public ColoredDoorColor Color { get; } = color;
    public ColoredDoorOrientation Orientation { get; } = orientation;
    public ushort InitialList { get; } = programs.InitialList;
    public ushort ClosedBlueList { get; } = programs.ClosedBlueList;
    public ushort HitList { get; } = programs.HitList;
    public ushort OpeningList { get; } = programs.OpeningList;
    public ushort ColoredClosedDraw { get; } = programs.ColoredClosedDraw;
    public byte HitThreshold { get; } = hitThreshold;
    public ColoredDoorPhase Phase { get; set; } = phase;
    public byte HitCounter { get; set; }
    public bool InitialDrawCompleted { get; set; }
    public bool HasPendingHit { get; set; }
    public SamusProjectileTypeWord PendingProjectileType { get; set; }
}
