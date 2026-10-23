using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The BG3 plane a gameplay frame shows (#1403): the picture of each of its <see cref="Layer3Plane.Columns"/> by
/// <see cref="Layer3Plane.Rows"/> cells, each its word's pre-coloured tile from the room's BG3 sheet (#1392), mirrored,
/// coloured and prioritised as the word was. The HUD's rows sit at the top; the room effect's area below.
/// </summary>
public sealed class Layer3Cells
{
    /// <summary>Holds each cell's picture.</summary>
    /// <exception cref="ArgumentException">There is not one picture for each cell.</exception>
    public Layer3Cells(ImmutableArray<BgTilePicture?> pictures)
    {
        if (pictures.IsDefault || pictures.Length != Layer3Plane.Columns * Layer3Plane.Rows)
            throw new ArgumentException($"The BG3 plane needs a picture for each of its {Layer3Plane.Columns * Layer3Plane.Rows} cells.", nameof(pictures));
        var shown = ImmutableArray.CreateBuilder<BgTilePicture>(pictures.Length);
        for (int cell = 0; cell < pictures.Length; cell++)
            shown.Add(pictures[cell] ?? throw new ArgumentException($"BG3 cell {cell} shows no picture.", nameof(pictures)));
        Pictures = shown.MoveToImmutable();
    }

    /// <summary>Each cell's picture, row-major.</summary>
    public ImmutableArray<BgTilePicture> Pictures { get; }

    /// <summary>
    /// A window of the plane as BG3 shows it: <paramref name="rows"/> rows from <paramref name="firstRow"/>, repeating
    /// every 256 pixels across and every <paramref name="rows"/> rows down.
    /// </summary>
    /// <param name="firstRow">The first row: 0 for the whole plane or the HUD, 32 for an atmosphere's page.</param>
    /// <param name="rows">32 or 64.</param>
    /// <exception cref="ArgumentOutOfRangeException">The rows are not a window of the plane.</exception>
    public IBgSource Shown(int firstRow, int rows)
    {
        if (rows is not (32 or 64) || firstRow < 0 || firstRow + rows > Layer3Plane.Rows)
            throw new ArgumentOutOfRangeException(nameof(rows), rows, $"Rows {firstRow}..{firstRow + rows - 1} are not a window of the plane.");
        return new Window(this, firstRow, rows);
    }

    /// <summary>A window of the plane, each cell drawn from its picture.</summary>
    private sealed class Window(Layer3Cells plane, int firstRow, int rows) : IBgSource
    {
        /// <inheritdoc />
        public bool TrySample(Position16<BgPlanePixels> scrolled, out BgSample sample)
        {
            const int tile = 8;
            int row = firstRow + scrolled.Y / tile % rows, column = scrolled.X / tile % Layer3Plane.Columns;
            BgTilePicture picture = plane.Pictures[row * Layer3Plane.Columns + column];
            sample = BgSample.OfPicture(picture.Picture, new Position16<ImagePixels>(
                (ushort)(picture.Corner.X + scrolled.X % tile), (ushort)(picture.Corner.Y + scrolled.Y % tile)));
            return true;
        }
    }
}
