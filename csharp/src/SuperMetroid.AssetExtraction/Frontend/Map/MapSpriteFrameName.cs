
using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>A map sprite frame and its installed name.</summary>
/// <param name="Id">The id.</param>
/// <param name="Name">The name.</param>
public readonly partial record struct MapSpriteFrameName(MapSpriteId Id, string Name);
