namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Immutable map-arrow anchors and cosmetic phase durations; no controller bindings.</summary>
public sealed class MapArrowPresentation
{
    /// <summary>Four validated direction entries, with no neutral arrow selection.</summary>
    private readonly MapArrowVisual left, right, up, down;
    /// <summary>Retains the directional visuals the import validated.</summary>
    internal MapArrowPresentation(MapArrowVisual left, MapArrowVisual right, MapArrowVisual up, MapArrowVisual down)
    { this.left = left; this.right = right; this.up = up; this.down = down; }

    /// <summary>Selects the authored visual for a named map-scroll direction.</summary>
    public MapArrowVisual Get(MapScrollDirection direction) => direction switch
    {
        MapScrollDirection.Left => left,
        MapScrollDirection.Right => right,
        MapScrollDirection.Up => up,
        MapScrollDirection.Down => down,
        MapScrollDirection.None => throw new ArgumentOutOfRangeException(nameof(direction)),
        _ => throw new ArgumentOutOfRangeException(nameof(direction)),
    };
}
