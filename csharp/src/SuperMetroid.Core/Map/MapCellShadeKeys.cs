namespace SuperMetroid.Core.Map;

/// <summary>
/// The installed key of each <see cref="MapCellShade"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class MapCellShadeKeys
{
    /// <summary>Each MapCellShade member's installed key.</summary>
    internal static InstalledKeys<MapCellShade> Installed { get; } = new(new Dictionary<MapCellShade, string>
    {
        [MapCellShade.Unexplored] = "Unexplored",
        [MapCellShade.Explored] = "Explored",
    });
}
