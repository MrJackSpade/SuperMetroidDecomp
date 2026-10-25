namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The cartridge character and packed map sources of an ending Mode 7 scene (#1416: moved out of Core).</summary>
/// <param name="Characters">The compressed high-byte character lane.</param>
/// <param name="PackedMap">The compressed packed low-byte map source.</param>
internal readonly record struct EndingFloorSources(int Characters, int PackedMap);
