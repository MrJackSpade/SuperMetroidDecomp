namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The character and packed map sources of an ending Mode 7 scene.</summary>
/// <param name="Characters">The characters.</param>
/// <param name="PackedMap">The packed map.</param>
public readonly record struct EndingFloorSources(int Characters, int PackedMap);
