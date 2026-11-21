namespace SuperMetroid.Core.Map;

/// <summary>Reads and writes a <see cref="MapCellEdge"/> in an installed document by its installed key (#2520).</summary>
internal sealed partial class MapCellEdgeJsonConverter() : InstalledKeyJsonConverter<MapCellEdge>(MapCellEdgeKeys.Installed);
