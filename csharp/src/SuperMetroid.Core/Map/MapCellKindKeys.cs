namespace SuperMetroid.Core.Map;

/// <summary>
/// The installed key of each <see cref="MapCellKind"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class MapCellKindKeys
{
    /// <summary>Each MapCellKind member's installed key.</summary>
    internal static InstalledKeys<MapCellKind> Installed { get; } = new(new Dictionary<MapCellKind, string>
    {
        [MapCellKind.Empty] = "Empty",
        [MapCellKind.Room] = "Room",
        [MapCellKind.Item] = "Item",
        [MapCellKind.SaveStation] = "SaveStation",
        [MapCellKind.Elevator] = "Elevator",
        [MapCellKind.ElevatorShaft] = "ElevatorShaft",
        [MapCellKind.ElevatorExit] = "ElevatorExit",
        [MapCellKind.Passage] = "Passage",
    });
}
