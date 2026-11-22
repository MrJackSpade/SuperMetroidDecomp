using System.Collections.Frozen;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Immutable map-arrow anchors; no controller bindings.</summary>
public sealed partial class MapArrowPresentation
{
    /// <summary>The four validated direction entries, with no neutral arrow selection.</summary>
    private readonly FrozenDictionary<MapScrollDirection, MapArrowVisual> visuals;

    /// <summary>Retains the directional visuals the import validated.</summary>
    internal MapArrowPresentation(MapArrowVisual left, MapArrowVisual right, MapArrowVisual up, MapArrowVisual down) =>
        visuals = new Dictionary<MapScrollDirection, MapArrowVisual>
        {
            [MapScrollDirection.Left] = left,
            [MapScrollDirection.Right] = right,
            [MapScrollDirection.Up] = up,
            [MapScrollDirection.Down] = down,
        }.ToFrozenDictionary();

    /// <summary>Selects the authored visual for a named map-scroll direction.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> is no direction, which has no arrow.</exception>
    public MapArrowVisual Get(MapScrollDirection direction) => Ensure.Listed(visuals, direction);
}
