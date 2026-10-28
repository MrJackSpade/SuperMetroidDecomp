using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A tile layer reading characters of one depth, whose tiles the ending's screen remembers (#1404).</summary>
/// <param name="Layer">The tile layer.</param>
/// <param name="Depth">The depth of the characters it reads.</param>
internal readonly record struct EndingLayerTiles(EndingTileLayer Layer, TileInkDepth Depth);
