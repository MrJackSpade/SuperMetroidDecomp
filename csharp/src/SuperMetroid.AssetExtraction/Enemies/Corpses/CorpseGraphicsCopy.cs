namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>One copy of a corpse's graphics between its rotting surfaces.</summary>
/// <param name="SourceOffset">The first byte copied, within the source surface.</param>
/// <param name="DestinationOffset">Where it lands, within the destination surface.</param>
/// <param name="Length">How many bytes are copied.</param>
internal readonly record struct CorpseGraphicsCopy(int SourceOffset, int DestinationOffset, int Length);
