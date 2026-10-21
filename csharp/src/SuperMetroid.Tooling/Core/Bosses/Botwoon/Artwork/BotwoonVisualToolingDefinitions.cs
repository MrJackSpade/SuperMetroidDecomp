namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>Development-tool members of <see cref="BotwoonVisualDefinitions"/>; never linked by player hosts.</summary>
internal static class BotwoonVisualToolingDefinitions
{
    /// <summary>
    /// The facings in the cartridge's physical program order, from up-left counterclockwise to up.
    /// Installed artwork is named and numbered in this order.
    /// </summary>
    internal static readonly BotwoonFacing[] PhysicalOrder =
    [
        BotwoonFacing.UpLeft, BotwoonFacing.Left, BotwoonFacing.DownLeft, BotwoonFacing.Down,
        BotwoonFacing.DownRight, BotwoonFacing.Right, BotwoonFacing.UpRight, BotwoonFacing.Up,
    ];
}
