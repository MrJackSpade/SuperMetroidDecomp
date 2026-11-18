using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The pixels a picture hides as it is drawn (#1652, #2478): for each pixel of the picture, in reading order, whether it
/// shows nothing. A dissolve that erases an illustration step by step hides its pixels here, so the illustration is stored
/// once and shown at every step.
/// </summary>
public sealed class HiddenPicturePixels
{
    /// <summary>Whether each pixel shows nothing, in reading order.</summary>
    private readonly ImmutableArray<bool> hidden;

    /// <summary>The pixels a picture of <paramref name="size"/> hides: those <paramref name="hidden"/> marks, in reading order.</summary>
    /// <exception cref="ArgumentException">The marks are not one for each pixel of the picture.</exception>
    public HiddenPicturePixels(Extent16<ImagePixels> size, ImmutableArray<bool> hidden)
    {
        Ensure.LengthEqual(hidden.AsSpan(), size.X * size.Y);
        Size = size;
        this.hidden = hidden;
    }

    /// <summary>The width and height of the picture.</summary>
    public Extent16<ImagePixels> Size { get; }

    /// <summary>Whether the picture's pixel at <paramref name="pixel"/> shows nothing.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The pixel lies outside the picture.</exception>
    public bool Hides(Position16<ImagePixels> pixel) =>
        hidden[(Ensure.BetweenInclusive(0, Size.Y - 1, pixel.Y) * Size.X) + Ensure.BetweenInclusive(0, Size.X - 1, pixel.X)];
}
