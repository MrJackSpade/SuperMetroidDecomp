namespace SuperMetroid.Core.Frontend.Map;

/// <summary>One area's calculated SRAM destination and occupied native map-byte view.</summary>
internal readonly record struct ExploredMapPackingDefinition(
    ushort DestinationOffset,
    ExploredMapPackingDefinitions.OccupiedByteIndexes AreaByteIndexes);
