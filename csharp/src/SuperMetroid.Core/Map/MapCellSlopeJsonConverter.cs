namespace SuperMetroid.Core.Map;

/// <summary>Reads and writes a <see cref="MapCellSlope"/> in an installed document by its installed key (#2520).</summary>
internal sealed partial class MapCellSlopeJsonConverter() : InstalledKeyJsonConverter<MapCellSlope>(MapCellSlopeKeys.Installed);
