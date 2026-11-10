namespace SuperMetroid.Core.Earthquakes;

/// <summary>How far an earthquake throws what it shakes from its rest position (<see cref="EarthquakeShakeDefinitions.Pixels"/>).</summary>
public enum EarthquakeStrength
{
    /// <summary>A light shake.</summary>
    Light,

    /// <summary>A medium shake.</summary>
    Medium,

    /// <summary>A strong shake.</summary>
    Strong,
}
