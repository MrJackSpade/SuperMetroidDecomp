namespace SuperMetroid.Core.Hardware;

/// <summary>A readable projection of one hardware OAM record.</summary>
/// <param name="X">The nine-bit screen X, 0 through 511, where values from 256 lie left of the screen.</param>
/// <param name="Y">The eight-bit screen Y, where values past the visible field lie above the screen.</param>
/// <param name="TileNumber">The nine-bit OBJ character number.</param>
/// <param name="Palette">The OBJ palette, 0 through 7.</param>
/// <param name="Priority">The OBJ priority tier, 0 through 3.</param>
/// <param name="FlipX">Whether the sprite is mirrored horizontally.</param>
/// <param name="FlipY">Whether the sprite is mirrored vertically.</param>
/// <param name="IsLarge">Whether the sprite uses the larger configured OBJ size.</param>
public readonly record struct OamEntry(
    int X,
    byte Y,
    int TileNumber,
    int Palette,
    int Priority,
    bool FlipX,
    bool FlipY,
    bool IsLarge);
