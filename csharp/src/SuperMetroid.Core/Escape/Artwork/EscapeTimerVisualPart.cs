namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>One ordered OAM part using the timer's full nine-bit gameplay OBJ tile number.</summary>
[SpatialComponents("External format: the escape-timer JSON stores each OAM part displacement as offsetX/offsetY.")]
public sealed record EscapeTimerVisualPart
{
    /// <summary>Signed horizontal pixel offset from the composition's anchor, from -256 through 255, encoded in the native nine-bit spritemap X field.</summary>
    public required int OffsetX { get; init; }
    /// <summary>Signed vertical pixel offset from the composition's anchor, from -128 through 127, encoded as a signed byte.</summary>
    public required int OffsetY { get; init; }
    /// <summary>Gameplay OBJ character number from $1E0, the timer's first character, through $1F8, its last; a 16-pixel part must also fit its other three characters in that range.</summary>
    public required int TileNumber { get; init; }
    /// <summary>Square sprite side length in pixels, either 8 or 16, selecting the small or large gameplay OBJ size.</summary>
    [IntegerQuantity("sprite edge length in pixels, eight or sixteen")]
    public required int Size { get; init; }
    /// <summary>OBJ priority tier from 0 through 3; stock timer parts use 3.</summary>
    public required int Priority { get; init; }
    /// <summary>Explicit OBJ palette selector from 0 through 7, or null to inherit the presentation's shared palette.</summary>
    public required int? Palette { get; init; }
    /// <summary>Whether the sprite's pixels are reflected horizontally without changing its authored anchor offset.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Whether the sprite's pixels are reflected vertically without changing its authored anchor offset.</summary>
    public required bool FlipY { get; init; }
}
