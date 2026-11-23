namespace Engine.Shared;

/// <summary>An exact distance and direction in two dimensions: how far right and how far down.</summary>
/// <param name="X">The distance rightward; negative is leftward.</param>
/// <param name="Y">The distance downward; negative is upward.</param>
public readonly record struct PixelOffset(Pixels X, Pixels Y)
{
    /// <summary>No distance.</summary>
    public static PixelOffset Zero => default;

    /// <summary>A whole number of pixels each way.</summary>
    public static PixelOffset FromWhole(int x, int y) => new(Pixels.FromWhole(x), Pixels.FromWhole(y));

    public static PixelOffset operator +(PixelOffset left, PixelOffset right) => new(left.X + right.X, left.Y + right.Y);

    public static PixelOffset operator -(PixelOffset left, PixelOffset right) => new(left.X - right.X, left.Y - right.Y);

    public static PixelOffset operator -(PixelOffset value) => new(-value.X, -value.Y);
}
