using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>
/// The shades the import pre-colours the game-over Baby in (#1512): its idle colours and each phase of its cry. Which inks
/// each recolours, and to what, is the import's.
/// </summary>
public static class GameOverBabyShades
{
    /// <summary>The Baby shown in its <paramref name="colours"/> colours.</summary>
    public static PictureShade Colours(GameOverBabyColouring colours) => new($"GameOverBaby{colours}", 0);
}
