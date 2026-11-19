using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver;

/// <summary>The game-over page's five text elements and the cells they start on.</summary>
/// <remarks>
/// Independently reviewed for #1165: the menu's initialization loads the title, objective, prompt, Yes and No
/// elements in this order. The elements start at screen columns/rows (11,5), (5,14), (10,16), (7,19) and (7,23).
/// Named screen-element cases select the text and its origin directly, without persistent record storage.
/// </remarks>
public static class GameOverTexts
{
    /// <summary>Selects one text element; unsupported selectors are rejected.</summary>
    private static GameOverTextStream Get(GameOverTextElement element) => element switch
    {
        GameOverTextElement.Title => new(element, new(11, 5), "GAME OVER"),
        GameOverTextElement.Objective => new(element, new(5, 14), "FIND THE METROID LARVA"),
        GameOverTextElement.Prompt => new(element, new(10, 16), "TRY AGAIN"),
        GameOverTextElement.ReturnToGame => new(element, new(7, 19), "YES - RETURN TO GAME"),
        GameOverTextElement.ReturnToTitle => new(element, new(7, 23), "NO - GO TO TITLE"),
        _ => throw new ArgumentOutOfRangeException(nameof(element)),
    };

    /// <summary>The five elements in the order the menu's initialization loads them (#2567): the title, objective, prompt, Yes and No.</summary>
    private static readonly IReadOnlyList<GameOverTextElement> LoadOrder = Ensure.EveryMemberOnce<GameOverTextElement>(
    [
        GameOverTextElement.Title,
        GameOverTextElement.Objective,
        GameOverTextElement.Prompt,
        GameOverTextElement.ReturnToGame,
        GameOverTextElement.ReturnToTitle,
    ]);

    /// <summary>Enumerates the five elements in their load order.</summary>
    public static IEnumerable<GameOverTextStream> All => LoadOrder.Select(Get);
}
