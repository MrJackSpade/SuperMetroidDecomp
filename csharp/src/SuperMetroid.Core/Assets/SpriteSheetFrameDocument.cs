namespace SuperMetroid.Core.Assets;

/// <summary>One frame in a sprite sheet manifest.</summary>
public sealed record SpriteSheetFrameDocument
{
    /// <summary>The frame rectangle's top-left pixel on the sheet.</summary>
    public required Position16<ImagePixels> Corner { get; init; }

    /// <summary>The frame rectangle's width and height.</summary>
    public required Extent16<ImagePixels> Size { get; init; }

    /// <summary>The frame's origin, from the rectangle's top-left pixel.</summary>
    public required Offset16<ImagePixels> Origin { get; init; }

    /// <summary>Whether the frame takes part in the screen's colour blending.</summary>
    public required bool Blends { get; init; }

    /// <summary>The frame's authored priority against the backgrounds 0..3.</summary>
    public required int Priority { get; init; }

    /// <summary>The sprite palette rows the frame was coloured from.</summary>
    public required SpritePaletteRows Rows { get; init; }
}
