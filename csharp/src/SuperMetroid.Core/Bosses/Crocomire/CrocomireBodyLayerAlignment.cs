using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Aligns his body layer vertically with Crocomire's body: the frame's signed correction from the body's position.</summary>
internal static class CrocomireBodyLayerAlignment
{
    /// <summary>The signed Y correction aligning his body layer to body frame <paramref name="frame"/>.</summary>
    internal static int Correction(EnemyFrameId? frame) =>
        frame is { } shown && CrocomireBodyLayerPlacementDefinitions.Corrections.TryGetValue(shown, out int correction) ? correction : 0;

    /// <summary>Applies the signed correction with native sixteen-bit wrapping.</summary>
    internal static int VerticalPlacement(int bodyY, EnemyFrameId? frame) =>
        CoordinateAxis.Wrap(CrocomireBodyLayerPlacementDefinitions.VerticalOrigin - bodyY + Correction(frame));
}
