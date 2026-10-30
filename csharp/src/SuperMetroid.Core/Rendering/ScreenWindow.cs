namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The part of the screen an effect applies to (#1468): the union of up to <see cref="MaximumShapes"/> shapes, limited
/// to the screen lines from <see cref="FirstLine"/> up to <see cref="EndLine"/>. The backend tests each pixel against
/// the shapes; a window without shapes covers nothing.
/// </summary>
public sealed record ScreenWindow
{
    /// <summary>The most shapes one window joins.</summary>
    public const int MaximumShapes = 4;

    /// <summary>The window's own copy of its shapes.</summary>
    private readonly WindowShape[] shapes;

    /// <summary>The shapes whose union the window covers.</summary>
    public ReadOnlySpan<WindowShape> Shapes => shapes;

    /// <summary>The first screen line the window covers.</summary>
    public int FirstLine { get; }

    /// <summary>The screen line after the last one the window covers.</summary>
    public int EndLine { get; }

    /// <summary>Copies the shapes a window joins, limited to a band of screen lines.</summary>
    /// <param name="shapes">Up to <see cref="MaximumShapes"/> shapes; none covers nothing.</param>
    /// <param name="firstLine">The first screen line covered.</param>
    /// <param name="endLine">The screen line after the last one covered, at or below <paramref name="firstLine"/>.</param>
    /// <exception cref="ArgumentException">There are too many shapes, or one is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The band of lines is not within the screen.</exception>
    public ScreenWindow(ReadOnlySpan<WindowShape> shapes, int firstLine = 0, int endLine = DisplayScreen.Height)
    {
        if (shapes.Length > MaximumShapes)
            throw new ArgumentException($"A screen window joins at most {MaximumShapes} shapes.", nameof(shapes));
        foreach (WindowShape shape in shapes)
            Ensure.NotNull(shape);
        FirstLine = Ensure.BetweenInclusive(0, DisplayScreen.Height, firstLine);
        EndLine = Ensure.BetweenInclusive(FirstLine, DisplayScreen.Height, endLine);
        this.shapes = shapes.ToArray();
    }

    /// <summary>A window that covers nothing.</summary>
    public static ScreenWindow Closed { get; } = new([]);

    /// <summary>One shape, limited to the screen lines from <paramref name="firstLine"/> up to <paramref name="endLine"/>.</summary>
    public static ScreenWindow Of(WindowShape shape, int firstLine = 0, int endLine = DisplayScreen.Height) =>
        new([shape], firstLine, endLine);

    /// <summary>Whether both windows join the same shapes, in the same order, over the same lines.</summary>
    public bool Equals(ScreenWindow? other) =>
        other is not null && FirstLine == other.FirstLine && EndLine == other.EndLine && shapes.SequenceEqual(other.shapes);

    /// <summary>A hash of the window's lines and its first shape.</summary>
    public override int GetHashCode() => HashCode.Combine(FirstLine, EndLine, shapes.Length == 0 ? null : shapes[0]);
}
