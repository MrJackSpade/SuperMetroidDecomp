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
}
