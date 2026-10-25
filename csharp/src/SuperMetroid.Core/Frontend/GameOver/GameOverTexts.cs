namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>The game-over page's five text elements and the cells they start on.</summary>
/// <remarks>
/// Independently reviewed for #1165: the menu's initialization loads the title, objective, prompt, Yes and No
/// elements in this order. The elements start at screen columns/rows (11,5), (5,14), (10,16), (7,19) and (7,23).
/// Named screen-element cases select the text and its origin directly, without persistent record storage.
/// </remarks>
public static class GameOverTexts
{
    /// <summary>Five bounded text elements loaded in initialization order: title, objective, prompt, Yes, and No.</summary>
    public const int Count = 5;

    /// <summary>Selects one text element; unsupported selectors are rejected.</summary>
    public static GameOverTextStream Get(GameOverTextElement element) => element switch
    {
        GameOverTextElement.Title => new(element, new(11, 5), "GAME OVER"),
        GameOverTextElement.Objective => new(element, new(5, 14), "FIND THE METROID LARVA"),
        GameOverTextElement.Prompt => new(element, new(10, 16), "TRY AGAIN"),
        GameOverTextElement.ReturnToGame => new(element, new(7, 19), "YES - RETURN TO GAME"),
        GameOverTextElement.ReturnToTitle => new(element, new(7, 23), "NO - GO TO TITLE"),
        _ => throw new ArgumentOutOfRangeException(nameof(element)),
    };

    /// <summary>Enumerates the five elements in their load order, without a stored table.</summary>
    public static IEnumerable<GameOverTextStream> All
    {
        get
        {
            foreach (GameOverTextElement element in Enum.GetValues<GameOverTextElement>())
                yield return Get(element);
        }
    }
}
