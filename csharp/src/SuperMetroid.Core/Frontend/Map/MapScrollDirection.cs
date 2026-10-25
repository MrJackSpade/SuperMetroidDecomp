namespace SuperMetroid.Core.Frontend.Map;

/// <summary>The direction a map scroll step moves the viewport, or none.</summary>
public enum MapScrollDirection
{
    /// <summary>No eight-tick scroll step is active.</summary>
    None,
    /// <summary>Scroll the map viewport toward smaller horizontal offsets.</summary>
    Left,
    /// <summary>Scroll the map viewport toward larger horizontal offsets.</summary>
    Right,
    /// <summary>Scroll the map viewport toward smaller vertical offsets.</summary>
    Up,
    /// <summary>Scroll the map viewport toward larger vertical offsets.</summary>
    Down
}
