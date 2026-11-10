namespace SuperMetroid.Core.Fx;

/// <summary>The way an earthquake throws what it shakes: across, up and down, or both at once.</summary>
public enum EarthquakeDirection
{
    /// <summary>Side to side.</summary>
    Horizontal,

    /// <summary>Up and down.</summary>
    Vertical,

    /// <summary>Side to side and up and down together.</summary>
    Diagonal,
}
