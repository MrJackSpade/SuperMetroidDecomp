namespace SuperMetroid.Core.Map;

/// <summary>How much of each area map an assist shows; drawn only, never saved as exploration or as a collected map.</summary>
public enum MapRevealMode
{
    /// <summary>Use only persistent exploration and legitimately acquired area-map data.</summary>
    None = 0,

    /// <summary>Reveal every cell exposed by the area's ordinary map station.</summary>
    Public = 1,

    /// <summary>Reveal every nonblank map cell, including secret-only cells.</summary>
    Secret = 2,
}
