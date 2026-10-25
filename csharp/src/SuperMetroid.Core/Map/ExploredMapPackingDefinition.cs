namespace SuperMetroid.Core.Map;

/// <summary>One area's calculated SRAM destination and occupied native map-byte view.</summary>
/// <param name="SaveOffset">The area's first byte in the packed save map: the sum of the earlier areas' byte counts.</param>
/// <param name="AreaByteIndexes">The map bytes the area occupies.</param>
internal readonly record struct ExploredMapPackingDefinition(
    int SaveOffset,
    ExploredMapPackingDefinitions.OccupiedByteIndexes AreaByteIndexes);
