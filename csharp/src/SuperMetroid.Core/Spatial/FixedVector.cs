namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A two-dimensional displacement or per-frame velocity, in pixels and fractions of a pixel. Each axis is one
/// <see cref="SignedSixteenSixteen"/>, so whole/fraction assembly, overflow and wrap live
/// in exactly one place.
/// </summary>
/// <typeparam name="TSpace">The coordinate space the displacement is measured in.</typeparam>
public readonly partial record struct FixedVector<TSpace>(SignedSixteenSixteen X, SignedSixteenSixteen Y)
    where TSpace : ISpatialSpace
{
    /// <summary>No displacement.</summary>
    public static FixedVector<TSpace> Zero => default;

    /// <summary>A displacement of whole pixels only.</summary>
    public static FixedVector<TSpace> FromWholePixels(Offset16<TSpace> offset) =>
        new(SignedSixteenSixteen.FromWholePixels(offset.X), SignedSixteenSixteen.FromWholePixels(offset.Y));

    /// <summary>Both axes' whole pixels, -32768 through 32767, as a signed whole-pixel offset.</summary>
    public Offset16<TSpace> Whole => new(X.WholePixels, Y.WholePixels);

    /// <summary>Replaces the X axis.</summary>
    public FixedVector<TSpace> WithX(SignedSixteenSixteen x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public FixedVector<TSpace> WithY(SignedSixteenSixteen y) => this with { Y = y };

    /// <summary>Replaces both axes' whole pixels, keeping both fractions.</summary>
    public FixedVector<TSpace> WithWhole(Offset16<TSpace> whole) =>
        new(X.WithWholePixels(whole.X), Y.WithWholePixels(whole.Y));

    /// <summary>Replaces only the X whole pixels, as game code that sets only the whole pixels does.</summary>
    public FixedVector<TSpace> WithWholeX(int wholePixels) => WithX(X.WithWholePixels(wholePixels));

    /// <summary>Replaces only the Y whole pixels, as game code that sets only the whole pixels does.</summary>
    public FixedVector<TSpace> WithWholeY(int wholePixels) => WithY(Y.WithWholePixels(wholePixels));

    /// <summary>Adds two displacements, carrying fractions and wrapping each axis at 65536 whole pixels.</summary>
    public static FixedVector<TSpace> operator +(FixedVector<TSpace> left, FixedVector<TSpace> right) =>
        new(left.X + right.X, left.Y + right.Y);

    /// <summary>Adds whole pixels to each axis, wrapping each axis's whole pixels at 65536; fractions are unchanged.</summary>
    public static FixedVector<TSpace> operator +(FixedVector<TSpace> vector, Offset16<TSpace> wholePixels) =>
        vector + FromWholePixels(wholePixels);

    /// <summary>Subtracts whole pixels from each axis, wrapping each axis's whole pixels at 65536; fractions are unchanged.</summary>
    public static FixedVector<TSpace> operator -(FixedVector<TSpace> vector, Offset16<TSpace> wholePixels) =>
        vector - FromWholePixels(wholePixels);

    /// <summary>The displacement reversed: each axis's whole pixels and fraction reversed together.</summary>
    public static FixedVector<TSpace> operator -(FixedVector<TSpace> vector) => new(-vector.X, -vector.Y);

    /// <summary>Subtracts two displacements, fractions included, wrapping each axis at 65536 whole pixels.</summary>
    public static FixedVector<TSpace> operator -(FixedVector<TSpace> left, FixedVector<TSpace> right) =>
        new(left.X - right.X, left.Y - right.Y);
}
