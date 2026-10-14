namespace SuperMetroid.Core.Rendering;

/// <summary>Exclusive Mode 7 overflow operations shared by display packets and GPU constants.</summary>
public enum Mode7OverflowMode : byte
{
    /// <summary>M7SEL=$80: samples beyond the 1024-pixel map are transparent.</summary>
    Transparent = 0,
    /// <summary>M7SEL=$C0: use character zero outside the map, retaining the pixel's low three coordinates.</summary>
    CharacterZero = 1,
    /// <summary>M7SEL bit seven clear: wrap both tile-map coordinates to ten bits.</summary>
    Wrap = 2,
}
