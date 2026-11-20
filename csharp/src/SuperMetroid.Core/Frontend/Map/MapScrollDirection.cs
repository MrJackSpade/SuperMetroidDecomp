namespace SuperMetroid.Core.Frontend.Map;

/// <summary>The direction a map scroll step moves the viewport.</summary>
public enum MapScrollDirection
{
    /// <summary>Scroll the map viewport toward smaller horizontal offsets.</summary>
    Left,
    /// <summary>Scroll the map viewport toward larger horizontal offsets.</summary>
    Right,
    /// <summary>Scroll the map viewport toward smaller vertical offsets.</summary>
    Up,
    /// <summary>Scroll the map viewport toward larger vertical offsets.</summary>
    Down
}
