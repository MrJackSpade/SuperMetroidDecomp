using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The range of an installed Ceres cinematic actor's starting screen coordinate (#1627): a signed whole pixel, negative left
/// of or above the screen, which the screen's wrap holds unchanged.
/// </summary>
internal static class CeresActorScreenCoordinate
{
    /// <summary>
    /// Whether <paramref name="pixels"/> is a signed screen coordinate, -32768 through 32767: one the screen's wrap gives
    /// back unchanged.
    /// </summary>
    /// <param name="pixels">The installed coordinate, in whole screen pixels.</param>
    internal static bool IsSigned(int pixels) => CoordinateAxis.AsSigned(CoordinateAxis.Wrap(pixels)) == pixels;
}
