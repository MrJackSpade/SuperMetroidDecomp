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

    /// <summary>The sprite palette rows the frame was coloured from.</summary>
    public required SpritePaletteRows Rows { get; init; }

    /// <summary>The parts the frame was composed from, each an object of the screen's object limit.</summary>
    public required int Objects { get; init; }
}
