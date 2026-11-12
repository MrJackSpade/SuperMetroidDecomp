using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The pixels a picture hides as it is drawn, by 8x8 cell (#1652): for each cell, in reading order, the pixel columns of the
/// cell that show nothing. A dissolve that erases columns of an illustration step by step hides them here, so the
/// illustration is stored once and shown at every step.
/// </summary>
/// <param name="CellsAcross">Cells across the picture.</param>
/// <param name="CellsDown">Cells down the picture.</param>
/// <param name="Cells">Each cell's hidden pixel columns, in reading order.</param>
public sealed record HiddenPictureColumns(int CellsAcross, int CellsDown, ImmutableArray<CellColumns> Cells)
{
    /// <summary>Pixels along a cell's side.</summary>
    public const int CellPixels = 8;

    /// <summary>Each pixel column of a cell, from its left.</summary>
    public static ImmutableArray<CellColumns> Columns { get; } =
    [
        CellColumns.First, CellColumns.Second, CellColumns.Third, CellColumns.Fourth,
        CellColumns.Fifth, CellColumns.Sixth, CellColumns.Seventh, CellColumns.Eighth,
    ];

    /// <summary>Whether the picture's pixel at <paramref name="pixel"/> shows nothing.</summary>
    public bool Hides(Position16<ImagePixels> pixel) =>
        Cells[(pixel.Y / CellPixels * CellsAcross) + (pixel.X / CellPixels)].HasFlag(Columns[pixel.X % CellPixels]);
}
