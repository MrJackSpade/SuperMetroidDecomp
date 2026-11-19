namespace SuperMetroid.Core.Map;

/// <summary>Reads and writes a <see cref="MapCellKind"/> in an installed document by its installed key (#2520).</summary>
internal sealed class MapCellKindJsonConverter() : InstalledKeyJsonConverter<MapCellKind>(MapCellKindKeys.Installed);
