namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Cartridge values and helpers of <see cref="GameOptionsRow"/> that only verification reads.</summary>
internal static class GameOptionsRowConstants
{
    /// <summary>The rows of <paramref name="page"/> from the top.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The page is not one of the three options pages.</exception>
    internal static IReadOnlyList<GameOptionsRow> OnPage(GameOptionsPage page) => page switch
    {
        GameOptionsPage.Primary => [.. GameOptionsLayout.PrimaryRows.Select(row => new GameOptionsRow.OnPrimaryPage(row))],
        GameOptionsPage.Controller => [.. GameOptionsLayout.ControllerRows.Select(row => new GameOptionsRow.OnControllerPage(row))],
        GameOptionsPage.Special => [.. GameOptionsLayout.SpecialRows.Select(row => new GameOptionsRow.OnSpecialPage(row))],
        _ => throw new ArgumentOutOfRangeException(nameof(page), page, "Options rows exist only on the three menu pages."),
    };
}
