namespace SuperMetroid.Core.Frontend.Options.Artwork;

/// <summary>
/// Places the options screen's sprites (<see cref="GameOptionsPresentationDefinitions"/>): the cursor and heading anchors.
/// The pages' cells are the import's, which pictures them (#1467).
/// </summary>
public static class GameOptionsArtworkCells
{
    /// <summary>$82:F307/F31B/F33F selection missile rows, including controller scroll displacement.</summary>
    internal static Position16<SpritePixels> CursorAnchor(GameOptionsPage page, int row)
    {
        if ((uint)row >= GameOptionsNames.CursorCount(page))
            throw new InvalidDataException($"Options cursor {page}[{row}] is not authored.");
        return page switch
        {
            GameOptionsPage.Primary => new(GameOptionsDefinitions.Cursors.PrimaryX, GameOptionsPositions.PrimaryY(row)),
            GameOptionsPage.Controller => new(GameOptionsDefinitions.Cursors.ControllerX, GameOptionsPositions.ControllerY(row)),
            GameOptionsPage.Special => new(GameOptionsDefinitions.Cursors.SpecialX, GameOptionsPositions.SpecialY(row)),
            _ => throw new ArgumentOutOfRangeException(nameof(page)),
        };
    }

    /// <summary>$82:F34B/F353/F35B and F369 named page heading setup anchors.</summary>
    internal static Position16<SpritePixels> HeadingAnchor(GameOptionsPage page) =>
        new(GameOptionsPositions.HeadingX(page), GameOptionsDefinitions.Headings.HeadingY);
}
