using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Fixed placement data of his body layer, used by <c>Update Crocomire BG2 scroll</c> at $A4:8B5B.
/// The cartridge searches 17 body frames in reverse order and, on a match, adds that frame's fourth-component Y
/// offset.
/// This is camera/presentation alignment, not editable boss artwork.
/// </summary>
internal static class CrocomireBodyLayerPlacementDefinitions
{
    /// <summary>Native $A4:8B5B vertical-scroll origin.</summary>
    internal const int VerticalOrigin = 0x0043;

    /// <summary>
    /// The body frames whose body-layer alignment needs a signed Y correction: steps 1 to 7 of the charge-forward step back and
    /// steps 1 to 4 of the moving claws. Every other frame, including mapped zero-offset poses, adds zero.
    /// </summary>
    internal static readonly Dictionary<EnemyFrameId, int> Corrections = new()
    {
        [EnemyFrameId.CrocomireBody1] = -2,
        [EnemyFrameId.CrocomireBody2] = -2,
        [EnemyFrameId.CrocomireBody3] = -1,
        [EnemyFrameId.CrocomireBody4] = -2,
        [EnemyFrameId.CrocomireBody5] = -2,
        [EnemyFrameId.CrocomireBody6] = -1,
        [EnemyFrameId.CrocomireBody7] = -1,
        [EnemyFrameId.CrocomireBody20] = -2,
        [EnemyFrameId.CrocomireBody21] = -4,
        [EnemyFrameId.CrocomireBody22] = -2,
        [EnemyFrameId.CrocomireBody23] = -1,
    };
}
