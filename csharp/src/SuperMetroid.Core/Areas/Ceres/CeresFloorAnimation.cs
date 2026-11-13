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
    /// <summary>
    /// The capsule frames in reflected phase order 0, 1, 2, 1; <paramref name="phase"/> must already be bounded to 0..3.
    /// </summary>
    internal static CeresFloorAnimationFrame BabyFrameForPhase(int phase)
    {
        if ((uint)phase >= CeresFloorAnimationDefinitions.BabyPhaseCount) throw new ArgumentOutOfRangeException(nameof(phase));
        int reflectedPhase = Math.Min(phase, CeresFloorAnimationDefinitions.BabyPhaseCount - phase);
        return CeresFloorAnimationDefinitions.BabyFrames[reflectedPhase];
    }

    /// <summary>The elevator platform's light (0) or dark (1) strip.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is neither.</exception>
    internal static CeresFloorAnimationFrame EnemyFrameId(int frame) =>
        (uint)frame < (uint)CeresFloorAnimationDefinitions.PlatformFrames.Length
            ? CeresFloorAnimationDefinitions.PlatformFrames[frame]
            : throw new ArgumentOutOfRangeException(nameof(frame));
}
