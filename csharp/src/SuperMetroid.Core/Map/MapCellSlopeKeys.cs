namespace SuperMetroid.Core.Map;

/// <summary>
/// The installed key of each <see cref="MapCellSlope"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class MapCellSlopeKeys
{
    /// <summary>Each MapCellSlope member's installed key.</summary>
    internal static InstalledKeys<MapCellSlope> Installed { get; } = new(new Dictionary<MapCellSlope, string>
    {
        [MapCellSlope.None] = "None",
        [MapCellSlope.RightwardFloorLow] = "RightwardFloorLow",
        [MapCellSlope.RightwardFloorHigh] = "RightwardFloorHigh",
        [MapCellSlope.RightwardCeilingLow] = "RightwardCeilingLow",
        [MapCellSlope.RightwardCeilingHigh] = "RightwardCeilingHigh",
        [MapCellSlope.LeftwardFloorLow] = "LeftwardFloorLow",
        [MapCellSlope.LeftwardFloorHigh] = "LeftwardFloorHigh",
        [MapCellSlope.LeftwardCeilingLow] = "LeftwardCeilingLow",
        [MapCellSlope.LeftwardCeilingHigh] = "LeftwardCeilingHigh",
    });
}
