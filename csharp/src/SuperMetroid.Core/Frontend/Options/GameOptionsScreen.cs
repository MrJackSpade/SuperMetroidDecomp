using SuperMetroid.Core.Progression;
namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// One of the options menu's screens (#2560): a page, and for a settings page the language its labels are written in. The
/// primary page is one screen whichever language is chosen; it shows the choice in its language rows.
/// </summary>
public sealed partial record GameOptionsScreen
{
    /// <summary>Holds the screen's page and language.</summary>
    private GameOptionsScreen(GameOptionsPage page, TextLanguage? language)
    {
        Page = page;
        Language = language;
    }

    /// <summary>The primary options page.</summary>
    public static GameOptionsScreen Primary { get; } = new(GameOptionsPage.Primary, null);

    /// <summary>Every screen, in document order: the primary page, then each settings page in English and in Japanese.</summary>
    public static IReadOnlyList<GameOptionsScreen> All { get; } =
    [
        Primary,
        Settings(GameOptionsPage.Controller, TextLanguage.English),
        Settings(GameOptionsPage.Controller, TextLanguage.Japanese),
        Settings(GameOptionsPage.Special, TextLanguage.English),
        Settings(GameOptionsPage.Special, TextLanguage.Japanese),
    ];

    /// <summary>The page the screen shows.</summary>
    public GameOptionsPage Page { get; }

    /// <summary>The language a settings page's labels are written in; null for the primary page, which has one screen for both.</summary>
    public TextLanguage? Language { get; }

    /// <summary>The settings page <paramref name="page"/> written in <paramref name="language"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is the primary page, which is no settings page.</exception>
    public static GameOptionsScreen Settings(GameOptionsPage page, TextLanguage language) => page switch
    {
        GameOptionsPage.Controller or GameOptionsPage.Special => new(page, language),
        GameOptionsPage.Primary => throw new ArgumentOutOfRangeException(nameof(page), page, "The primary page is no settings page."),
        _ => throw new ArgumentOutOfRangeException(nameof(page), page, "The primary page is no settings page."),
    };

    /// <summary>The screen the menu shows for <paramref name="page"/> while <paramref name="language"/> is chosen.</summary>
    public static GameOptionsScreen Shown(GameOptionsPage page, TextLanguage language) =>
        page == GameOptionsPage.Primary ? Primary : Settings(page, language);
}
