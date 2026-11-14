using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The seven Ceres floor animation frames (<see cref="CeresFloorAnimationDefinitions"/>) as pictures drawn over a Ceres
/// room's turning floor (#1467): the capsule frame for a phase and the elevator platform's strip for its light or dark
/// frame. The import pictures each frame, and the floor's sheet draws it by the frame (<see cref="RoomFloorSheet"/>).
/// </summary>
public static class CeresFloorAnimation
{
    /// <summary>The capsule's frame at phase <paramref name="phase"/> of its four-phase cycle.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is outside the cycle.</exception>
    internal static CeresFloorAnimationFrame BabyFrameForPhase(int phase) => CycleFrame(CeresFloorAnimationDefinitions.CapsuleCycle, phase);

    /// <summary>The elevator platform's strip at phase <paramref name="phase"/> of its two-phase cycle.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is outside the cycle.</exception>
    internal static CeresFloorAnimationFrame ElevatorPlatformFrameForPhase(int phase) => CycleFrame(CeresFloorAnimationDefinitions.ElevatorPlatformCycle, phase);

    /// <summary>The wings' frame at beat <paramref name="phase"/> of their two-beat cycle.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is outside the cycle.</exception>
    internal static CeresFloorAnimationFrame WingFrameForPhase(int phase) => CycleFrame(CeresFloorAnimationDefinitions.WingCycle, phase);

    /// <summary>The frame at <paramref name="phase"/> of <paramref name="cycle"/>.</summary>
    private static CeresFloorAnimationFrame CycleFrame(CeresFloorAnimationFrame[] cycle, int phase) =>
        (uint)phase < (uint)cycle.Length ? cycle[phase] : throw new ArgumentOutOfRangeException(nameof(phase));
}
