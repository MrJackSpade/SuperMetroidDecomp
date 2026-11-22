namespace SuperMetroid.Core.Map;

/// <summary>
/// The installed key of each <see cref="MapCellEdge"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class MapCellEdgeKeys
{
    /// <summary>Each MapCellEdge member's installed key.</summary>
    internal static InstalledKeys<MapCellEdge> Installed { get; } = new(new Dictionary<MapCellEdge, string>
    {
        [MapCellEdge.Open] = "Open",
        [MapCellEdge.Wall] = "Wall",
        [MapCellEdge.ElevatorShaft] = "ElevatorShaft",
    });
}
