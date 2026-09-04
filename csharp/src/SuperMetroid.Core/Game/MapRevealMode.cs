namespace SuperMetroid.Core.Game;

/// <summary>Temporary host-side visibility applied while presenting cartridge area maps.</summary>
public enum MapRevealMode : byte
{
    /// <summary>Use only persistent exploration and legitimately acquired area-map data.</summary>
    None = 0,

    /// <summary>Reveal every cell exposed by the area's ordinary cartridge map station.</summary>
    Public = 1,

    /// <summary>Reveal every nonblank cartridge map cell, including secret-only cells.</summary>
    Secret = 2,
}
