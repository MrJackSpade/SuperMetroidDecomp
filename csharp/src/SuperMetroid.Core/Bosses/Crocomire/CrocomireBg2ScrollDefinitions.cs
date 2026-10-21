using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Fixed scroll data used by <c>Update Crocomire BG2 scroll</c> at $A4:8B5B.
/// The cartridge searches 17 body frames in reverse order and, on a match, adds that frame's fourth-component Y
/// offset.
/// This is camera/presentation alignment, not editable boss artwork.
/// </summary>
internal static class CrocomireBg2ScrollDefinitions
{
    /// <summary>Native $A4:8B5B vertical-scroll origin.</summary>
    internal const ushort VerticalOrigin = 0x0043;

    /// <summary>
    /// The body frames whose BG2 alignment needs a signed Y correction: steps 1 to 7 of the charge-forward step back and
    /// steps 1 to 4 of the moving claws. Every other frame, including mapped zero-offset poses, adds zero.
    /// </summary>
    private static readonly Dictionary<EnemyFrameId, int> Corrections = new()
    {
        [EnemyFrameId.CrocomireBodyOamBFF6] = -2,
        [EnemyFrameId.CrocomireBodyOamC028] = -2,
        [EnemyFrameId.CrocomireBodyOamC05A] = -1,
        [EnemyFrameId.CrocomireBodyOamC08C] = -2,
        [EnemyFrameId.CrocomireBodyOamC0BE] = -2,
        [EnemyFrameId.CrocomireBodyOamC0F0] = -1,
        [EnemyFrameId.CrocomireBodyOamC122] = -1,
        [EnemyFrameId.CrocomireBodyOamC4AC] = -2,
        [EnemyFrameId.CrocomireBodyOamC4DE] = -4,
        [EnemyFrameId.CrocomireBodyOamC510] = -2,
        [EnemyFrameId.CrocomireBodyOamC542] = -1,
    };

    /// <summary>The signed Y correction aligning BG2 to body frame <paramref name="frame"/>.</summary>
    internal static int Correction(EnemyFrameId? frame) =>
        frame is { } shown && Corrections.TryGetValue(shown, out int correction) ? correction : 0;

    /// <summary>Applies the signed correction with native sixteen-bit wrapping.</summary>
    internal static ushort VerticalScroll(ushort bodyY, EnemyFrameId? frame) =>
        unchecked((ushort)(VerticalOrigin - bodyY + Correction(frame)));
}