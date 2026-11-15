using SuperMetroid.Core.Hud;

namespace SuperMetroid.Verification.Hud;

/// <summary>
/// Checks the minimap's places (#1985): each is named by its offset from Samus's cell and shows in the square the flat,
/// row-by-row grid placed it in, so the HUD's picture is unchanged.
/// </summary>
internal static class MinimapGridVerification
{
    /// <summary>Checks the places' order, their squares, Samus's square and the hidden minimap.</summary>
    internal static void VerifyMinimapGrid()
    {
        AssertEqual(15, MinimapGrid.Places.Length, "the minimap shows five columns by three rows");
        for (int index = 0; index < MinimapGrid.Places.Length; index++)
        {
            Offset16<MapTiles> place = MinimapGrid.Places[index];
            AssertEqual(new Offset16<MapTiles>(index % 5 - 2, index / 5 - 1), place, "places run row by row, left to right, around Samus's cell");
            AssertEqual(new Position16<MapTiles>(index % 5, index / 5), MinimapGrid.Square(place), "each place shows in its row-by-row square");
        }
        AssertEqual(new Position16<MapTiles>(2, 1), MinimapGrid.Square(MinimapGrid.SamusCell), "Samus's cell shows in the middle square");
        AssertTrue(MinimapGrid.Places.All(place => MinimapGrid.Hidden[place] is MinimapHiddenCell), "a boss's room hides every place");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = MinimapGrid.Hidden[new Offset16<MapTiles>(3, 0)], "a place beyond the minimap is no place");
        Console.WriteLine("Minimap grid: fifteen places named from Samus's cell show in the row-by-row squares; the hidden minimap hides them all.");
    }
}
