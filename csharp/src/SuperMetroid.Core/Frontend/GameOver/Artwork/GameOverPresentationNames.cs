namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>
/// Names the game-over document's Baby frames (<see cref="GameOverPresentationDefinitions"/>).
/// </summary>
public static class GameOverPresentationNames
{
    /// <summary>Maps a Baby animation frame to its authored sprite composition identity.</summary>
    /// <param name="frame">Closed, middle, or open mouth frame.</param>
    /// <returns>The corresponding <c>Baby.</c> sprite dictionary key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The frame is unsupported.</exception>
    public static string BabyFrameName(GameOverBabyFrame frame) =>
        GameOverPresentationDefinitions.BabyFrameNames[frame];
}
