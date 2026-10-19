using SuperMetroid.Core.Frontend.GameOver;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>The bank-$81 pointer of each game-over text element's command stream.</summary>
internal static class GameOverTextSourceDefinitions
{
    /// <summary>Each element's stream, by element.</summary>
    internal static IReadOnlyDictionary<GameOverTextElement, ushort> Pointers { get; } = new Dictionary<GameOverTextElement, ushort>
    {
        [GameOverTextElement.Title] = 0x92dc,
        [GameOverTextElement.Objective] = 0x9304,
        [GameOverTextElement.Prompt] = 0x9334,
        [GameOverTextElement.ReturnToGame] = 0x934c,
        [GameOverTextElement.ReturnToTitle] = 0x93a0,
    };
}
