using SuperMetroid.Core.Spatial;
namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>Development-tool members of <see cref="BotwoonVisualDefinitions"/>; never linked by player hosts.</summary>
internal static class BotwoonVisualToolingDefinitions
{
    /// <summary>
    /// The facings in the cartridge's physical program order, from up-left counterclockwise to up.
    /// Installed artwork is named and numbered in this order.
    /// </summary>
    internal static readonly CompassDirection[] PhysicalOrder =
    [
        CompassDirection.UpLeft, CompassDirection.Left, CompassDirection.DownLeft, CompassDirection.Down,
        CompassDirection.DownRight, CompassDirection.Right, CompassDirection.UpRight, CompassDirection.Up,
    ];
}
