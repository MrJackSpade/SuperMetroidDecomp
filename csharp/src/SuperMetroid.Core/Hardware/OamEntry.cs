namespace SuperMetroid.Core.Hardware;

/// <summary>A readable projection of one hardware OAM record.</summary>
public readonly record struct OamEntry(
    int X,
    byte Y,
    int TileNumber,
    int Palette,
    int Priority,
    bool FlipX,
    bool FlipY,
    bool IsLarge);
