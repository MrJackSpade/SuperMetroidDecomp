using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using static SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork.GameOverPageDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

/// <summary>
/// Spells the game-over page (<see cref="GameOverPageDefinitions"/>): each cell of the title, objective, prompt and the two
/// answers with their explanations, in the small and large menu fonts.
/// </summary>
internal static class GameOverText
{
    /// <summary>The tile page cell <paramref name="cell"/> shows: the blank page plus the five native game-over text elements; no stored tilemap rows.</summary>
    internal static BackgroundTile TilemapTile(int cell) => new(TilemapCharacter(cell), 0, highPriority: false);

    /// <summary>The menu atlas cell page cell <paramref name="cell"/> shows.</summary>
    private static ushort TilemapCharacter(int cell)
    {
        if ((uint)cell >= TilemapCellCount) throw new ArgumentOutOfRangeException(nameof(cell));
        foreach (GameOverTextElement element in Enum.GetValues<GameOverTextElement>())
        {
            GameOverTextStream stream = GameOverTexts.Get(element);
            int column = cell % TilemapWidth - stream.Origin.X;
            int row = cell / TilemapWidth - stream.Origin.Y;
            if (column < 0 || row is < 0 or > 1) continue;
            switch (element)
            {
                case GameOverTextElement.Title:
                    if (column < stream.Description.Length) return LargeLetter(stream.Description[column], row);
                    break;
                case GameOverTextElement.Objective:
                    if (row == 0 && column < GameOverPageDefinitionsConstants.ObjectiveText.Length) return SmallLetter(GameOverPageDefinitionsConstants.ObjectiveText[column]);
                    break;
                case GameOverTextElement.Prompt:
                    if (row == 0 && column < GameOverPageDefinitionsConstants.PromptText.Length) return SmallLetter(GameOverPageDefinitionsConstants.PromptText[column]);
                    break;
                case GameOverTextElement.ReturnToGame:
                case GameOverTextElement.ReturnToTitle:
                    string answer = element == GameOverTextElement.ReturnToGame ? "YES" : "N O";
                    if (column < answer.Length) return LargeLetter(answer[column], row);
                    string explanation = element == GameOverTextElement.ReturnToGame ? GameOverPageDefinitionsConstants.ReturnToGameText : GameOverPageDefinitionsConstants.ReturnToTitleText;
                    if (row == 1 && column - answer.Length < explanation.Length)
                        return SmallLetter(explanation[column - answer.Length]);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(GameOverTextElement)} {element}.");
            }
        }
        return BlankTile;
    }

    /// <summary>Maps a character in the native small-font text lines to its menu atlas cell.</summary>
    private static ushort SmallLetter(char letter) => letter switch
    {
        >= 'A' and <= 'Z' => (ushort)(GameOverPageDefinitionsConstants.SmallLetterA + letter - 'A'),
        ' ' => BlankTile,
        '!' => GameOverPageDefinitionsConstants.ExclamationTile,
        '?' => GameOverPageDefinitionsConstants.QuestionTile,
        '(' => GameOverPageDefinitionsConstants.OpeningParenthesisTile,
        ')' => GameOverPageDefinitionsConstants.OpeningParenthesisTile + 1,
        _ => throw new ArgumentOutOfRangeException(nameof(letter)),
    };

    /// <summary>The menu atlas cell of the upper (row zero) or lower half of a large letter.</summary>
    private static ushort LargeLetter(char letter, int row)
    {
        GameOverLargeGlyph glyph = Ensure.Listed(GameOverPageDefinitionsConstants.LargeLetters, letter);
        return checked((ushort)(row == 0 ? glyph.Top : glyph.Bottom));
    }
}
