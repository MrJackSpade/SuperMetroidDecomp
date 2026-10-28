using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Aligns BG2 vertically with Crocomire's body: the frame's signed correction from the body's position.</summary>
internal static class CrocomireBg2Alignment
{
    /// <summary>The signed Y correction aligning BG2 to body frame <paramref name="frame"/>.</summary>
    internal static int Correction(EnemyFrameId? frame) =>
        frame is { } shown && CrocomireBg2ScrollDefinitions.Corrections.TryGetValue(shown, out int correction) ? correction : 0;

    /// <summary>Applies the signed correction with native sixteen-bit wrapping.</summary>
    internal static ushort VerticalScroll(int bodyY, EnemyFrameId? frame) =>
        unchecked((ushort)(CrocomireBg2ScrollDefinitions.VerticalOrigin - bodyY + Correction(frame)));
}
