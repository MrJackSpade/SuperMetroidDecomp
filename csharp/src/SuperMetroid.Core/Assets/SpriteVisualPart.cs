namespace SuperMetroid.Core.Assets;

/// <summary>One ordered region in an indexed tile sheet. Null palette inherits its drawing owner's current palette.</summary>
[SpatialComponents("External format: sprite composition JSON stores each OAM part displacement as offsetX/offsetY.")]
public sealed record SpriteVisualPart
{
    /// <summary>Signed horizontal pixel displacement from the composition origin.</summary>
    public required int OffsetX { get; init; }
    /// <summary>Signed vertical pixel displacement from the composition origin.</summary>
    public required int OffsetY { get; init; }
    /// <summary>Zero-based tile column in the presentation's indexed artwork atlas.</summary>
    public required int TileColumn { get; init; }
    /// <summary>Zero-based tile row in the presentation's indexed artwork atlas.</summary>
    public required int TileRow { get; init; }
    /// <summary>Square OBJ size in pixels, either 8 or 16.</summary>
    [IntegerQuantity("sprite edge length in pixels, eight or sixteen")]
    public required int Size { get; init; }
    /// <summary>SNES OBJ priority tier from 0 through 3.</summary>
    public required int Priority { get; init; }
    /// <summary>OBJ palette selector, or <see langword="null"/> to inherit the drawing owner's palette.</summary>
    public required int? Palette { get; init; }
    /// <summary>Horizontal reflection applied to the selected artwork region.</summary>
    public required bool FlipX { get; init; }
    /// <summary>Vertical reflection applied to the selected artwork region.</summary>
    public required bool FlipY { get; init; }
}
