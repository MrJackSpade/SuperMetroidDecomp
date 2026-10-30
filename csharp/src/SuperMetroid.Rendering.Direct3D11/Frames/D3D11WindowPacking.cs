using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// Packs a <see cref="ScreenWindow"/> into the constant words the shaders' window test reads (#1468, Window.hlsli):
/// a header row of the shape count, the first line and the line after the last, then two rows per shape, its kind and
/// its parameters as signed words.
/// </summary>
internal static class D3D11WindowPacking
{
    /// <summary>The words in one row of the window block.</summary>
    private const int RowWords = 4;

    /// <summary>The rows each shape takes.</summary>
    private const int ShapeRows = 2;

    /// <summary>The words the window block takes: its header row and each shape's rows.</summary>
    internal const int Words = RowWords * (1 + ShapeRows * ScreenWindow.MaximumShapes);

    /// <summary>The kind of each shape, as Window.hlsli names them.</summary>
    private enum ShapeKind
    {
        /// <summary>No shape.</summary>
        None,
        /// <summary>A <see cref="WindowRectangle"/>.</summary>
        Rectangle,
        /// <summary>A <see cref="WindowOval"/>.</summary>
        Oval,
        /// <summary>A <see cref="WindowCone"/>.</summary>
        Cone,
    }

    /// <summary>Writes <paramref name="window"/> into the window block of <paramref name="data"/>.</summary>
    internal static void Pack(Span<uint> data, ScreenWindow window)
    {
        Ensure.NotNull(window);
        Span<uint> block = data.Slice(D3D11ShaderLayout.WindowWordOffset, Words);
        block.Clear();
        ReadOnlySpan<WindowShape> shapes = window.Shapes;
        block[0] = (uint)shapes.Length;
        block[1] = (uint)window.FirstLine;
        block[2] = (uint)window.EndLine;
        for (int index = 0; index < shapes.Length; index++)
        {
            Span<uint> row = block.Slice(RowWords * (1 + ShapeRows * index), RowWords * ShapeRows);
            switch (shapes[index])
            {
                case WindowRectangle rectangle:
                    Write(row, ShapeKind.Rectangle, rectangle.TopLeft.X, rectangle.TopLeft.Y, rectangle.BottomRight.X,
                        rectangle.BottomRight.Y, 0);
                    break;
                case WindowOval oval:
                    Write(row, ShapeKind.Oval, oval.CoreStart.X, oval.CoreStart.Y, oval.CoreWidth,
                        oval.HorizontalRadius, oval.VerticalRadius);
                    break;
                case WindowCone cone:
                    Write(row, ShapeKind.Cone, cone.Apex.X, cone.Apex.Y, cone.FirstEdge, cone.SecondEdge, 0);
                    break;
                default:
                    throw new NotSupportedException($"The backend cannot draw a {shapes[index].GetType().Name} window shape.");
            }
        }
    }

    /// <summary>Writes one shape's kind and its five parameters, in order, as signed words.</summary>
    private static void Write(Span<uint> row, ShapeKind kind, int first, int second, int third, int fourth, int fifth)
    {
        row[0] = (uint)kind;
        row[1] = unchecked((uint)first);
        row[2] = unchecked((uint)second);
        row[3] = unchecked((uint)third);
        row[4] = unchecked((uint)fourth);
        row[5] = unchecked((uint)fifth);
    }
}
