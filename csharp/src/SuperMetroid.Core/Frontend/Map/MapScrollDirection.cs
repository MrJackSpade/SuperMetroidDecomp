namespace SuperMetroid.Core.Frontend.Map;

/// <summary>$82:925D's mutually exclusive map-scrolling dispatcher indices.</summary>
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
