using System.Collections.Immutable;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// How every HUD image is laid out on a sheet (#1466): shelves of images, each as many cell rows tall as its tallest image,
/// one above the next, each image at its place on its shelf.
/// </summary>
/// <param name="ShelfRows">Each shelf's height in cell rows, top shelf first.</param>
/// <param name="Placements">Each image's place.</param>
internal sealed record HudSheetLayout(ImmutableArray<int> ShelfRows, ImmutableArray<HudSheetPlacement> Placements)
{
    /// <summary>The cell rows every shelf takes.</summary>
    internal int Rows => ShelfRows.Sum();

    /// <summary>The cell row shelf <paramref name="shelf"/> starts on.</summary>
    internal int ShelfTop(int shelf) => ShelfRows.Take(shelf).Sum();
}
