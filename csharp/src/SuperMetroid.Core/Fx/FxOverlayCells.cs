using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The BG3 plane a gameplay frame shows (#1403): the picture of each of its <see cref="FxOverlayPlane.Columns"/> by
/// <see cref="FxOverlayPlane.Rows"/> cells, each its word's pre-coloured tile from the room's BG3 sheet (#1392), mirrored,
/// coloured and prioritised as the word was: the room effect's page and nothing else. The HUD is its own layer (#1466).
/// </summary>
public sealed class FxOverlayCells
{
    /// <summary>Holds each cell's picture.</summary>
    /// <exception cref="ArgumentException">There is not one picture for each cell.</exception>
    public FxOverlayCells(ImmutableArray<BgTilePicture?> pictures)
    {
        if (pictures.IsDefault || pictures.Length != FxOverlayPlane.Columns * FxOverlayPlane.Rows)
            throw new ArgumentException($"The BG3 plane needs a picture for each of its {FxOverlayPlane.Columns * FxOverlayPlane.Rows} cells.", nameof(pictures));
        var shown = ImmutableArray.CreateBuilder<BgTilePicture>(pictures.Length);
        for (int cell = 0; cell < pictures.Length; cell++)
            shown.Add(pictures[cell] ?? throw new ArgumentException($"BG3 cell {cell} shows no picture.", nameof(pictures)));
        Pictures = shown.MoveToImmutable();
    }

    /// <summary>Each cell's picture, row-major.</summary>
    public ImmutableArray<BgTilePicture> Pictures { get; }
}
