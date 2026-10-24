namespace SuperMetroid.Core.Spatial;

/// <summary>What shows beyond a Mode 7 floor's 1024 by 1024 pixels (#1403).</summary>
public enum Mode7OverflowMode : byte
{
    /// <summary>Nothing shows beyond the floor.</summary>
    Transparent = 0,
    /// <summary>Tile zero shows beyond the floor, at the pixel the low three bits of each coordinate name.</summary>
    CharacterZero = 1,
    /// <summary>The floor repeats beyond its edges.</summary>
    Wrap = 2,
}
