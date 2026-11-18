namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Cartridge values and helpers of <see cref="GameOptionsRow"/> that only verification reads.</summary>
internal static class GameOptionsRowConstants
{
    /// <summary>The rows of <paramref name="page"/> from the top.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The page is not one of the three options pages.</exception>
    internal static IReadOnlyList<GameOptionsRow> OnPage(GameOptionsPage page) => page switch
    {
        GameOptionsPage.Primary => [.. GameOptionsPageRows.Primary.Select(row => new GameOptionsRow.OnPrimaryPage())],
        GameOptionsPage.Controller => [.. GameOptionsPageRows.Controller.Select(row => new GameOptionsRow.OnControllerPage())],
        GameOptionsPage.Special => [.. GameOptionsPageRows.Special.Select(row => new GameOptionsRow.OnSpecialPage())],
        _ => throw new ArgumentOutOfRangeException(nameof(page), page, "Options rows exist only on the three menu pages."),
    };
}
