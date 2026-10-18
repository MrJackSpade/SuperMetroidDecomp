using SuperMetroid.Core.Frontend.GameOver;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>The import pipeline's map from game-over text elements to the bank-$81 streams extraction walks.</summary>
public static class GameOverTextSources
{
    /// <summary>The bank-$81 pointer of <paramref name="element"/>'s command stream.</summary>
    public static ushort Pointer(GameOverTextElement element) =>
        GameOverTextSourceDefinitions.Pointers.TryGetValue(element, out ushort pointer)
            ? pointer
            : throw new ArgumentOutOfRangeException(nameof(element), element, "Not a game-over text element.");
}
