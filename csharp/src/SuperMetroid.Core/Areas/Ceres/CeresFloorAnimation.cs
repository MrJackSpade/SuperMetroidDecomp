using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The seven Ceres floor animation frames (<see cref="CeresFloorAnimationDefinitions"/>) as pictures drawn over a Ceres
/// room's turning floor (#1467): the elevator platform's strip for its light or dark frame; the capsule and the wings
/// walk their cycles as the getaway's turning floor shows them (#2329). The import pictures each frame, and the floor's
/// sheet draws it by the frame (<see cref="RoomFloorSheet"/>).
/// </summary>
public static class CeresFloorAnimation
{
    /// <summary>The elevator platform's strip at phase <paramref name="phase"/> of its two-phase cycle.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is outside the cycle.</exception>
    internal static CeresFloorAnimationFrame ElevatorPlatformFrameForPhase(int phase) => CycleFrame(CeresFloorAnimationDefinitions.ElevatorPlatformCycle, phase);

    /// <summary>The frame at <paramref name="phase"/> of <paramref name="cycle"/>.</summary>
    private static CeresFloorAnimationFrame CycleFrame(CeresFloorAnimationFrame[] cycle, int phase) =>
        (uint)phase < (uint)cycle.Length ? cycle[phase] : throw new ArgumentOutOfRangeException(nameof(phase));
}
