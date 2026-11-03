using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The shade the Ceres elevator arrival's projectiles are pre-coloured in (#1512): a new game clears the colour the
/// level-data concealer's solid pixels show, so the concealer is black until the arrival ends. The import colours it.
/// </summary>
internal static class CeresElevatorShades
{
    /// <summary>The arrival's colours with the concealer's colour cleared to black.</summary>
    internal static PictureShade ConcealerCleared { get; } = new("ceres-elevator-concealer-cleared", 0);
}
